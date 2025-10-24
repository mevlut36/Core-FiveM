using CitizenFX.Core;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;
using ShurikenLegal.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static ShurikenLegal.Client.ClientMain;

namespace ShurikenLegal.Client.Jobs
{
    public abstract class Job
    {
        public ObjectPool Pool;
        public bool en_service = false;
        public readonly ClientMain Main;
        protected JobConfig Config;

        public Job(ClientMain caller)
        {
            Pool = caller.Pool;
            Main = caller;
            Config = GetJobConfig();
            InitializeDoors();
        }

        protected abstract JobConfig GetJobConfig();

        public virtual void ShowMenu()
        {
            var job = JsonConvert.DeserializeObject<JobInfo>(Main.PlayerInst.Job);
            var menu = new NativeMenu(Config.MenuTitle, "Menu intéraction")
            {
                TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                Visible = true,
                UseMouse = false,
            };
            Pool.Add(menu);

            // Annonce
            if (Config.HasAnnounce && job.JobRank >= Config.MinRankForAnnounce)
            {
                AddAnnounceItem(menu);
            }

            // Facturation
            if (Config.HasBilling)
            {
                AddBillingItem(menu);
            }

            // Recrutement
            if (Config.HasRecruitment && job.JobRank >= Config.MinRankForRecruitment)
            {
                AddRecruitmentItem(menu, job);
            }

            // Quitter le métier
            AddLeaveJobItem(menu);

            // Items personnalisés par métier
            AddCustomMenuItems(menu, job);
        }

        protected virtual void AddCustomMenuItems(NativeMenu menu, JobInfo job)
        {
            
        }

        protected void AddAnnounceItem(NativeMenu menu)
        {
            var announce = new NativeItem("Faire une annonce");
            menu.Add(announce);
            announce.Activated += async (sender, e) =>
            {
                var input = await GetUserInput("Texte", "", 255);
                if (!string.IsNullOrEmpty(input))
                {
                    BaseScript.TriggerServerEvent("core:sendAnnounce", Config.JobName, input);
                }
            };
        }

        protected void AddBillingItem(NativeMenu menu)
        {
            var facture = new NativeItem("Rédiger une facture", "Approchez vous du joueur");
            menu.Add(facture);
            facture.Activated += async (sender, e) =>
            {
                var textInput = await GetUserInput("Montant", "", 20);
                if (int.TryParse(textInput, out int amount))
                {
                    Main.SendBill(Config.JobName, amount, Main.GetPlayer().Name);
                }
            };
        }

        protected void AddRecruitmentItem(NativeMenu menu, JobInfo job)
        {
            var recrute = new NativeItem("Recruter un joueur", "Approchez vous du joueur");
            menu.Add(recrute);
            recrute.Activated += async (sender, e) =>
            {
                var textInput = await GetUserInput("Quel rang (1-5)", "", 1);
                if (int.TryParse(textInput, out int rank))
                {
                    var player = GetPlayerPed(-1);
                    var playerCoords = GetEntityCoords(player, true);
                    var withoutMe = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                    var playerTarget = World.GetClosest(playerCoords, withoutMe.ToArray());

                    if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y,
                        playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                    {
                        BaseScript.TriggerServerEvent("legal_server:recruit",
                            Config.JobId, GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)));
                    }
                }
            };
        }

        protected void AddLeaveJobItem(NativeMenu menu)
        {
            var leave = new NativeItem("Quitter son métier");
            menu.Add(leave);
            leave.Activated += async (sender, e) =>
            {
                var input = await GetUserInput("Êtes vous sûr ? (Y/N)", "N", 1);
                if (input?.ToUpper() == "Y")
                {
                    BaseScript.TriggerServerEvent("legal_server:setJob", GetPlayerServerId(PlayerId()), 0, 0);
                    menu.Visible = false;
                    Pool.Remove(menu);
                }
            };
        }

        public virtual void Cloth()
        {
            if (!Config.HasClothing) return;

            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            float dist = Config.PosCloth.DistanceToSquared(playerCoords);

            if (dist < 8)
            {
                World.DrawMarker(MarkerType.VerticalCylinder, Config.PosCloth, Vector3.Zero,
                    Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(224, 50, 50),
                    bobUpAndDown: true, rotateY: true);
            }

            if (dist < 2)
            {
                Main.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir votre casier");
                if (IsControlJustPressed(0, 38))
                {
                    ShowClothingMenu();
                }
            }
        }

        protected virtual void ShowClothingMenu()
        {
            var menu = new NativeMenu(Config.JobName, "Tenues de service")
            {
                UseMouse = false
            };

            foreach (var outfit in Config.Outfits)
            {
                var item = new NativeItem(outfit.Key);
                item.Activated += (sender, e) => ApplyOutfit(outfit.Value);
                menu.Add(item);
            }

            Pool.Add(menu);
            menu.Visible = true;
        }

        protected void ApplyOutfit(ClothingSet outfit)
        {
            foreach (var component in outfit.Components)
            {
                var comp = component.Value;
                SetPedComponentVariation(GetPlayerPed(-1), comp.ComponentId,
                    comp.DrawableId, comp.TextureId, comp.PaletteId);
            }
        }

        public virtual void Coffre()
        {
            if (!Config.HasChest) return;

            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            float dist = Config.PosCoffreEntreprise.DistanceToSquared(playerCoords);

            if (dist < 8)
            {
                World.DrawMarker(MarkerType.ChevronUpx1, Config.PosCoffreEntreprise,
                    Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1),
                    System.Drawing.Color.FromArgb(224, 50, 50), bobUpAndDown: true, rotateY: true);
            }

            if (dist < 2)
            {
                Main.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir le coffre");
                if (IsControlPressed(0, 38))
                {
                    ShowChestMenu();
                }
            }
        }

        protected virtual void ShowChestMenu()
        {
            var menu = new NativeMenu($"Coffre {Config.JobName}", "Gestion du coffre")
            {
                UseMouse = false
            };
            Pool.Add(menu);

            var pickMenu = new NativeMenu("Retirer", "Retirer des objets")
            {
                UseMouse = false,
                TitleFont = CitizenFX.Core.UI.Font.ChaletLondon
            };
            Pool.Add(pickMenu);
            menu.AddSubMenu(pickMenu);

            var dropMenu = new NativeMenu("Déposer", "Déposer des objets")
            {
                UseMouse = false,
                TitleFont = CitizenFX.Core.UI.Font.ChaletLondon
            };
            Pool.Add(dropMenu);
            menu.AddSubMenu(dropMenu);

            var companyChest = Main.CompanyInst.Chest;
            if (companyChest != null)
            {
                foreach (var chestItem in companyChest)
                {
                    if (chestItem == null || string.IsNullOrEmpty(chestItem.Item)) continue;

                    var item = new NativeItem($"{chestItem.Item} ({chestItem.Quantity})");
                    pickMenu.Add(item);

                    item.Activated += async (sender, e) =>
                    {
                        var textInput = await GetUserInput("Quantité", "1", 4);

                        if (int.TryParse(textInput, out int quantity) && quantity > 0)
                        {
                            if (quantity <= chestItem.Quantity)
                            {
                                BaseScript.TriggerServerEvent("legal_server:getItemFromCompany",
                                    JsonConvert.DeserializeObject<JobInfo>(Main.PlayerInst.Job).JobID,
                                    chestItem.Item,
                                    quantity,
                                    chestItem.Type);

                                Main.SendNotif($"~g~Vous avez retiré {quantity} {chestItem.Item}");
                            }
                            else
                            {
                                Main.SendNotif("~r~Quantité insuffisante dans le coffre");
                            }
                        }
                        else
                        {
                            Main.SendNotif("~r~Quantité invalide");
                        }

                        pickMenu.Visible = false;
                    };
                }
            }

            var playerInventory = Main.PlayerInst.Inventory;
            if (playerInventory != null)
            {
                foreach (var inventoryItem in playerInventory)
                {
                    if (inventoryItem == null || string.IsNullOrEmpty(inventoryItem.Item) || inventoryItem.Quantity <= 0) continue;

                    var item = new NativeItem($"{inventoryItem.Item} ({inventoryItem.Quantity})");
                    dropMenu.Add(item);

                    item.Activated += async (sender, e) =>
                    {
                        var textInput = await GetUserInput("Quantité", "1", 4);

                        if (int.TryParse(textInput, out int quantity) && quantity > 0)
                        {
                            if (quantity <= inventoryItem.Quantity)
                            {
                                BaseScript.TriggerServerEvent("legal_server:setItemInCompany",
                                    JsonConvert.DeserializeObject<JobInfo>(Main.PlayerInst.Job).JobID,
                                    inventoryItem.Item,
                                    quantity,
                                    inventoryItem.Type);

                                Main.SendNotif($"~g~Vous avez déposé {quantity} {inventoryItem.Item}");
                            }
                            else
                            {
                                Main.SendNotif("~r~Vous n'avez pas assez de cet objet");
                            }
                        }
                        else
                        {
                            Main.SendNotif("~r~Quantité invalide");
                        }

                        dropMenu.Visible = false;
                    };
                }
            }

            var refreshButton = new NativeItem("Rafraîchir");
            menu.Add(refreshButton);
            refreshButton.Activated += (sender, e) =>
            {
                BaseScript.TriggerServerEvent("legal_server:requestCompanyData", JsonConvert.DeserializeObject<JobInfo>(Main.PlayerInst.Job).JobID);
                menu.Visible = false;
                ShowChestMenu();
            };

            menu.Visible = true;
        }

        public virtual void Garage()
        {
            if (!Config.HasGarage) return;

            var playerCoords = GetEntityCoords(PlayerPedId(), false);

            HandleGarageExit(playerCoords);

            HandleGarageEntry(playerCoords);
        }

        protected virtual void HandleGarageExit(Vector3 playerCoords)
        {
            float distSortie = Config.PosGarageSortie.DistanceToSquared(playerCoords);

            if (distSortie < 8)
            {
                World.DrawMarker(MarkerType.CarSymbol, Config.PosGarageSortie, Vector3.Zero,
                    Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(224, 50, 50),
                    bobUpAndDown: true, rotateY: true);
            }

            if (distSortie < 2)
            {
                Main.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour sortir un véhicule");
                if (IsControlPressed(0, 38))
                {
                    ShowVehicleMenu();
                }
            }
        }

        protected virtual void HandleGarageEntry(Vector3 playerCoords)
        {
            float distEntrer = Config.PosGarageEntrer.DistanceToSquared(playerCoords);

            if (IsPedInAnyVehicle(GetPlayerPed(-1), false))
            {
                if (distEntrer < 8)
                {
                    World.DrawMarker(MarkerType.CarSymbol, Config.PosGarageEntrer, Vector3.Zero,
                        Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(224, 50, 50),
                        bobUpAndDown: true, rotateY: true);
                }

                if (distEntrer < 3)
                {
                    Main.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ranger le véhicule");
                    if (IsControlPressed(0, 38))
                    {
                        var vehicle = GetVehiclePedIsIn(GetPlayerPed(-1), false);
                        SetEntityAsMissionEntity(vehicle, true, true);
                        DeleteVehicle(ref vehicle);
                        Main.SendNotif("~g~Véhicule rangé");
                    }
                }
            }
        }

        protected virtual void ShowVehicleMenu()
        {
            var menu = new NativeMenu("Garage", "Sortir un véhicule")
            {
                UseMouse = false
            };

            foreach (var vehicle in Config.AvailableVehicles)
            {
                var item = new NativeItem(vehicle.ToUpper());
                item.Activated += async (sender, e) => await SpawnVehicle(vehicle);
                menu.Add(item);
            }

            Pool.Add(menu);
            menu.Visible = true;
        }

        protected async Task SpawnVehicle(string modelName)
        {
            var spawnPos = GetAvailableParkingSpot();
            if (spawnPos == Vector3.Zero)
            {
                Main.SendNotif("~r~Aucune place disponible");
                return;
            }

            var model = new Model(modelName);
            model.Request();

            while (!model.IsLoaded)
            {
                await BaseScript.Delay(50);
            }

            var vehicle = await World.CreateVehicle(model, spawnPos, 90f);
            if (vehicle != null && vehicle.Exists())
            {
                SetVehicleNumberPlateText(vehicle.Handle, $"{Config.JobName.ToUpper()}");
                Main.SendNotif("~g~Véhicule sorti du garage");
            }
        }

        protected Vector3 GetAvailableParkingSpot()
        {
            foreach (var spot in Config.ParkingSpots)
            {
                if (!IsSpotOccupied(spot))
                {
                    return spot;
                }
            }
            return Vector3.Zero;
        }

        protected bool IsSpotOccupied(Vector3 spot)
        {
            var vehicles = World.GetAllVehicles();
            return vehicles.Any(v => v.Position.DistanceToSquared(spot) < 4f);
        }

        public virtual void Ticked()
        {
            if (Config.HasDoors)
            {
                HandleDoors();
            }

            if (en_service)
            {
                Cloth();
                Coffre();
                Garage();
            }
        }

        protected void InitializeDoors()
        {
            if (Config?.Doors == null || Config.Doors.Count == 0)
            {
                return;
            }

            foreach (var door in Config.Doors)
            {
                try
                {
                    AddDoorToSystem((uint)door.DoorHash, door.ModelHash,
                        door.Coordinates.X, door.Coordinates.Y, door.Coordinates.Z,
                        false, false, false);

                    door.Initialize();
                }
                catch (System.Exception ex)
                {
                    CitizenFX.Core.Debug.WriteLine($"[ERROR] Failed to initialize door: {ex.Message}");
                }
            }
        }

        protected virtual void HandleDoors()
        {
            if (!HasCorrectJob() || Config?.Doors == null || Config.Doors.Count == 0)
            {
                return;
            }

            Vector3 playerPosition = GetEntityCoords(PlayerPedId(), false);
            DoorClass nearestDoor = GetNearestDoor(playerPosition);

            if (nearestDoor != null)
            {
                float distance = Vector3.DistanceSquared(nearestDoor.Coordinates, playerPosition);

                if (distance < 16f)
                {
                    World.DrawMarker(
                        MarkerType.VerticalCylinder,
                        nearestDoor.Coordinates,
                        Vector3.Zero,
                        Vector3.Zero,
                        new Vector3(0.5f, 0.5f, 1f),
                        System.Drawing.Color.FromArgb(100, 50, 150, 255),
                        false,
                        false
                    );
                }

                if (distance < 4f)
                {
                    int currentState = DoorSystemGetDoorState((uint)nearestDoor.DoorHash);
                    string doorStatus = currentState == 0 ? "~g~ouverte" : "~r~fermée";
                    Main.SendTextUI($"~w~Appuyez sur ~y~E ~w~pour interagir avec la porte ({doorStatus})");

                    if (IsControlJustPressed(0, 38))
                    {
                        int newState = currentState == 0 ? 1 : 0;
                        nearestDoor.SetDoorState(newState);
                    }
                }
            }
        }

        private bool HasCorrectJob()
        {
            var jobInfo = JsonConvert.DeserializeObject<JobInfo>(Main.PlayerInst.Job);
            return jobInfo != null && jobInfo.JobID == Config.JobId;
        }

        protected void ToggleDoor(DoorClass door)
        {
            int currentState = DoorSystemGetDoorState((uint)door.DoorHash);
            int newState = (currentState == 0) ? 1 : 0; // 0 = ouvert, 1 = fermé

            door.SetDoorState(newState);

            Vector3 playerPosition = GetEntityCoords(PlayerPedId(), false);
            BaseScript.TriggerServerEvent("legal_server:setDoorState",
                JsonConvert.SerializeObject(playerPosition), newState);

            string status = newState == 0 ? "ouverte" : "fermée";
            Main.SendNotif($"Porte ~y~{status}");
        }

        public DoorClass GetNearestDoor(Vector3 playerPosition)
        {
            DoorClass nearest = null;
            float minDistance = float.MaxValue;

            foreach (var door in Config.Doors)
            {
                float distance = Vector3.DistanceSquared(door.Coordinates, playerPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = door;
                }
            }

            return minDistance <= 100f ? nearest : null;
        }

        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // THANKS vMENU
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await BaseScript.Delay(0);

            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3:
                    case 2:
                        return null;
                    case 1:
                        return GetOnscreenKeyboardResult();
                    default:
                        await BaseScript.Delay(0);
                        break;
                }
            }
        }
    }
}