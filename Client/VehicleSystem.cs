using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Core.Shared;
using Mono.CSharp;

namespace Core.Client
{
    public class VehicleSystem
    {
        ClientMain Client;
        Format Format;
        PlayerMenu PlayerMenu;
        ObjectPool Pool = new ObjectPool();

        // Couleurs pour l'esthétique
        private readonly System.Drawing.Color AccentColor = System.Drawing.Color.FromArgb(255, 255, 152, 0);
        private readonly System.Drawing.Color SuccessColor = System.Drawing.Color.FromArgb(255, 76, 175, 80);
        private readonly System.Drawing.Color ErrorColor = System.Drawing.Color.FromArgb(255, 244, 67, 54);
        private readonly System.Drawing.Color InfoColor = System.Drawing.Color.FromArgb(255, 33, 150, 243);
        private readonly System.Drawing.Color WarningColor = System.Drawing.Color.FromArgb(255, 255, 193, 7);

        // Tracking du véhicule actuel
        private int currentVehicle = 0;
        private const float MAX_BOOT_DISTANCE = 5f;
        private const float MAX_MENU_DISTANCE = 10f;

        public VehicleSystem(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            Client.AddEvent("core:changeLockState", new Action<int, string, int>(ChangeLockState));
            Client.AddEvent("core:getCarInformation", new Action(GetCarInformation));
        }

        public void GetCarInformation()
        {
            Vehicle vehicle = Client.GetLocalPlayer().Character.CurrentVehicle;

            if (vehicle != null)
            {
                string modelName = GetDisplayNameFromVehicleModel(vehicle.Model);
                string licensePlate = GetVehicleNumberPlateText(vehicle.Handle);
                int engineLevel = GetVehicleMod(vehicle.Handle, 11);
                int brakeLevel = GetVehicleMod(vehicle.Handle, 12);

                var carInfo = new
                {
                    Model = modelName,
                    LicensePlate = licensePlate,
                    EngineLevel = engineLevel,
                    BrakeLevel = brakeLevel,
                };

                string json = JsonConvert.SerializeObject(carInfo);
                BaseScript.TriggerServerEvent("core:receiveCarInformation", json);
            }
        }

        public void ChangeLockState(int id, string plate, int isLock)
        {
            if (isLock == 2 || isLock == 0)
            {
                SetVehicleDoorsLocked(id, 1);
                PlayVehicleDoorOpenSound(id, 1);

                // Effets visuels
                SetVehicleLights(id, 2);
                BaseScript.Delay(150).ContinueWith(_ =>
                {
                    SetVehicleLights(id, 0);
                });

                ShowNotification("~g~✓ Véhicule déverrouillé", SuccessColor);
                PlaySoundFrontend(-1, "VEHICLE_UNLOCK", "HUD_AWARDS", false);
            }
            else if (isLock == 1)
            {
                SetVehicleDoorsLocked(id, 2);
                PlayVehicleDoorOpenSound(id, 2);

                // Double flash des feux
                SetVehicleLights(id, 2);
                BaseScript.Delay(150).ContinueWith(_ =>
                {
                    SetVehicleLights(id, 0);
                    BaseScript.Delay(150).ContinueWith(__ =>
                    {
                        SetVehicleLights(id, 2);
                        BaseScript.Delay(150).ContinueWith(___ =>
                        {
                            SetVehicleLights(id, 0);
                        });
                    });
                });

                ShowNotification("~r~✓ Véhicule verrouillé", ErrorColor);
                PlaySoundFrontend(-1, "VEHICLE_LOCK", "HUD_AWARDS", false);
            }
            else
            {
                ShowNotification("~r~✗ Erreur de verrouillage", ErrorColor);
            }
        }

        public void GetVehicleBoot(string plate)
        {
            var items = PlayerMenu.PlayerInst.Inventory;

            var menu = new NativeMenu(
                $"🚗 Coffre du véhicule",
                $"~o~Plaque: ~w~{plate}\n~b~---------------------\n~y~Gérez vos objets"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            Pool.Add(menu);
            menu.Visible = true;

            // Statistiques du coffre
            int totalItemsInBoot = Client.MyVehicle.Boot?.Count ?? 0;
            int totalQuantityInBoot = Client.MyVehicle.Boot?.Sum(b => b.Quantity) ?? 0;

            var statsItem = new NativeItem(
                "📊 Statistiques du coffre",
                $"~b~---------------------\n" +
                $"~b~Articles différents:~w~ {totalItemsInBoot}\n" +
                $"~b~Quantité totale:~w~ {totalQuantityInBoot}\n" +
                $"~b~Capacité:~w~ ~g~Illimitée\n" +
                $"~b~---------------------"
            )
            {
                Enabled = false
            };
            menu.Add(statsItem);

            menu.Add(CreateSeparator());

            // Menu pour retirer
            var pick = new NativeMenu("📤 Retirer des objets", "~y~Prenez des objets du coffre")
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            menu.AddSubMenu(pick);
            Pool.Add(pick);

            if (Client.MyVehicle.Boot != null && Client.MyVehicle.Boot.Count > 0)
            {
                foreach (BootInfo boot in Client.MyVehicle.Boot)
                {
                    if (boot.Quantity > 0)
                    {
                        var itemIcon = GetItemIcon(boot.Item);
                        var item = new NativeItem(
                            $"{itemIcon} {boot.Item}",
                            $"~b~---------------------\n" +
                            $"~b~Type:~w~ {boot.Type}\n" +
                            $"~b~Quantité disponible:~w~ ~g~{boot.Quantity}\n" +
                            $"~b~---------------------\n" +
                            $"~g~➤ Cliquez pour retirer",
                            $"~g~x{boot.Quantity}"
                        );
                        pick.Add(item);

                        item.Activated += async (sender, e) =>
                        {
                            await WithdrawFromBoot(plate, boot, pick, item);
                        };
                    }
                }
            }
            else
            {
                var emptyItem = new NativeItem("📭 Coffre vide", "~y~Aucun objet dans le coffre")
                {
                    Enabled = false
                };
                pick.Add(emptyItem);
            }

            // Menu pour déposer
            var drop = new NativeMenu("📥 Déposer des objets", "~y~Mettez des objets dans le coffre")
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            menu.AddSubMenu(drop);
            Pool.Add(drop);

            if (items != null && items.Any(i => i.Quantity > 0 && i.Item != null))
            {
                // Grouper par type
                var groupedItems = items.Where(i => i.Quantity > 0 && i.Item != null)
                                        .GroupBy(i => i.Type);

                foreach (var group in groupedItems)
                {
                    var typeHeader = new NativeItem($"~h~📦 {group.Key.ToUpper()}", "")
                    {
                        Enabled = false
                    };
                    drop.Add(typeHeader);

                    foreach (var item in group)
                    {
                        var itemIcon = GetItemIcon(item.Item);
                        var invItem = new NativeItem(
                            $"{itemIcon} {item.Item}",
                            $"~b~---------------------\n" +
                            $"~b~Type:~w~ {item.Type}\n" +
                            $"~b~En possession:~w~ ~g~{item.Quantity}\n" +
                            $"~b~---------------------\n" +
                            $"~g~➤ Cliquez pour déposer",
                            $"~g~x{item.Quantity}"
                        );
                        drop.Add(invItem);

                        invItem.Activated += async (sender, e) =>
                        {
                            await DepositInBoot(plate, item, drop);
                        };
                    }

                    drop.Add(CreateSeparator());
                }
            }
            else
            {
                var emptyItem = new NativeItem("👜 Inventaire vide", "~y~Vous n'avez aucun objet")
                {
                    Enabled = false
                };
                drop.Add(emptyItem);
            }

            menu.Add(CreateSeparator());

            // Options rapides
            var quickInfo = new NativeItem(
                "💡 Conseil",
                "~y~Astuce: Organisez vos objets stratégiquement\n" +
                "~w~Les objets lourds sont mieux dans le coffre !"
            )
            {
                Enabled = false
            };
            menu.Add(quickInfo);

            menu.Closing += (sender, e) =>
            {
                currentVehicle = 0;
            };
        }

        private async Task WithdrawFromBoot(string plate, BootInfo boot, NativeMenu menu, NativeItem item)
        {
            var textInput = await Format.GetUserInput("Quantité à retirer", boot.Quantity.ToString(), 4);

            if (string.IsNullOrEmpty(textInput) || !int.TryParse(textInput, out int parsedInput) || parsedInput <= 0)
            {
                ShowNotification("~r~✗ Quantité invalide", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                return;
            }

            if (parsedInput <= boot.Quantity)
            {
                ShowNotification("~y~⏳ Retrait en cours...", WarningColor);
                await BaseScript.Delay(500);

                BaseScript.TriggerServerEvent("core:removeItemFromBoot", plate, boot.Item, parsedInput);

                ShowNotification(
                    $"~g~✓ Objet retiré\n" +
                    $"~w~Article: ~b~{boot.Item}\n" +
                    $"~w~Quantité: ~g~x{parsedInput}",
                    SuccessColor
                );
                PlaySoundFrontend(-1, "PICK_UP", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);

                if (boot.Quantity - parsedInput <= 0)
                {
                    menu.Remove(item);
                }

                menu.Visible = false;
            }
            else
            {
                ShowNotification("~r~✗ Quantité trop importante", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        private async Task DepositInBoot(string plate, InventoryItem item, NativeMenu menu)
        {
            var textInput = await Format.GetUserInput("Quantité à déposer", item.Quantity.ToString(), 4);

            if (string.IsNullOrEmpty(textInput) || !int.TryParse(textInput, out int parsedInput) || parsedInput <= 0)
            {
                ShowNotification("~r~✗ Quantité invalide", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                return;
            }

            if (parsedInput <= item.Quantity)
            {
                ShowNotification("~y~⏳ Dépôt en cours...", WarningColor);
                await BaseScript.Delay(500);

                BaseScript.TriggerServerEvent("core:addItemInBoot", plate, item.Item, parsedInput, item.Type);

                ShowNotification(
                    $"~g~✓ Objet déposé\n" +
                    $"~w~Article: ~b~{item.Item}\n" +
                    $"~w~Quantité: ~g~x{parsedInput}",
                    SuccessColor
                );
                PlaySoundFrontend(-1, "PICKUP_WEAPON_SMOKEGRENADE", "HUD_FRONTEND_CUSTOM_SOUNDSET", false);

                menu.Visible = false;
            }
            else
            {
                ShowNotification("~r~✗ Quantité trop importante", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        public void VehicleMenu()
        {
            var vehicle = GetVehiclePedIsIn(PlayerPedId(), false);

            if (vehicle == 0)
            {
                ShowNotification("~r~✗ Vous devez être dans un véhicule", ErrorColor);
                return;
            }

            currentVehicle = vehicle;

            // Récupération des stats
            var engineHealth = (GetVehicleEngineHealth(vehicle)) * 100.0 / 1000.0;
            var bodyHealth = (GetVehicleBodyHealth(vehicle)) * 100.0 / 1000.0;
            var fuelLevel = GetVehicleFuelLevel(vehicle);
            var dirtLevel = GetVehicleDirtLevel(vehicle);
            string plate = GetVehicleNumberPlateText(vehicle);
            string modelName = GetDisplayNameFromVehicleModel((uint)GetEntityModel(vehicle));

            var menu = new NativeMenu(
                $"🚗 {modelName}",
                $"~o~Plaque: ~w~{plate}\n~b~---------------------"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            Pool.Add(menu);
            menu.Visible = true;

            // Statistiques du véhicule
            var statsMenu = new NativeMenu("📊 Statistiques", "~y~État du véhicule")
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            Pool.Add(statsMenu);
            menu.AddSubMenu(statsMenu);

            var engineItem = new NativeItem(
                "🔧 État du moteur",
                $"~b~Santé:~w~ {GetHealthBar(engineHealth)}\n~w~{String.Format("{0:0.##}", engineHealth)}%"
            )
            {
                Enabled = false
            };
            statsMenu.Add(engineItem);

            var bodyItem = new NativeItem(
                "🛡️ État de la carrosserie",
                $"~b~Intégrité:~w~ {GetHealthBar(bodyHealth)}\n~w~{String.Format("{0:0.##}", bodyHealth)}%"
            )
            {
                Enabled = false
            };
            statsMenu.Add(bodyItem);

            var fuelItem = new NativeItem(
                "⛽ Niveau de carburant",
                $"~b~Réservoir:~w~ {GetFuelBar(fuelLevel)}\n~w~{String.Format("{0:0.##}", fuelLevel)}%"
            )
            {
                Enabled = false
            };
            statsMenu.Add(fuelItem);

            var dirtItem = new NativeItem(
                "💧 Propreté",
                $"~b~Saleté:~w~ {GetDirtBar(dirtLevel)}\n~w~{GetCleanlinessText(dirtLevel)}"
            )
            {
                Enabled = false
            };
            statsMenu.Add(dirtItem);

            menu.Add(CreateSeparator());

            // Options du véhicule
            var driftMode = new NativeCheckboxItem("🏎️ Mode Drift", "~y~Active le drift mode pour glisser", false);
            menu.Add(driftMode);

            var driftState = false;
            driftMode.Activated += (sender, e) =>
            {
                driftState = !driftState;
                SetVehicleReduceGrip(vehicle, driftState);

                if (driftState)
                {
                    ShowNotification("~g~✓ Mode Drift activé\n~y~⚠️ Attention aux virages !", SuccessColor);
                    PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                }
                else
                {
                    ShowNotification("~b~✓ Mode Drift désactivé", InfoColor);
                    PlaySoundFrontend(-1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                }
            };

            menu.Add(CreateSeparator());

            // Gestion des portes
            var doorsMenu = new NativeMenu("🚪 Gestion des portes", "~y~Ouvrir/Fermer les portes")
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            Pool.Add(doorsMenu);
            menu.AddSubMenu(doorsMenu);

            var doorsList = new List<(string name, int index)>
            {
                ("🚪 Avant gauche", 0),
                ("🚪 Avant droit", 1),
                ("🚪 Arrière gauche", 2),
                ("🚪 Arrière droit", 3),
                ("🚗 Capot", 4),
                ("🚙 Coffre", 5)
            };

            foreach (var door in doorsList)
            {
                var doorItem = new NativeItem(
                    door.name,
                    GetVehicleDoorAngleRatio(vehicle, door.index) > 0.1f ? "~g~Ouverte" : "~r~Fermée"
                );
                doorsMenu.Add(doorItem);

                doorItem.Activated += (sender, e) =>
                {
                    bool isOpen = GetVehicleDoorAngleRatio(vehicle, door.index) > 0.1f;

                    if (isOpen)
                    {
                        SetVehicleDoorShut(vehicle, door.index, false);
                        ShowNotification($"~r~✓ {door.name} fermée", ErrorColor);
                    }
                    else
                    {
                        SetVehicleDoorOpen(vehicle, door.index, false, false);
                        ShowNotification($"~g~✓ {door.name} ouverte", SuccessColor);
                    }

                    PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                };
            }

            var allDoorsItem = new NativeItem("🚗 Toutes les portes", "~y~Ouvrir/Fermer toutes les portes");
            doorsMenu.Add(allDoorsItem);

            allDoorsItem.Activated += (sender, e) =>
            {
                bool anyOpen = false;
                for (int i = 0; i < 6; i++)
                {
                    if (GetVehicleDoorAngleRatio(vehicle, i) > 0.1f)
                    {
                        anyOpen = true;
                        break;
                    }
                }

                if (anyOpen)
                {
                    SetVehicleDoorsShut(vehicle, false);
                    ShowNotification("~r~✓ Toutes les portes fermées", ErrorColor);
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        SetVehicleDoorOpen(vehicle, i, false, false);
                    }
                    ShowNotification("~g~✓ Toutes les portes ouvertes", SuccessColor);
                }

                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            // Limiteur de vitesse
            var speedLimits = new List<int> { 50, 90, 110, 130, 999 };
            var currentLimit = speedLimits.FindIndex(s => Math.Abs(GetVehicleMaxSpeed(vehicle) * 3.6f - s) < 10);
            if (currentLimit == -1) currentLimit = speedLimits.Count - 1;

            var limiter = new NativeListItem<int>(
                "⚡ Limiteur de vitesse",
                "~y~Limitez votre vitesse maximale",
                speedLimits.ToArray()
            );
            limiter.SelectedIndex = currentLimit;
            menu.Add(limiter);

            limiter.ItemChanged += (sender, e) =>
            {
                int selectedSpeed = limiter.SelectedItem;

                if (selectedSpeed == 999)
                {
                    SetVehicleMaxSpeed(vehicle, 999f);
                    ShowNotification("~g~✓ Limiteur désactivé\n~w~Vitesse maximale", SuccessColor);
                }
                else
                {
                    SetVehicleMaxSpeed(vehicle, selectedSpeed / 3.6f);
                    ShowNotification($"~y~✓ Limiteur activé\n~w~Maximum: ~b~{selectedSpeed} km/h", WarningColor);
                }

                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            // Options rapides
            var rotateItem = new NativeItem("🔄 Faire pivoter la caméra", "~y~Changez de vue");
            menu.Add(rotateItem);
            rotateItem.Activated += (sender, e) =>
            {
                SetFollowVehicleCamViewMode(GetFollowVehicleCamViewMode() == 4 ? 0 : GetFollowVehicleCamViewMode() + 1);
                PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            var cleanItem = new NativeItem("💧 Nettoyer le véhicule", "~g~$50", "~g~$50");
            menu.Add(cleanItem);
            cleanItem.Activated += (sender, e) =>
            {
                if (PlayerMenu.PlayerInst.Money >= 50)
                {
                    SetVehicleDirtLevel(vehicle, 0f);
                    ShowNotification("~g~✓ Véhicule nettoyé\n~r~-$50", SuccessColor);
                    PlaySoundFrontend(-1, "PURCHASE", "HUD_LIQUOR_STORE_SOUNDSET", false);
                    BaseScript.TriggerServerEvent("core:transaction", 50, "Nettoyage", 1, "service");
                }
                else
                {
                    ShowNotification("~r~✗ Fonds insuffisants", ErrorColor);
                    PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                }
            };

            menu.Closing += (sender, e) =>
            {
                currentVehicle = 0;
            };
        }

        private string GetHealthBar(double percentage)
        {
            int bars = (int)(percentage / 10);
            string bar = "~g~";

            if (percentage < 30) bar = "~r~";
            else if (percentage < 60) bar = "~o~";
            else if (percentage < 80) bar = "~y~";

            return bar + new string('█', bars) + "~s~" + new string('░', 10 - bars);
        }

        private string GetFuelBar(float fuel)
        {
            int bars = (int)(fuel / 10);
            string bar = "~g~";

            if (fuel < 20) bar = "~r~";
            else if (fuel < 50) bar = "~o~";

            return bar + new string('█', bars) + "~s~" + new string('░', 10 - bars);
        }

        private string GetDirtBar(float dirt)
        {
            int bars = (int)((15 - dirt) / 1.5);
            if (bars > 10) bars = 10;
            if (bars < 0) bars = 0;

            string bar = "~g~";
            if (dirt > 10) bar = "~r~";
            else if (dirt > 5) bar = "~o~";

            return bar + new string('█', bars) + "~s~" + new string('░', 10 - bars);
        }

        private string GetCleanlinessText(float dirt)
        {
            if (dirt < 2) return "~g~Impeccable";
            if (dirt < 5) return "~b~Propre";
            if (dirt < 8) return "~y~Un peu sale";
            if (dirt < 12) return "~o~Sale";
            return "~r~Très sale";
        }

        private string GetItemIcon(string itemName)
        {
            switch (itemName)
            {
                case "Pain":
                    return "🍞";
                case "Sandwich":
                    return "🥪";
                case "Burger":
                    return "🍔";
                case "Eau":
                    return "💧";
                case "Coca Cola":
                    return "🥤";
                case "Café":
                    return "☕";
                case "Phone":
                    return "📱";
                case "Dollars":
                    return "💵";
                case "Ordinateur":
                    return "💻";
                case "Perceuse":
                    return "🔨";
                case "Menotte":
                    return "🔗";
                case "Cigarettes":
                    return "🚬";
                case "Briquet":
                    return "🔥";
                default:
                    return "📦";
            }
        }


        private NativeItem CreateSeparator()
        {
            return new NativeItem("~b~---------------------", "")
            {
                Enabled = false
            };
        }

        private void DrawAdvancedMarker(Vector3 position)
        {
            float pulseSize = 1.0f + (float)Math.Sin(Game.GameTime / 200.0f) * 0.15f;

            World.DrawMarker(
                MarkerType.VerticalCylinder,
                position,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(1.2f, 1.2f, 0.8f),
                AccentColor,
                true,
                false,
                true
            );

            World.DrawMarker(
                MarkerType.HorizontalCircleFat,
                position + new Vector3(0, 0, 0.1f),
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(pulseSize, pulseSize, 0.1f),
                System.Drawing.Color.FromArgb(80, AccentColor.R, AccentColor.G, AccentColor.B),
                true,
                false,
                true
            );
        }

        private void DrawInteractionPrompt(string text)
        {
            SetTextFont(4);
            SetTextScale(0.5f, 0.5f);
            SetTextProportional(false);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextCentre(true);
            SetTextEntry("STRING");
            DrawText(0.50f, 0.90f);
        }

        private void ShowNotification(string message, System.Drawing.Color color)
        {
            Format.ShowAdvancedNotification("🚗 Véhicule", "ShurikenRP", message);
        }

        public async Task OnTick()
        {
            // Changement de siège
            if (IsPedInAnyVehicle(GetPlayerPed(-1), false))
            {
                var vehicle = GetVehiclePedIsIn(GetPlayerPed(-1), false);
                var maxSeat = GetVehicleMaxNumberOfPassengers(vehicle) + 1;
                var controls = new List<int> { 157, 158, 160, 164, 165, 159, 161, 162 };

                for (int i = -1; i < maxSeat - 1; i++)
                {
                    if (IsControlJustPressed(0, controls[i + 1]))
                    {
                        TaskWarpPedIntoVehicle(GetPlayerPed(-1), vehicle, i);
                        PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    }
                }

                // Menu véhicule (R)
                if (IsControlJustPressed(0, 80))
                {
                    VehicleMenu();
                }
            }

            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            var closestVehicle = World.GetClosest(playerCoords, World.GetAllVehicles());

            if (closestVehicle != null)
            {
                var distance = GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, closestVehicle.Position.X, closestVehicle.Position.Y, closestVehicle.Position.Z, true);

                // Verrouillage (U)
                if (distance < 10)
                {
                    if (IsControlJustPressed(0, 303))
                    {
                        string plate = GetVehicleNumberPlateText(closestVehicle.Handle);
                        var isLock = GetVehicleDoorLockStatus(closestVehicle.Handle);
                        BaseScript.TriggerServerEvent("core:changeStateVehicle", closestVehicle.Handle, plate, isLock);
                    }
                }

                // Coffre (K)
                if (distance < 5)
                {
                    DrawInteractionPrompt("Coffre");

                    if (IsControlJustPressed(0, 311))
                    {
                        var isLock = GetVehicleDoorLockStatus(closestVehicle.Handle);
                        string plate = GetVehicleNumberPlateText(closestVehicle.Handle);

                        if (isLock != 2)
                        {
                            BaseScript.TriggerServerEvent("core:requestVehicleByPlate", plate);

                            while (Client.waitingForResponse)
                            {
                                await BaseScript.Delay(100);
                            }

                            GetVehicleBoot(plate);
                        }
                        else
                        {
                            ShowNotification("~r~✗ Véhicule verrouillé", ErrorColor);
                            PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                        }
                    }
                }
            }

            if (currentVehicle != 0 && DoesEntityExist(currentVehicle))
            {
                var vehiclePos = GetEntityCoords(currentVehicle, true);
                var dist = playerCoords.DistanceToSquared(vehiclePos);

                if (dist > MAX_MENU_DISTANCE * MAX_MENU_DISTANCE)
                {
                    foreach (var menu in Pool.ToList())
                    {
                        if (menu.Visible)
                        {
                            menu.Visible = false;
                        }
                    }

                    ShowNotification("~r~✗ Vous vous êtes trop éloigné du véhicule", ErrorColor);
                    PlaySoundFrontend(-1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    currentVehicle = 0;
                }
            }
        }
    }
}