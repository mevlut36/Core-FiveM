using CitizenFX.Core;
using LemonUI.Menus;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static CitizenFX.Core.Native.API;
using ShurikenLegal.Shared;

namespace ShurikenLegal.Client.Jobs
{
    enum eVehicleModType
    {
        VMT_SPOILER = 0,
        VMT_BUMPER_F = 1,
        VMT_BUMPER_R = 2,
        VMT_SKIRT = 3,
        VMT_EXHAUST = 4,
        VMT_CHASSIS = 5,
        VMT_GRILL = 6,
        VMT_BONNET = 7,
        VMT_WING_L = 8,
        VMT_WING_R = 9,
        VMT_ROOF = 10,
        VMT_ENGINE = 11,
        VMT_BRAKES = 12,
        VMT_GEARBOX = 13,
        VMT_HORN = 14,
        VMT_SUSPENSION = 15,
        VMT_ARMOUR = 16,
        VMT_NITROUS = 17,
        VMT_TURBO = 18,
        VMT_SUBWOOFER = 19,
        VMT_TYRE_SMOKE = 20,
        VMT_HYDRAULICS = 21,
        VMT_XENON_LIGHTS = 22,
        VMT_WHEELS = 23,
        VMT_WHEELS_REAR_OR_HYDRAULICS = 24,
        VMT_PLTHOLDER = 25,
        VMT_PLTVANITY = 26,
        VMT_INTERIOR1 = 27,
        VMT_INTERIOR2 = 28,
        VMT_INTERIOR3 = 29,
        VMT_INTERIOR4 = 30,
        VMT_INTERIOR5 = 31,
        VMT_SEATS = 32,
        VMT_STEERING = 33,
        VMT_KNOB = 34,
        VMT_PLAQUE = 35,
        VMT_ICE = 36,
        VMT_TRUNK = 37,
        VMT_HYDRO = 38,
        VMT_ENGINEBAY1 = 39,
        VMT_ENGINEBAY2 = 40,
        VMT_ENGINEBAY3 = 41,
        VMT_CHASSIS2 = 42,
        VMT_CHASSIS3 = 43,
        VMT_CHASSIS4 = 44,
        VMT_CHASSIS5 = 45,
        VMT_DOOR_L = 46,
        VMT_DOOR_R = 47,
        VMT_LIVERY_MOD = 48,
        VMT_LIGHTBAR = 49,
    };
    public class Bennys : Job
    {
        public ClientMain Client;
        public Vector3 JobZone = new Vector3(-212, -1327, 30);
        public List<VehColors> VehicleColors;
        public List<VehWheels> VehicleWheels;

        public Vector3 garagePosSortie = new Vector3(-191.8f, -1315, 32);
        public Vector3 garagePosEntrer = new Vector3(-180, -1286, 32);

        public Vector3 coffreEntreprise = new Vector3((float)-206.9, (float)-1331.3, 35);
        public Vector3 clothPos = new Vector3(-207, (float)-1341.6, (float)34.2);

        private Dictionary<List<int>, int> xenon_dict = new Dictionary<List<int>, int>()
        {
            { new List<int>() { 235, 235, 235 }, -1 },
            { new List<int>() { 211, 234, 252 }, 0 },
            { new List<int>() { 0, 122, 219 }, 1 },
            { new List<int>() { 129, 199, 255 }, 2 },
            { new List<int>() { 51, 255, 91 }, 3 },
            { new List<int>() { 131, 255, 0 }, 4 },
            { new List<int>() { 255, 251, 0 }, 5 },
            { new List<int>() { 255, 228, 0 }, 6 },
            { new List<int>() { 255, 193, 0 }, 7 },
            { new List<int>() { 223, 0, 0 }, 8 },
            { new List<int>() { 245, 0, 182 }, 9 },
            { new List<int>() { 255, 0, 251 }, 10 },
            { new List<int>() { 255, 0, 251 }, 11 },
            { new List<int>() { 54, 0, 255 }, 12 }
        };

        VehicleInfo VehInfo = new VehicleInfo();

        public Bennys(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
            BaseScript.TriggerServerEvent("legal_server:vehicle_colors");
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 3,
                JobName = "bennys",
                MenuTitle = "Benny's",
                PosCoffreEntreprise = coffreEntreprise,
                PosCloth = clothPos,
                PosGarageSortie = garagePosSortie,
                PosGarageEntrer = garagePosEntrer,
                Outfits = new Dictionary<string, ClothingSet>
                {
                    ["Tenue de mécanicien"] = new ClothingSet
                    {
                        Components = new Dictionary<int, ComponentVariation>
                        {
                            [8] = new ComponentVariation { ComponentId = 8, DrawableId = 242, TextureId = 0 },
                            [11] = new ComponentVariation { ComponentId = 11, DrawableId = 55, TextureId = 0 },
                            [4] = new ComponentVariation { ComponentId = 4, DrawableId = 15, TextureId = 0 },
                            [6] = new ComponentVariation { ComponentId = 6, DrawableId = 129, TextureId = 0 },
                            [3] = new ComponentVariation { ComponentId = 3, DrawableId = 0, TextureId = 0 }
                        }
                    },
                },
                Doors = new List<DoorClass>
                {
                    new DoorClass { DoorHash = 1, ModelHash = (uint)GetHashKey("lr_prop_supermod_door01"), Coordinates = new Vector3(-205.6f, -1310.683f, 34.24021f) }
                },
                HasAnnounce = true,
                HasBilling = true,
                HasRecruitment = true,
                HasGarage = true,
                HasChest = true,
                HasClothing = true,
                HasDoors = true,
                AvailableVehicles = new List<string>
                {
                    "towtruck",
                    "flatbed"
                }
            };
        }

        public Dictionary<string, List<int>> GetNeonColor()
        {
            var neon_color_dict = new Dictionary<string, List<int>>
            {
                { "Blanc", new List<int>() { 222, 222, 255 } },
                { "Bleu", new List<int>() { 2, 21, 255 } },
                { "Bleu électrique", new List<int>() { 3, 83, 255 } },
                { "Vert menthe", new List<int>() { 0, 255, 140 } },
                { "Vert citron", new List<int>() { 94, 255, 1 } },
                { "Jaune", new List<int>() { 255, 255, 0 } },
                { "Or", new List<int>() { 255, 150, 0 } },
                { "Orange", new List<int>() { 255, 62, 0 } },
                { "Rouge", new List<int>() { 255, 1, 1 } },
                { "Rose", new List<int>() { 255, 50, 100 } },
                { "Rose vif", new List<int>() { 255, 5, 190 } },
                { "Noir", new List<int>() { 15, 3, 255 } }
            };
            return neon_color_dict;
        }

        public override void Cloth()
        {
            base.Cloth();
        }

        public override void Coffre()
        {
            base.Coffre();
        }

        public override void Garage()
        {
            base.Garage();
        }

        public override void Ticked()
        {
            base.Ticked();
            IsVehicleInJobZone();
        }

        public override void ShowMenu()
        {
            var handle = GetVehiclePedIsIn(GetPlayerPed(-1), false);
            Vehicle veh = new Vehicle(handle);
            var notInServiceMenu = new NativeMenu("Bennys", "Menu mécano");
            Pool.Add(notInServiceMenu);
            var prendreService = new NativeCheckboxItem("Prendre son service");
            notInServiceMenu.Add(prendreService);

            var inServiceMenu = new NativeMenu("Bennys", "Menu mécano");
            Pool.Add(inServiceMenu);
            var finService = new NativeCheckboxItem("Arrêter son service");
            inServiceMenu.Add(finService);

            if (en_service)
            {
                inServiceMenu.Visible = true;
            }
            else
            {
                notInServiceMenu.Visible = true;
            }

            var carMenu = new NativeMenu("Bennys", "Intéraction véhicules");
            Pool.Add(carMenu);

            var reparer = new NativeItem("Réparer le véhicule", "", "~g~$500");
            carMenu.Add(reparer);

            var performances = new NativeMenu("Bennys", "Performances");
            Pool.Add(performances);
            carMenu.AddSubMenu(performances);

            var esthetique = new NativeMenu("Bennys", "Esthétique");
            Pool.Add(esthetique);
            carMenu.AddSubMenu(esthetique);

            // FULL CUSTOM
            var full_custom = new NativeItem("Amélioration max.", "Améliorer le véhicule à son plein potentiel !");
            performances.Add(full_custom);

            // UPGRADE ENGINE
            var moteur = new NativeListItem<int>("Changer le moteur", 1, 2, 3, 4);
            performances.Add(moteur);

            // UPGRADE BRAKES
            var brakes = new NativeListItem<int>("Changer les freins", 1, 2, 3, 4);
            performances.Add(brakes);

            // UPGRADE SUSPENSION
            var suspension = new NativeListItem<int>("Changer les suspensions", 1, 2, 3);
            performances.Add(suspension);

            var motif = new NativeListItem<int>("Motif", 0, 1, 2, 3, 4, 5, 6, 7, 8);
            esthetique.Add(motif);
            motif.ItemChanged += (sender, e) =>
            {
                SetVehicleMod(veh.Handle, 48, e.Index, false);
                SetVehicleModKit(veh.Handle, 0);
                ToggleVehicleMod(veh.Handle, 48, true);
                VehInfo.LiveryMod = e.Index;
            };

            // COULEUR DU VEHICULE
            var paintTypeDict = new Dictionary<int, string>()
            {
                {0, "Normal" },
                {1, "Métalique" },
                {2, "Nacrage" },
                {3, "Mat" },
                {4, "Métal" },
                {5, "Chrome" }
            };
            var paintType1 = new NativeListItem<string>("Type de peinture primaire", paintTypeDict.Values.ToArray());
            var paintType2 = new NativeListItem<string>("Type de peinture secondaire", paintTypeDict.Values.ToArray());
            var paintsType1 = new NativeColorPanel("Couleur primaire", ColorNatives().ToArray());
            var paintsType2 = new NativeColorPanel("Couleur secondaire", ColorNatives().ToArray());
            paintType1.Panel = paintsType1;
            esthetique.Add(paintType1);
            esthetique.Add(paintType2);
            var couleur = new NativeItem("Couleur principale");
            var couleurs = new NativeColorPanel("Couleur principale", ColorNatives().ToArray());
            couleur.Panel = couleurs;
            esthetique.Add(couleur);

            var couleur_scnd = new NativeItem("Couleur secondaire");
            var couleurs_scnd = new NativeColorPanel("Couleur secondaire", ColorNatives().ToArray());
            couleur_scnd.Panel = couleurs_scnd;
            esthetique.Add(couleur_scnd);

            // CAPOT
            var bonnet = new NativeListItem<int>("Capot", 0, 1, 2, 3, 4, 5, 6);
            esthetique.Add(bonnet);

            // PARE-CHOCS
            var bumpers = new NativeMenu("Pare-chocs", "Pare-chocs");
            bumpers.UseMouse = false;
            Pool.Add(bumpers);
            esthetique.AddSubMenu(bumpers);
            var bumper_f = new NativeListItem<int>("Pare-chocs avant", 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            bumpers.Add(bumper_f);
            var bumper_r = new NativeListItem<int>("Pare-chocs arrière", 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            bumpers.Add(bumper_r);

            // Aileron
            var aileron_type = new NativeListItem<int>("Mettre un aileron", 0, 1, 2, 3, 4);
            esthetique.Add(aileron_type);

            // GLASSES
            var glasses = new NativeListItem<int>("Changer les vitres", 0, 1, 2, 3, 4, 5, 6);
            esthetique.Add(glasses);

            // PHARE DU VEHICULE
            var phare = new NativeItem("Phare");
            var xenon2 = new NativeColorPanel("Couleur des phares", XenonNatives().ToArray());
            phare.Panel = xenon2;
            esthetique.Add(phare);

            // NEON KIT
            var neon_kit = new NativeListItem<int>("Kit de néons", 0, 1, 2, 3, 4);
            esthetique.Add(neon_kit);

            // NEON KIT COLOR TODO
            var neon_kit_color = new NativeListItem<string>("Couleur kit de néons", "Changer la couleur des kits de néons", GetNeonColor().Keys.ToArray());
            esthetique.Add(neon_kit_color);
            neon_kit_color.ItemChanged += (sender, e) =>
            {
                var neon = GetNeonColor()[neon_kit_color.SelectedItem];
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleNeonLightsColour(veh.Handle, neon[0], neon[1], neon[2]);
                        VehInfo.LightBar = neon[0];
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            var chassis_bot = new NativeListItem<int>("Chassis bas", 1, 2, 3, 4);
            esthetique.Add(chassis_bot);
            chassis_bot.ItemChanged += (sender, e) =>
            {
                SetVehicleMod(veh.Handle, 3, e.Index, false);
                SetVehicleModKit(veh.Handle, 0);
                ToggleVehicleMod(veh.Handle, 3, true);
                VehInfo.Skirt = e.Index;
            };

            var chassis_phare1 = new NativeListItem<int>("Chassis phare (1)", 1, 2, 3, 4);
            esthetique.Add(chassis_phare1);
            chassis_phare1.ItemChanged += (sender, e) =>
            {
                SetVehicleMod(veh.Handle, 42, e.Index, false);
                SetVehicleModKit(veh.Handle, 0);
                ToggleVehicleMod(veh.Handle, 42, true);
                VehInfo.Chassis2 = e.Index;
            };
            var chassis_phare2 = new NativeListItem<int>("Chassis phares (2)", 1, 2, 3, 4);
            esthetique.Add(chassis_phare2);
            chassis_phare2.ItemChanged += (sender, e) =>
            {
                SetVehicleMod(veh.Handle, 43, e.Index, false);
                SetVehicleModKit(veh.Handle, 0);
                ToggleVehicleMod(veh.Handle, 43, true);
                VehInfo.Chassis3 = e.Index;
            };
            var chassis_top = new NativeListItem<int>("Chassis toit", 1, 2, 3, 4);
            esthetique.Add(chassis_top);
            chassis_top.ItemChanged += (sender, e) =>
            {
                SetVehicleMod(veh.Handle, 44, e.Index, false);
                SetVehicleModKit(veh.Handle, 0);
                ToggleVehicleMod(veh.Handle, 44, true);
                VehInfo.Chassis4 = e.Index;
            };

            // WHEELS
            var wheels = new NativeMenu("Roues", "Roues");
            Pool.Add(wheels);
            var wheel_list = new List<int>();
            for (int i = 0; i < 51; i++)
            {
                wheel_list.Add(i);
            }
            var wheel_item = new NativeListItem<int>("Roues", "Liste de toutes les roues disponibles", wheel_list.ToArray());
            wheels.Add(wheel_item);
            wheel_item.ItemChanged += (sender, e) =>
            {
                SetVehicleMod(veh.Handle, 23, e.Index, false);
                SetVehicleModKit(veh.Handle, 0);
                ToggleVehicleMod(veh.Handle, 23, true);
                VehInfo.Wheels = e.Index;
            };

            // ROOF
            var roof = new NativeListItem<int>("Toit", "Changer le toit de la voiture", 0, 1, 2, 3, 4, 5, 6, 7, 8);
            esthetique.Add(roof);

            // KLAXON
            var klaxon = new NativeListItem<int>("Klaxon", "Ajoute un klakon", 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28);
            esthetique.Add(klaxon);

            // POT D'ECHAPPEMENT
            var exhaust = new NativeListItem<int>("Pot d'échappement", 0, 1, 2, 3, 4, 5, 6);
            esthetique.Add(exhaust);

            var updateCar = new NativeItem("Mettre à jour le véhicule", "Met à jour les modifications du véhicule au joueur le plus proche");
            inServiceMenu.Add(updateCar);

            esthetique.AddSubMenu(wheels);
            inServiceMenu.AddSubMenu(carMenu);

            inServiceMenu.UseMouse = false;
            notInServiceMenu.UseMouse = false;
            carMenu.UseMouse = false;
            performances.UseMouse = false;
            esthetique.UseMouse = false;
            wheels.UseMouse = false;

            if (en_service == true)
            {
                notInServiceMenu.Visible = false;
                inServiceMenu.Visible = true;
            }
            else if (en_service == false)
            {
                notInServiceMenu.Visible = true;
                inServiceMenu.Visible = false;
            }
            finService.CheckboxChanged += (sender, e) =>
            {
                en_service = false;
                inServiceMenu.Visible = false;
                notInServiceMenu.Visible = true;
            };
            prendreService.CheckboxChanged += (sender, e) =>
            {
                notInServiceMenu.Visible = false;
                inServiceMenu.Visible = true;
                en_service = true;
            };

            updateCar.Activated += (sender, e) =>
            {
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                var withoutMe = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                var closestPed = World.GetClosest(GetEntityCoords(GetPlayerPed(-1), true), withoutMe.ToArray());
                if (GetDistanceBetweenCoords(closestPed.Position.X, closestPed.Position.Y, closestPed.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                {
                    BaseScript.TriggerServerEvent("legal_server:updateCar", GetPlayerServerId(NetworkGetPlayerIndexFromPed(closestPed.Handle)), JsonConvert.SerializeObject(VehInfo));
                }
            };

            // <--  MENU VOITURE  --> //
            paintType1.Activated += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        var paintType = paintType1.SelectedIndex;
                        int paintIndex = paintType1.SelectedIndex;

                        SetVehicleModColor_1(veh.Handle, paintsType1.SelectedIndex, paintIndex, paintIndex);
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };
            couleur.Activated += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        VehInfo.ColorPrimary = couleurs.SelectedIndex;
                        veh.Mods.PrimaryColor = (VehicleColor)couleurs.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };
            couleur_scnd.Activated += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        VehInfo.ColorSecondary = couleurs_scnd.SelectedIndex;
                        veh.Mods.SecondaryColor = (VehicleColor)couleurs_scnd.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // PHARE COLOR ITEM PANEL
            phare.Activated += (sender, e) =>
            {
                xenon2.SelectedColor.ToString();
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        ToggleVehicleMod(veh.Handle, 22, true);
                        SetVehicleXenonLightsColor(veh.Handle, xenon_dict.First(kv => kv.Key[0] == xenon2.SelectedColor.R && kv.Key[1] == xenon2.SelectedColor.G && kv.Key[2] == xenon2.SelectedColor.B).Value);
                        VehInfo.XenonLights = xenon2.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // CAR REPAIR
            reparer.Activated += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    SetVehicleFixed(veh.Handle);
                }
            };

            // FULL CUSTOM
            full_custom.Activated += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 17, 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 17, true);

                        SetVehicleMod(veh.Handle, 11, 4, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 11, true);

                        SetVehicleMod(veh.Handle, 12, 4, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 12, true);

                        SetVehicleMod(veh.Handle, 15, 3, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 15, true);
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // UPGRADE ENGINE
            moteur.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 11, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 11, true);
                        VehInfo.Engine = moteur.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // UPGRADE BRAKES
            brakes.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 12, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 12, true);
                        VehInfo.BrakeLevel = brakes.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // UPGRADE SUSPENSION
            suspension.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 15, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 15, true);
                        VehInfo.Suspension = suspension.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // SPOILER
            aileron_type.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 0, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 0, true);
                        VehInfo.Spoiler = aileron_type.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // GLASSES
            glasses.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleWindowTint(veh.Handle, e.Index - 1);
                        SetVehicleMod(veh.Handle, 36, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 36, true);
                        VehInfo.Ice = glasses.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // NEON KIT
            neon_kit.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleNeonLightEnabled(veh.Handle, e.Index - 1, true);
                        SetVehicleMod(veh.Handle, 49, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 49, true);
                        if (e.Index == 0)
                        {
                            DisableVehicleNeonLights(veh.Handle, true);
                        }
                        else
                        {
                            DisableVehicleNeonLights(veh.Handle, false);
                        }
                        VehInfo.LightBar = neon_kit.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // KLAXON
            klaxon.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 14, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 14, true);
                        VehInfo.Horn = klaxon.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // ROOF
            roof.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 10, e.Index - 1, false);
                        SetVehicleRoofLivery(veh.Handle, e.Index - 1);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 10, true);
                        VehInfo.Roof = roof.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // EXHAUST
            exhaust.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 4, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 4, true);
                        VehInfo.Exhaust = exhaust.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // PARE-CHOCS
            bumper_f.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 1, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 1, true);
                        VehInfo.Bumber_F = bumper_f.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };
            bumper_r.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 2, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 2, true);
                        VehInfo.Bumber_R = bumper_r.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };

            // CAPOT
            bonnet.ItemChanged += (sender, e) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) == true)
                {
                    if (IsVehicleInJobZone() == true)
                    {
                        SetVehicleMod(veh.Handle, 7, e.Index - 1, false);
                        SetVehicleModKit(veh.Handle, 0);
                        ToggleVehicleMod(veh.Handle, 7, true);
                        VehInfo.Bonnet = bonnet.SelectedIndex;
                    }
                    else
                    {
                        Client.SendNotif("Vous devez être sur la zone de travail pour modifier ce véhicule.");
                    }
                }
            };
        }

        public bool IsVehicleInJobZone()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var distance = GetDistanceBetweenCoords(JobZone.X, JobZone.Y, JobZone.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);
            if (distance < 8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<NativeColorData> XenonNatives()
        {
            List<NativeColorData> colornat = new List<NativeColorData>();
            foreach (var color in xenon_dict)
                colornat.Add(new NativeColorData(color.Value.ToString(), System.Drawing.Color.FromArgb(color.Key[0], color.Key[1], color.Key[2])));
            return colornat;
        }
        public List<NativeColorData> ColorNatives()
        {
            List<NativeColorData> colornat = new List<NativeColorData>();
            foreach (var color in VehicleColors) // LE PROBLEME VIENT DE VEHICLECOLORS
            {
                colornat.Add(new NativeColorData(color.Description, color.RGB));
            }
            
            return colornat;
        }
        public List<string> WheelsNatives()
        {
            List<string> wheels = new List<string>();
            foreach (var wheel in VehicleWheels)
                wheels.Add(wheel.Wheel);
            return wheels;
        }
    }

    public class VehicleColors
    {
        public List<VehColors> Colors;
    }

    public class VehColors
    {
        [JsonProperty("ID")]
        public int Id { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Hex")]
        public string Hex { get; set; }

        [JsonProperty("RGB")]
        public string Raw_RGB { get; set; }

        List<int> colors = new List<int>();
        [JsonIgnore]
        public Color RGB
        {
            get
            {
                var raw = Raw_RGB.Replace(" ", "");
                var colors_string = raw.Split(',');
                foreach (var cs in colors_string)
                {
                    var color = int.Parse(cs);
                    colors.Add(color);
                }
                return Color.FromArgb(255, colors[0], colors[1], colors[2]);
            }
        }
    }

    public class WheelsList
    {
        public List<VehWheels> Wheels;
    }

    public class VehWheels
    {
        [JsonProperty("WheelType")]
        public int WheelType { get; set; }
        [JsonProperty("Wheel")]
        public string Wheel { get; set; }
        [JsonProperty("vID")]
        public int VID { get; set; }
    }
}
