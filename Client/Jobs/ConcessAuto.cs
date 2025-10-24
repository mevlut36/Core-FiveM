using CitizenFX.Core;
using ShurikenLegal.Client.data;
using ShurikenLegal.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Game = CitizenFX.Core.Game;

namespace ShurikenLegal.Client.Jobs
{
    public class ConcessAuto : Job
    {
        public Vector3 garagePosSortie = new Vector3(-33, -1103, 26);
        public Vector3 garagePosEntrer = new Vector3(-42, -1098, 26);
        public Vector3 coffreEntreprise = new Vector3(-31, -1112, 26);
        public Vector3 clothPos = new Vector3((float)-28.10, -1104, (float)25.8);
        public Vector3 Vendeur = new Vector3((float)-42.67, (float)-1092.8, (float)25.4);
        public Vector3 VendeurPosition = new Vector3(-42.67f, -1092.8f, 25.4f);
        public float VendeurHeading = 340.0f;

        private bool isUIOpen = false;

        private int previewVehicle = 0;
        private bool cameraActive = false;
        private Vector3 previewPos = new Vector3(-42, -1098, 27f);
        private int _rotateCamTimer = 0;
        private List<int> _previewCameras = new List<int>();
        private string currentVehicleModel = "";
        private int currentPrimaryColor = 0;
        private int currentSecondaryColor = 0;

        private int _vendeurPnjHandle = 0;
        private bool _isPnjActive = false;

        private bool _vehiclesInitialized = false;
        private Dictionary<string, List<string>> _availableVehicles = new Dictionary<string, List<string>>();
        private HashSet<string> _excludedVehicles = new HashSet<string>()
        {
            "OPPRESSOR", "OPPRESSOR2", "VIGILANTE", "DELUXO", "SCRAMJET",
            "INSURGENT", "INSURGENT2", "INSURGENT3", "NIGHTSHARK", "APC", "RHINO", "KHANJALI",
            "KURUMA2", "LIMO2", "BOXVILLE5", "ZHABA", "SQUADDIE", "WINKY", "TOREADOR", "STROMBERG",
            "POLICE", "POLICE2", "POLICE3", "POLICE4", "POLICEB", "POLICET", "SHERIFF", "SHERIFF2",
            "FBI", "FBI2", "FIRETRUK", "AMBULANCE", "PBUS", "PRANGER", "RIOT", "RIOT2",
            "STOCKADE", "STOCKADE3", "CARGOPLANE", "BLIMP", "BLIMP2", "BLIMP3", "BESRA"
        };

        public ConcessAuto(ClientMain caller) : base(caller)
        {
            InitializeVehicles();
            RegisterEventHandlers();
            Debug.WriteLine("[CONCESS] ConcessAuto initialisé avec succès !");
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 12,
                JobName = "Concessionnaire",
                MenuTitle = "Concessionnaire",
                HasGarage = true,
                HasChest = true,
                HasClothing = true,
                HasAnnounce = true,
                HasBilling = true,
                MinRankForAnnounce = 0
            };
        }

        private void RegisterEventHandlers()
        {
            Debug.WriteLine("[CONCESS] Enregistrement des event handlers");

            Main.AddEvent("concess:previewVehicle", new Action<string>(async (model) =>
            {
                Debug.WriteLine($"[CONCESS] Event reçu - previewVehicle: {model}");
                await StartVehiclePreview(model);
            }));

            Main.AddEvent("concess:setPrimaryColor", new Action<int>((colorId) =>
            {
                Debug.WriteLine($"[CONCESS] Event reçu - setPrimaryColor: {colorId}");
                ApplyPrimaryColor(colorId);
            }));

            Main.AddEvent("concess:setSecondaryColor", new Action<int>((colorId) =>
            {
                Debug.WriteLine($"[CONCESS] Event reçu - setSecondaryColor: {colorId}");
                ApplySecondaryColor(colorId);
            }));

            Main.AddEvent("concess:setWheels", new Action<int, int>((wheelType, wheelIndex) =>
            {
                Debug.WriteLine($"[CONCESS] Event reçu - setWheels: Type={wheelType}, Index={wheelIndex}");
                ApplyWheels(wheelType, wheelIndex);
            }));

            Main.AddEvent("concess:buyVehicle", new Action(async () =>
            {
                Debug.WriteLine("[CONCESS] Event reçu - buyVehicle");
                await BuyCurrentVehicle();
            }));

            Main.AddEvent("concess:closePreview", new Action(() =>
            {
                Debug.WriteLine("[CONCESS] Event reçu - closePreview");
                CleanupPreview();
                isUIOpen = false;
            }));

            Main.AddEvent("nui:closed", new Action(() =>
            {
                Debug.WriteLine("[CONCESS] Event reçu - nui:closed, réinitialisation");
                if (isUIOpen)
                {
                    CleanupPreview();
                    isUIOpen = false;
                }
            }));

            Debug.WriteLine("[CONCESS] Tous les event handlers enregistrés");
        }

        private void InitializeVehicles()
        {
            if (_vehiclesInitialized) return;

            Debug.WriteLine("[CONCESS] Initialisation des véhicules...");

            var knownVehicles = new Dictionary<string, List<string>>
            {
                ["compact"] = new List<string> { "BLISTA", "BRIOSO", "DILETTANTE", "ISSI2", "PANTO", "PRAIRIE", "RHAPSODY" },
                ["sedan"] = new List<string> { "ASEA", "ASTEROPE", "COG55", "COGNOSCENTI", "EMPEROR", "FUGITIVE",
                    "GLENDALE", "INGOT", "INTRUDER", "PREMIER", "PRIMO", "REGINA", "SCHAFTER2", "STANIER",
                    "STRATUM", "STRETCH", "SURGE", "TAILGATER", "WARRENER", "WASHINGTON" },
                ["suv"] = new List<string> { "BALLER", "BALLER2", "BJXL", "CAVALCADE", "CAVALCADE2", "CONTENDER",
                    "DUBSTA", "DUBSTA2", "FQ2", "GRANGER", "GRESLEY", "HABANERO", "HUNTLEY", "LANDSTALKER",
                    "MESA", "PATRIOT", "RADI", "ROCOTO", "SEMINOLE", "SERRANO", "XLS" },
                ["coupe"] = new List<string> { "COGCABRIO", "EXEMPLAR", "F620", "FELON", "FELON2", "JACKAL",
                    "ORACLE", "ORACLE2", "SENTINEL", "SENTINEL2", "WINDSOR", "WINDSOR2", "ZION", "ZION2" },
                ["muscle"] = new List<string> { "BLADE", "BUCCANEER", "BUCCANEER2", "CHINO", "CHINO2", "COQUETTE3",
                    "DOMINATOR", "DOMINATOR2", "DUKES", "DUKES2", "GAUNTLET", "GAUNTLET2", "HOTKNIFE", "FACTION",
                    "FACTION2", "MOONBEAM", "MOONBEAM2", "NIGHTSHADE", "PHOENIX", "PICADOR", "RATLOADER",
                    "RATLOADER2", "RUINER", "SABREGT", "SABREGT2", "SLAMVAN", "SLAMVAN2", "STALLION",
                    "STALLION2", "TAMPA", "VIGERO", "VIRGO", "VIRGO2", "VIRGO3", "VOODOO", "VOODOO2" },
                ["sport"] = new List<string> { "ALPHA", "BANSHEE", "BESTIAGTS", "BLISTA2", "BUFFALO", "BUFFALO2",
                    "BUFFALO3", "CARBONIZZARE", "COMET2", "COQUETTE", "ELEGY", "ELEGY2", "FELTZER2", "FUROREGT",
                    "FUSILADE", "FUTO", "JESTER", "JESTER2", "KHAMELION", "KURUMA", "LYNX", "MASSACRO",
                    "MASSACRO2", "NINEF", "NINEF2", "OMNIS", "PENUMBRA", "RAPIDGT", "RAPIDGT2", "RAPTOR",
                    "SCHAFTER3", "SCHAFTER4", "SCHAFTER5", "SCHAFTER6", "SCHWARZER", "SEVEN70", "SULTAN",
                    "SURANO", "TAMPA2", "TROPOS", "VERLIERER2" },
                ["super"] = new List<string> { "ADDER", "BANSHEE2", "BULLET", "CHEETAH", "ENTITYXF", "FMJ",
                    "INFERNUS", "OSIRIS", "LE7B", "REAPER", "SHEAVA", "SULTANRS", "T20", "TURISMOR", "TYRUS",
                    "VACCA", "VOLTIC", "ZENTORNO", "PROTOTIPO", "X80PROTO" },
                ["motorcycle"] = new List<string> { "AKUMA", "AVARUS", "BAGGER", "BATI", "BATI2", "BF400",
                    "CARBONRS", "CHIMERA", "CLIFFHANGER", "DAEMON", "DAEMON2", "DEFILER", "DOUBLE", "ENDURO",
                    "ESSKEY", "FAGGIO", "FAGGIO2", "GARGOYLE", "HAKUCHOU", "HAKUCHOU2", "HEXER", "INNOVATION",
                    "LECTRO", "MANCHEZ", "NEMESIS", "NIGHTBLADE", "PCJ", "RATBIKE", "RUFFIAN", "SANCHEZ",
                    "SANCHEZ2", "SANCTUS", "SOVEREIGN", "THRUST", "VADER", "VINDICATOR", "VORTEX", "WOLFSBANE",
                    "ZOMBIEA", "ZOMBIEB" },
                ["offroad"] = new List<string> { "BFINJECTION", "BIFTA", "BLAZER", "BLAZER2", "BLAZER3", "BLAZER4",
                    "BODHI2", "BRAWLER", "DLOADER", "DUBSTA3", "DUNE", "REBEL", "REBEL2", "SANDKING", "SANDKING2",
                    "TECHNICAL", "TROPHYTRUCK", "TROPHYTRUCK2" },
                ["industrial"] = new List<string> { "BULLDOZER", "CUTTER", "DUMP", "FLATBED", "GUARDIAN", "HANDLER",
                    "MIXER", "MIXER2", "RUBBLE", "TIPTRUCK", "TIPTRUCK2" },
                ["utility"] = new List<string> { "AIRTUG", "CADDY", "CADDY2", "CADDY3", "DOCKTUG", "FORKLIFT",
                    "MOWER", "RIPLEY", "SADLER", "SADLER2", "SCRAP", "TOWTRUCK", "TOWTRUCK2", "TRACTOR",
                    "TRACTOR2", "TRACTOR3", "UTILLITRUCK", "UTILLITRUCK2", "UTILLITRUCK3" },
                ["van"] = new List<string> { "BISON", "BISON2", "BISON3", "BOBCATXL", "BOXVILLE", "BOXVILLE2",
                    "BOXVILLE3", "BOXVILLE4", "BURRITO", "BURRITO2", "BURRITO3", "BURRITO4", "BURRITO5",
                    "CAMPER", "GBURRITO", "GBURRITO2", "JOURNEY", "MINIVAN", "MINIVAN2", "PARADISE", "PONY",
                    "PONY2", "RUMPO", "RUMPO2", "RUMPO3", "SPEEDO", "SPEEDO2", "SURFER", "SURFER2",
                    "TACO", "YOUGA", "YOUGA2" }
            };

            _availableVehicles.Clear();
            int totalCount = 0;

            foreach (var category in knownVehicles)
            {
                _availableVehicles[category.Key] = new List<string>();

                foreach (var vehicle in category.Value)
                {
                    if (!_excludedVehicles.Contains(vehicle))
                    {
                        _availableVehicles[category.Key].Add(vehicle);
                        totalCount++;
                    }
                }
            }

            _vehiclesInitialized = true;
            Debug.WriteLine($"[CONCESS] {totalCount} véhicules initialisés dans {_availableVehicles.Count} catégories");
        }

        public override void ShowMenu()
        {
            if (isUIOpen) return;

            Debug.WriteLine("[CONCESS] ========== OUVERTURE DU MENU ==========");

            if (!_vehiclesInitialized)
            {
                InitializeVehicles();
            }

            isUIOpen = true;

            var colors = VehicleColorData.GetColors();
            var wheels = VehicleWheelData.GetWheels();

            var wheelTypes = wheels.GroupBy(w => w.WheelType)
                .Select(g => new
                {
                    id = g.Key,
                    name = GetWheelTypeName(g.Key),
                    wheels = g.Select(w => new
                    {
                        id = w.vID,
                        name = w.Wheel
                    }).ToList()
                }).ToList();

            var vehicleData = new
            {
                categories = _availableVehicles.Select(cat => new
                {
                    name = cat.Key,
                    displayName = GetCategoryDisplayName(cat.Key),
                    vehicles = cat.Value.Select(v => new
                    {
                        model = v,
                        name = GetDisplayNameFromVehicleModel((uint)GetHashKey(v)),
                        price = CalculateVehiclePrice(v)
                    }).ToList()
                }).ToList(),
                colors = colors.Select(c => new
                {
                    id = int.Parse(c.ID),
                    description = c.Description,
                    hex = c.Hex,
                    rgb = c.RGB
                }).ToList(),
                wheelTypes = wheelTypes
            };

            Debug.WriteLine($"[CONCESS] Données préparées:");
            Debug.WriteLine($"[CONCESS] - {vehicleData.categories.Count} catégories");
            Debug.WriteLine($"[CONCESS] - {vehicleData.colors.Count} couleurs");
            Debug.WriteLine($"[CONCESS] - {vehicleData.wheelTypes.Count} types de roues");

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(vehicleData);
            Debug.WriteLine($"[CONCESS] JSON préparé, taille: {json.Length} caractères");

            BaseScript.TriggerEvent("core:openConcessNUI", json);

            Debug.WriteLine("[CONCESS] Événement envoyé à Core");
            Debug.WriteLine("[CONCESS] ==========================================");
        }

        private string GetWheelTypeName(int wheelType)
        {
            switch (wheelType)
            {
                case 0: return "Sport";
                case 1: return "Muscle";
                case 2: return "Lowrider";
                case 3: return "SUV";
                case 4: return "Offroad";
                case 5: return "Tuner";
                case 6: return "High End";
                case 7: return "Benny's Original";
                case 8: return "Benny's Bespoke";
                case 9: return "Open Wheel";
                case 10: return "Street";
                case 11: return "Track";
                default: return $"Type {wheelType}";
            }
        }

        private string GetCategoryDisplayName(string category)
        {
            switch (category)
            {
                case "compact": return "Compactes";
                case "sedan": return "Berlines";
                case "suv": return "SUV";
                case "coupe": return "Coupés";
                case "muscle": return "Muscle Cars";
                case "sport": return "Sportives";
                case "super": return "Super Cars";
                case "motorcycle": return "Motos";
                case "offroad": return "Tout-terrain";
                case "industrial": return "Industriels";
                case "utility": return "Utilitaires";
                case "van": return "Vans";
                default: return category;
            }
        }

        private int CalculateVehiclePrice(string model)
        {
            uint hash = (uint)GetHashKey(model);
            int vehicleClass = GetVehicleClassFromName(hash);

            switch (vehicleClass)
            {
                case 0: return 15000;
                case 1: return 25000;
                case 2: return 35000;
                case 3: return 30000;
                case 4: return 40000;
                case 5: return 60000;
                case 6: return 80000;
                case 7: return 150000;
                case 8: return 20000;
                case 9: return 45000;
                default: return 25000;
            }
        }

        private async Task StartVehiclePreview(string modelName)
        {
            try
            {
                currentVehicleModel = modelName;
                uint vehicleHash = (uint)GetHashKey(modelName);

                RequestModel(vehicleHash);
                int timeout = 0;
                while (!HasModelLoaded(vehicleHash) && timeout < 100)
                {
                    await BaseScript.Delay(50);
                    timeout++;
                }

                if (!HasModelLoaded(vehicleHash))
                {
                    Main.SendNotif("~r~Impossible de charger le véhicule.");
                    return;
                }

                CleanupPreview();

                previewVehicle = CreateVehicle(vehicleHash, previewPos.X, previewPos.Y, previewPos.Z, 180f, false, false);

                SetEntityCollision(previewVehicle, false, false);
                SetVehicleOnGroundProperly(previewVehicle);
                FreezeEntityPosition(previewVehicle, true);

                GetVehicleColours(previewVehicle, ref currentPrimaryColor, ref currentSecondaryColor);

                SetupPreviewCamera();

                SetModelAsNoLongerNeeded(vehicleHash);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CONCESS] Erreur dans StartVehiclePreview: {ex.Message}");
                Main.SendNotif("~r~Erreur lors de la prévisualisation.");
            }
        }

        private void SetupPreviewCamera()
        {
            foreach (var cam2 in _previewCameras)
            {
                SetCamActive(cam2, false);
                DestroyCam(cam2, false);
            }
            _previewCameras.Clear();

            int cam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
            Vector3 vehiclePos = GetEntityCoords(previewVehicle, true);

            SetCamCoord(cam, vehiclePos.X + 5, vehiclePos.Y + 5, vehiclePos.Z + 2);
            PointCamAtEntity(cam, previewVehicle, 0, 0, 0, true);
            SetCamActive(cam, true);
            RenderScriptCams(true, true, 1000, true, false);

            _previewCameras.Add(cam);
            cameraActive = true;

            FreezeEntityPosition(GetPlayerPed(-1), true);
            SetPlayerControl(PlayerId(), false, 0);
        }

        private void ApplyPrimaryColor(int colorId)
        {
            if (DoesEntityExist(previewVehicle))
            {
                currentPrimaryColor = colorId;
                SetVehicleColours(previewVehicle, currentPrimaryColor, currentSecondaryColor);
            }
        }

        private void ApplySecondaryColor(int colorId)
        {
            if (DoesEntityExist(previewVehicle))
            {
                currentSecondaryColor = colorId;
                SetVehicleColours(previewVehicle, currentPrimaryColor, currentSecondaryColor);
            }
        }

        private void ApplyWheels(int wheelType, int wheelIndex)
        {
            if (DoesEntityExist(previewVehicle))
            {
                SetVehicleWheelType(previewVehicle, wheelType);
                SetVehicleMod(previewVehicle, 23, wheelIndex, false);

                if (GetNumVehicleMods(previewVehicle, 24) > 0)
                {
                    SetVehicleMod(previewVehicle, 24, wheelIndex, false);
                }
            }
        }

        private async Task BuyCurrentVehicle()
        {
            if (!DoesEntityExist(previewVehicle))
            {
                Main.SendNotif("~r~Aucun véhicule en prévisualisation.");
                return;
            }

            int price = CalculateVehiclePrice(currentVehicleModel);

            BaseScript.TriggerServerEvent("concess:buyVehicle", currentVehicleModel, currentPrimaryColor, currentSecondaryColor, price);

            CleanupPreview();

            BaseScript.TriggerEvent("core:closeNUI");
            isUIOpen = false;
        }

        private void CleanupPreview()
        {
            foreach (var cam in _previewCameras)
            {
                SetCamActive(cam, false);
                DestroyCam(cam, false);
            }
            _previewCameras.Clear();

            RenderScriptCams(false, false, 0, true, false);

            FreezeEntityPosition(GetPlayerPed(-1), false);
            SetPlayerControl(PlayerId(), true, 0);
            EnableAllControlActions(0);

            if (DoesEntityExist(previewVehicle))
            {
                DeleteEntity(ref previewVehicle);
                previewVehicle = 0;
            }

            cameraActive = false;
            currentVehicleModel = "";
        }

        public void OnTick()
        {
            if (cameraActive && DoesEntityExist(previewVehicle))
            {
                if (_rotateCamTimer % 300 == 0)
                {
                    float vehHeading = GetEntityHeading(previewVehicle);
                    SetEntityHeading(previewVehicle, (vehHeading + 90) % 360);
                }
                _rotateCamTimer++;
            }

            CheckPnjPresence();
        }

        private void CheckPnjPresence()
        {
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            float distance = GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z,
                VendeurPosition.X, VendeurPosition.Y, VendeurPosition.Z, false);

            if (distance < 50f && !_isPnjActive)
            {
                CreateVendeurPnj();
            }
            else if (distance >= 50f && _isPnjActive)
            {
                DeleteVendeurPnj();
            }
        }

        private void CreateVendeurPnj()
        {
            if (_isPnjActive) return;

            uint pedHash = (uint)GetHashKey("s_m_m_autoshop_01");
            RequestModel(pedHash);

            int timeout = 0;
            while (!HasModelLoaded(pedHash) && timeout < 100)
            {
                BaseScript.Delay(50).Wait();
                timeout++;
            }

            if (HasModelLoaded(pedHash))
            {
                _vendeurPnjHandle = CreatePed(4, pedHash, VendeurPosition.X, VendeurPosition.Y, VendeurPosition.Z, VendeurHeading, false, true);
                SetEntityInvincible(_vendeurPnjHandle, true);
                FreezeEntityPosition(_vendeurPnjHandle, true);
                SetBlockingOfNonTemporaryEvents(_vendeurPnjHandle, true);
                _isPnjActive = true;
                SetModelAsNoLongerNeeded(pedHash);
            }
        }

        private void DeleteVendeurPnj()
        {
            if (_isPnjActive && DoesEntityExist(_vendeurPnjHandle))
            {
                DeleteEntity(ref _vendeurPnjHandle);
                _vendeurPnjHandle = 0;
                _isPnjActive = false;
            }
        }
    }
}