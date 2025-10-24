using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using Core.Shared;

namespace Core.Client
{
    public class PlayerMenu
    {
        Format Format;
        ClientMain Client;
        Parking Parking;
        private NUIManager _nuiManager;
        private bool isUIOpen = false;
        private float lastF5Press = 0f;
        NoClip noClipInstance = new NoClip();

        public PlayerMenu(ClientMain caller)
        {
            Client = caller;
            Format = caller.Format;
            Parking = caller.Parking;
            _nuiManager = NUIManager.Instance;

            Client.AddEvent("core:receivePlayers", new Action<string, string>(OnReceivePlayers));

            RegisterCallbacks();
        }

        public PlayerInstance PlayerInst = new PlayerInstance
        {
            Inventory = new List<InventoryItem>(),
            Money = 0
        };

        private void RegisterCallbacks()
        {
            _nuiManager.RegisterCallback("player:reviveSelf", (data) =>
            {
                if (PlayerInst.Rank == "staff")
                {
                    RevivePlayer();
                    ShowNotification("Vous vous êtes réanimé", "success");
                }
            });
            _nuiManager.RegisterCallback("player:showCoordinates", (data) =>
            {
                var coords = GetEntityCoords(GetPlayerPed(-1), true);
                var heading = GetEntityHeading(GetPlayerPed(-1));

                string coordsText = $"X: {coords.X:F2} | Y: {coords.Y:F2} | Z: {coords.Z:F2} | H: {heading:F2}";

                ShowNotification($"Coordonnées: {coordsText}", "info");
                Format.ShowAdvancedNotification("Coordonnées", "Position actuelle",
                    $"~b~X:~w~ {coords.X:F2}\n~b~Y:~w~ {coords.Y:F2}\n~b~Z:~w~ {coords.Z:F2}\n~b~H:~w~ {heading:F2}");
            });
            Client.AddEvent("nui:closed", new Action(() =>
            {
                Debug.WriteLine("[PLAYERMENU] Event nui:closed reçu");
                isUIOpen = false;
            }));
            _nuiManager.RegisterCallback("player:quickRevive", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId") && PlayerInst.Rank == "staff")
                {
                    string targetId = dict["targetId"].ToString();
                    BaseScript.TriggerServerEvent("core:heal", int.Parse(targetId), 200);
                    ShowNotification($"Joueur {targetId} réanimé", "success");
                }
            });

            _nuiManager.RegisterCallback("player:quickNoclip", (data) =>
            {
                if (PlayerInst.Rank == "staff")
                {
                    noClipInstance.SetNoclipActive(!noClipInstance.IsNoclipActive());
                    string status = noClipInstance.IsNoclipActive() ? "activé" : "désactivé";
                    ShowNotification($"NoClip {status}", "info");
                }
            });

            _nuiManager.RegisterCallback("player:useItem", async (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("itemName"))
                {
                    string itemName = dict["itemName"].ToString();
                    await UseItem(itemName);
                }
            });

            _nuiManager.RegisterCallback("player:giveItem", async (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("itemName") && dict.ContainsKey("quantity"))
                {
                    string itemName = dict["itemName"].ToString();
                    int quantity = Convert.ToInt32(dict["quantity"]);
                    await GiveItem(itemName, quantity);
                }
            });

            _nuiManager.RegisterCallback("player:toggleWeapon", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("weaponName") && dict.ContainsKey("action"))
                {
                    string weaponName = dict["weaponName"].ToString();
                    string action = dict["action"].ToString();
                    ToggleWeapon(weaponName, action);
                }
            });

            _nuiManager.RegisterCallback("player:toggleClothes", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("clothesName") && dict.ContainsKey("action"))
                {
                    string clothesName = dict["clothesName"].ToString();
                    string action = dict["action"].ToString();
                    ToggleClothes(clothesName, action);
                }
            });

            _nuiManager.RegisterCallback("player:payBill", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("billIndex"))
                {
                    int billIndex = Convert.ToInt32(dict["billIndex"]);
                    PayBill(billIndex);
                }
            });

            _nuiManager.RegisterCallback("player:buyVIP", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("vipType") && dict.ContainsKey("price"))
                {
                    string vipType = dict["vipType"].ToString();
                    int price = Convert.ToInt32(dict["price"]);
                    BuyVIP(vipType, price);
                }
            });

            _nuiManager.RegisterCallback("player:buyImportVehicle", async (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("vehicleName") && dict.ContainsKey("price"))
                {
                    string vehicleName = dict["vehicleName"].ToString();
                    int price = Convert.ToInt32(dict["price"]);
                    await BuyImportVehicle(vehicleName, price);
                }
            });

            _nuiManager.RegisterCallback("player:showCardId", (data) =>
            {
                ShowCardId();
            });

            // === CALLBACKS ADMIN ===

            _nuiManager.RegisterCallback("admin:revive", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId"))
                {
                    string targetId = dict["targetId"].ToString();
                    BaseScript.TriggerServerEvent("core:heal", int.Parse(targetId), 200);
                    ShowNotification($"Joueur {targetId} soigné", "success");
                }
            });

            _nuiManager.RegisterCallback("admin:goto", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId"))
                {
                    string targetId = dict["targetId"].ToString();
                    BaseScript.TriggerServerEvent("core:goto", int.Parse(targetId));
                    ShowNotification($"Téléportation vers le joueur {targetId}", "info");
                }
            });

            _nuiManager.RegisterCallback("admin:bring", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId"))
                {
                    string targetId = dict["targetId"].ToString();
                    var myCoords = GetEntityCoords(GetPlayerPed(-1), true);
                    BaseScript.TriggerServerEvent("core:bringServer", int.Parse(targetId), (int)myCoords.X, (int)myCoords.Y, (int)myCoords.Z);
                    ShowNotification($"Joueur {targetId} téléporté vers vous", "success");
                }
            });

            _nuiManager.RegisterCallback("admin:cuff", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId"))
                {
                    string targetId = dict["targetId"].ToString();
                    BaseScript.TriggerServerEvent("core:cuff", int.Parse(targetId));
                    ShowNotification($"État des menottes changé pour le joueur {targetId}", "info");
                }
            });

            _nuiManager.RegisterCallback("admin:jail", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId") && dict.ContainsKey("duration") && dict.ContainsKey("reason"))
                {
                    string targetId = dict["targetId"].ToString();
                    int duration = Convert.ToInt32(dict["duration"]);
                    string reason = dict["reason"].ToString();

                    BaseScript.TriggerServerEvent("core:jail", int.Parse(targetId), reason, duration);
                    ShowNotification($"Joueur {targetId} emprisonné pour {duration}s", "warning");
                }
            });

            _nuiManager.RegisterCallback("admin:giveMoney", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId") && dict.ContainsKey("amount"))
                {
                    string targetId = dict["targetId"].ToString();
                    int amount = Convert.ToInt32(dict["amount"]);

                    BaseScript.TriggerServerEvent("core:giveMoney", int.Parse(targetId), amount);
                    ShowNotification($"${amount} donné au joueur {targetId}", "success");
                }
            });

            _nuiManager.RegisterCallback("admin:warn", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId") && dict.ContainsKey("reason"))
                {
                    string targetId = dict["targetId"].ToString();
                    string reason = dict["reason"].ToString();

                    BaseScript.TriggerServerEvent("core:warn", int.Parse(targetId), reason);
                    ShowNotification($"Joueur {targetId} averti", "warning");
                }
            });

            _nuiManager.RegisterCallback("admin:kick", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId") && dict.ContainsKey("reason"))
                {
                    string targetId = dict["targetId"].ToString();
                    string reason = dict["reason"].ToString();

                    BaseScript.TriggerServerEvent("core:kick", int.Parse(targetId), reason);
                    ShowNotification($"Joueur {targetId} expulsé", "error");
                }
            });

            _nuiManager.RegisterCallback("admin:ban", (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("targetId") && dict.ContainsKey("reason"))
                {
                    string targetId = dict["targetId"].ToString();
                    string reason = dict["reason"].ToString();

                    BaseScript.TriggerServerEvent("core:ban", int.Parse(targetId), reason);
                    ShowNotification($"Joueur {targetId} banni définitivement", "error");
                }
            });

            _nuiManager.RegisterCallback("admin:spawnVehicle", async (data) =>
            {
                var dict = data as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey("vehicleName"))
                {
                    string model = dict["vehicleName"].ToString();
                    var hash = (uint)GetHashKey(model);

                    if (!IsModelInCdimage(hash) || !IsModelAVehicle(hash))
                    {
                        ShowNotification("Ce véhicule n'existe pas", "error");
                        return;
                    }

                    var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
                    Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                    ShowNotification($"Véhicule {model} spawné", "success");
                }
            });

            _nuiManager.RegisterCallback("admin:toggleNoclip", (data) =>
            {
                noClipInstance.SetNoclipActive(!noClipInstance.IsNoclipActive());
            });

        }

        private void RevivePlayer()
        {
            int ped = GetPlayerPed(-1);

            StopScreenEffect("DeathFailMPIn");
            StopScreenEffect("DeathFailOut");
            AnimpostfxStopAll();

            ClearPedTasksImmediately(ped);

            SetEntityHealth(ped, 200);

            SetEntityInvincible(ped, false);
            SetEveryoneIgnorePlayer(ped, false);

            Client.IsDead = false;

            ClearTimecycleModifier();
            SetPedMotionBlur(ped, false);
        }

        public void F5Menu()
        {
            if (IsControlJustPressed(0, 166))
            {
                float currentTime = GetGameTimer() / 1000f;

                bool isStaff = PlayerInst.Rank == "staff";
                bool canOpen = isStaff || (currentTime - lastF5Press >= 5f);

                if (canOpen)
                {
                    // CORRECTION: Force cleanup si une UI est déjà ouverte
                    if (isUIOpen || _nuiManager.IsOpen)
                    {
                        Debug.WriteLine($"[PLAYERMENU] UI déjà ouverte ({_nuiManager.CurrentNUI}), nettoyage forcé");
                        _nuiManager.CloseNUI();
                        isUIOpen = false;
                        // Attendre un frame avant de réouvrir
                        return;
                    }

                    lastF5Press = currentTime;
                    OpenPlayerMenu();
                }
                else
                {
                    float remainingTime = 5f - (currentTime - lastF5Press);
                    ShowNotification($"Attendez {remainingTime:F1}s avant de rouvrir le menu", "warning");
                }
            }
        }

        private void OpenPlayerMenu()
        {
            Debug.WriteLine("[PLAYERMENU] OpenPlayerMenu appelé");
            isUIOpen = true;

            if (PlayerInst.Rank == "staff")
            {
                BaseScript.TriggerServerEvent("core:getPlayersList");
            }

            var items = PlayerInst.Inventory ?? new List<InventoryItem>();
            var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");

            var clothesList = new List<object>();
            try
            {
                var clothesInfo = JsonConvert.DeserializeObject<List<ClothingSet>>(PlayerInst.ClothesList);
                if (clothesInfo != null)
                {
                    foreach (var clothesSet in clothesInfo)
                    {
                        if (clothesSet.Name != null)
                        {
                            clothesList.Add(new
                            {
                                name = clothesSet.Name,
                                components = clothesSet.Components.Count
                            });
                        }
                    }
                }
            }
            catch { }

            var billsList = new List<object>();
            try
            {
                var bills = JsonConvert.DeserializeObject<List<Bills>>(PlayerInst.Bills);
                if (bills != null)
                {
                    billsList = bills.Select(b => new {
                        company = b.Company,
                        author = b.Author,
                        date = b.Date,
                        amount = b.Amount
                    }).ToList<object>();
                }
            }
            catch { }

            var job = JsonConvert.DeserializeObject<JobInfo>(PlayerInst.Job ?? "{}");

            var menuData = new
            {
                playerInfo = new
                {
                    firstname = PlayerInst.Firstname,
                    lastname = PlayerInst.Lastname,
                    birth = PlayerInst.Birth,
                    money = PlayerInst.Money,
                    cash = dollarsItem?.Quantity ?? 0,
                    bitcoin = PlayerInst.Bitcoin,
                    rank = PlayerInst.Rank,
                    job = job.JobID,
                    jobRank = job.JobRank
                },
                inventory = items.Where(i => i.Type == "item" && i.Quantity > 0).Select(i => new {
                    name = i.Item,
                    quantity = i.Quantity,
                    type = i.Type,
                    icon = GetItemIcon(i.Item)
                }).ToList(),
                weapons = items.Where(i => i.Type == "weapon").Select(w => new {
                    name = w.Item,
                    ammo = GetAmmoCount(w.Item),
                    icon = "🔫"
                }).ToList(),
                clothes = clothesList,
                bills = billsList,
                vehicles = GetImportVehicles(),
                isDead = Client.IsDead,
                isAdmin = PlayerInst.Rank == "staff",
                players = GetPlayersList()
            };

            _nuiManager.OpenNUI("playerMenu", menuData);
            Debug.WriteLine("[PLAYERMENU] NUI playerMenu ouvert");
        }

        private List<object> GetImportVehicles()
        {
            VehicleImport[] values = (VehicleImport[])System.Enum.GetValues(typeof(VehicleImport));
            var vehicles = new List<object>();

            foreach (VehicleImport vehicle in values)
            {
                DisplayAttribute displayAttribute = Format.GetDisplayAttribute(vehicle);
                vehicles.Add(new
                {
                    name = displayAttribute.Name,
                    model = displayAttribute.VehicleName,
                    price = displayAttribute.Price
                });
            }

            return vehicles;
        }

        private List<object> GetPlayersList()
        {
            var playersList = new List<object>();

            for (int i = 0; i < Client.PlayersInstList.Count; i++)
            {
                var playerInst = Client.PlayersInstList[i];
                var handle = Client.PlayersHandle[i];
                var job = JsonConvert.DeserializeObject<JobInfo>(playerInst.Job);

                playersList.Add(new
                {
                    id = handle,
                    dbId = playerInst.Id,
                    firstname = playerInst.Firstname,
                    lastname = playerInst.Lastname,
                    discord = playerInst.Discord,
                    job = $"{job.JobID} | {job.JobRank}",
                    organisation = playerInst.Organisation,
                    rank = playerInst.Rank,
                    bitcoin = playerInst.Bitcoin,
                    money = playerInst.Money
                });
            }

            return playersList;
        }

        private string GetItemIcon(string itemName)
        {
            switch (itemName)
            {
                case "Pain": return "🍞";
                case "Sandwich": return "🥪";
                case "Burger": return "🍔";
                case "Eau": return "💧";
                case "Coca Cola": return "🥤";
                case "Café": return "☕";
                case "Phone": return "📱";
                case "Dollars": return "💵";
                case "Ordinateur": return "💻";
                case "Perceuse": return "🔨";
                case "Menotte": return "🔗";
                case "Cigarettes": return "🚬";
                case "Briquet": return "🔥";
                case "Outil de crochetage": return "🔓";
                default: return "📦";
            }
        }

        private int GetAmmoCount(string weaponName)
        {
            var items = PlayerInst.Inventory;
            var ammoItem = items.FirstOrDefault(i => i.Item == "Munitions");
            return ammoItem?.Quantity ?? 0;
        }

        private async Task UseItem(string itemName)
        {
            switch (itemName)
            {
                case "Pain":
                    Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                    SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1)) + 10);
                    await Format.AddPropToPlayer("prop_sandwich_01", 28422, 0, 0, 0, 0, 0, 0, 3000);
                    Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                    break;
                case "Sandwich":
                    Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                    SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1)) + 30);
                    await Format.AddPropToPlayer("prop_sandwich_01", 28422, 0, 0, 0, 0, 0, 0, 3000);
                    Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                    break;
                case "Burger":
                    Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                    SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1)) + 30);
                    await Format.AddPropToPlayer("prop_cs_burger_01", 28422, 0, 0, 0, 0, 0, 0, 3000);
                    Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                    break;
                case "Eau":
                    Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                    await Format.AddPropToPlayer("prop_water_bottle", 28422, 0, 0, 0, 0, 0, 0, 3000);
                    Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                    break;
                case "Coca Cola":
                    Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                    await Format.AddPropToPlayer("prop_ecola_can", 28422, 0, 0, 0, 0, 0, 0, 3000);
                    Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                    break;
            }

            BaseScript.TriggerServerEvent("core:removeItem", itemName, 1);
            RefreshPlayerMenu();
        }

        private async Task GiveItem(string itemName, int quantity)
        {
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
            var closestPed = World.GetClosest(playerCoords, without_me.ToArray());

            if (GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, closestPed.Position.X, closestPed.Position.Y, closestPed.Position.Z, true) < 10)
            {
                var closestPedID = GetPlayerServerId(NetworkGetPlayerIndexFromPed(closestPed.Handle));
                BaseScript.TriggerServerEvent("core:shareItem", closestPedID, itemName, quantity);
                RefreshPlayerMenu();
            }
            else
            {
                ShowNotification("Personne à proximité", "error");
            }
        }

        private void ToggleWeapon(string weaponName, string action)
        {
            var ammo = GetAmmoCount("Munitions");

            if (action == "equip")
            {
                GiveWeaponToPed(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weaponName}"), 0, false, false);
                SetPedAmmo(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weaponName}"), ammo);
                ShowNotification($"Arme équipée: {weaponName}", "success");
            }
            else
            {
                RemoveWeaponFromPed(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weaponName}"));
                ShowNotification($"Arme rangée: {weaponName}", "info");
            }
        }

        private void ToggleClothes(string clothesName, string action)
        {
            var clothesInfo = JsonConvert.DeserializeObject<List<ClothingSet>>(PlayerInst.ClothesList);
            var clothesSet = clothesInfo.FirstOrDefault(c => c.Name == clothesName);

            if (clothesSet != null)
            {
                if (action == "wear")
                {
                    foreach (var component in clothesSet.Components)
                    {
                        if (component.Palette == 1)
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), component.ComponentId, component.Drawable, component.Texture, component.Palette);
                        }
                        else
                        {
                            SetPedPropIndex(GetPlayerPed(-1), component.ComponentId, component.Drawable, component.Texture, false);
                        }
                    }
                    ShowNotification($"Vêtements portés: {clothesName}", "success");
                }
                else
                {
                    foreach (var component in clothesSet.Components)
                    {
                        if (component.Palette == 1)
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), component.ComponentId, 15, 0, 2);
                        }
                        else
                        {
                            SetPedPropIndex(GetPlayerPed(-1), component.ComponentId, 8, 0, false);
                        }
                    }
                    ShowNotification($"Vêtements enlevés: {clothesName}", "info");
                }
            }
        }

        private void PayBill(int billIndex)
        {
            var bills = JsonConvert.DeserializeObject<List<Bills>>(PlayerInst.Bills);

            if (billIndex >= 0 && billIndex < bills.Count)
            {
                var bill = bills[billIndex];

                if (PlayerInst.Money >= bill.Amount)
                {
                    BaseScript.TriggerServerEvent("core:payBill", bill.Company, bill.Amount);
                    ShowNotification($"Facture payée: {bill.Company} - ${bill.Amount}", "success");
                    RefreshPlayerMenu();
                }
                else
                {
                    ShowNotification("Fonds insuffisants", "error");
                }
            }
        }

        private void BuyVIP(string vipType, int price)
        {
            if (PlayerInst.Bitcoin >= price)
            {
                BaseScript.TriggerServerEvent("core:bitcoinTransaction", price);
                ShowNotification($"VIP acheté: {vipType}", "success");
                RefreshPlayerMenu();
            }
            else
            {
                ShowNotification("Bitcoins insuffisants", "error");
            }
        }

        private async Task BuyImportVehicle(string vehicleName, int price)
        {
            if (PlayerInst.Bitcoin >= price)
            {
                BaseScript.TriggerServerEvent("core:bitcoinTransaction", price);

                var model = new Model(GetHashKey(vehicleName));
                if (!model.IsInCdImage || !model.IsValid)
                {
                    ShowNotification("Erreur: modèle invalide", "error");
                    return;
                }

                model.Request();
                while (!model.IsLoaded) await BaseScript.Delay(0);

                var car = await World.CreateVehicle(model.Hash, GetEntityCoords(GetPlayerPed(-1), true), 90);
                if (car != null && car.Exists())
                {
                    car.IsPersistent = true;
                    SetPedIntoVehicle(GetPlayerPed(-1), car.Handle, -2);
                    SendVehicleInfo(car);
                    BaseScript.TriggerServerEvent("core:getVehicleInfo");
                    ShowNotification($"Véhicule acheté: {vehicleName}", "success");
                }
            }
            else
            {
                ShowNotification("Bitcoins insuffisants", "error");
            }
        }

        private void ShowCardId()
        {
            var player = GetPlayerPed(-1);
            var playerCoords = GetEntityCoords(player, true);
            var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
            var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());

            if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
            {
                BaseScript.TriggerServerEvent("core:showCardId", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)));
                ShowNotification("Carte d'identité montrée", "success");
            }
            else
            {
                ShowNotification("Personne à proximité", "error");
            }
        }

        private void RefreshPlayerMenu()
        {
            if (isUIOpen)
            {
                _nuiManager.CloseNUI();
                isUIOpen = false;
                BaseScript.Delay(100).ContinueWith(_ => OpenPlayerMenu());
            }
        }

        private void ShowNotification(string message, string type = "info")
        {
            _nuiManager.SendNUIMessage(new
            {
                action = "notification",
                data = new { message, type }
            });
        }

        public void SendVehicleInfo(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists()) return;

            VehicleInfo info = new VehicleInfo
            {
                Model = API.GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash),
                Plate = vehicle.Mods.LicensePlate,
                Boot = new List<BootInfo>(),
                EngineLevel = vehicle.Mods[VehicleModType.Engine].Index,
                BrakeLevel = vehicle.Mods[VehicleModType.Brakes].Index,
                ColorPrimary = (int)vehicle.Mods.PrimaryColor,
                ColorSecondary = (int)vehicle.Mods.SecondaryColor,
            };

            BaseScript.TriggerServerEvent("core:sendVehicleInfo", JsonConvert.SerializeObject(info));
        }

        private void OnReceivePlayers(string jsonInst, string jsonHandles)
        {
            var playersInst = JsonConvert.DeserializeObject<List<PlayerInstance>>(jsonInst);
            var playersHandle = JsonConvert.DeserializeObject<List<string>>(jsonHandles);
            Client.PlayersInstList.Clear();
            Client.PlayersHandle.Clear();

            foreach (var playerInst in playersInst)
                Client.PlayersInstList.Add(playerInst);
            foreach (var player in playersHandle)
                Client.PlayersHandle.Add(player);

            if (isUIOpen && PlayerInst.Rank == "staff")
            {
                var playersList = GetPlayersList();
                _nuiManager.SendNUIMessage(new
                {
                    action = "updatePlayers",
                    players = playersList
                });
            }
        }

        public void GetPlayerData(string json)
        {
            var player = JsonConvert.DeserializeObject<PlayerInstance>(json);
            PlayerInst.Id = player.Id;
            PlayerInst.Discord = player.Discord;
            PlayerInst.Firstname = player.Firstname;
            PlayerInst.Lastname = player.Lastname;
            PlayerInst.State = player.State;
            PlayerInst.Skin = player.Skin;
            PlayerInst.Rank = player.Rank;
            PlayerInst.Bitcoin = player.Bitcoin;
            PlayerInst.Birth = player.Birth;
            PlayerInst.Job = player.Job;
            PlayerInst.Clothes = player.Clothes;
            PlayerInst.ClothesList = player.ClothesList;
            PlayerInst.Money = player.Money;
            PlayerInst.Bills = player.Bills;
            PlayerInst.Inventory = player.Inventory;
            PlayerInst.LastPosition = player.LastPosition;
        }

        public void TogglePlayerNames(bool showNames)
        {
            var gamerTags = new List<int>();
            for (int i = 0; i < Client.PlayersInstList.Count; i++)
            {
                var playerInst = Client.PlayersInstList[i];
                var handle = Client.PlayersHandle[i];
                var targetPlayer = GetPlayerPed(GetPlayerFromServerId(int.Parse(handle)));

                if (i >= gamerTags.Count)
                {
                    int gamerTagID = CreateMpGamerTag(targetPlayer, $"[{handle}] {playerInst.Firstname} {playerInst.Lastname}", true, false, "test", 1);
                    gamerTags.Add(gamerTagID);
                }

                SetMpGamerTagVisibility(gamerTags[i], 0, showNames);
            }
        }
    }
}