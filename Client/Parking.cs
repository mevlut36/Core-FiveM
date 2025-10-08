using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Core.Shared;
using static CitizenFX.Core.Native.API;
using System.Linq;

namespace Core.Client
{
    public class Parking
    {
        ClientMain Client;
        Format Format;
        ObjectPool Pool = new ObjectPool();
        PlayerMenu PlayerMenu;
        public Dictionary<Vector3, List<Vector3>> parkingDict = new Dictionary<Vector3, List<Vector3>>();
        public List<Vector3> parkingEnterList = new List<Vector3>();

        List<ParkingInfo> Parkings = new List<ParkingInfo>();
        public List<VehicleInfo> CarList = new List<VehicleInfo>();

        private readonly System.Drawing.Color MenuAccentColor = System.Drawing.Color.FromArgb(255, 0, 174, 239);
        private readonly System.Drawing.Color SuccessColor = System.Drawing.Color.FromArgb(255, 46, 204, 113);
        private readonly System.Drawing.Color ErrorColor = System.Drawing.Color.FromArgb(255, 231, 76, 60);
        private readonly System.Drawing.Color WarningColor = System.Drawing.Color.FromArgb(255, 241, 196, 15);

        public Parking(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            InitializeParkings();
            CreateParkingBlips();
            BaseScript.TriggerServerEvent("core:getVehicleInfo");
        }

        private void InitializeParkings()
        {
            ParkingInfo DriftParking = new ParkingInfo(
                "Drift Parking",
                "Parking souterrain sécurisé",
                new Vector3(-2184.8f, 1099, -23.2f),
                new Vector3(-2177.4f, 1107.4f, -24.3f),
                new List<Vector3>()
                {
                    new Vector3(-2178.1f, 1094.7f, -24.3f),
                    new Vector3(-2177.8f, 1087.2f, -24.3f),
                    new Vector3(-2158.8f, 1086.8f, -24.3f),
                },
                90,
                3
            );
            Parkings.Add(DriftParking);

            ParkingInfo DesertParking = new ParkingInfo(
                "Drift Desert",
                "Parking en plein désert",
                new Vector3(1734.8f, 3318, 41.2f),
                new Vector3(1726, 3315.1f, 41.2f),
                new List<Vector3>()
                {
                    new Vector3(1726, 3315.1f, 41.2f)
                },
                90,
                1
            );
            Parkings.Add(DesertParking);

            ParkingInfo RedParking = new ParkingInfo(
                "Red Parking",
                "Parking central de la ville",
                new Vector3(-285.5f, -887.2f, 31),
                new Vector3(-358.6f, -891.4f, 31f),
                new List<Vector3>()
                {
                    new Vector3(-292.8f, -886, 31),
                    new Vector3(-300.3f, -884.6f, 31),
                    new Vector3(-298.5f, -899.5f, 31)
                },
                90,
                3
            );
            Parkings.Add(RedParking);

            ParkingInfo CentralParking = new ParkingInfo(
                "Central Parking",
                "Parking du centre-ville",
                new Vector3(216.8f, -810, 30.7f),
                new Vector3(225.5f, -755.3f, 30.8f),
                new List<Vector3>()
                {
                    new Vector3(227.6f, -789.1f, 30.6f),
                    new Vector3(239.6f, -787.7f, 30.5f),
                    new Vector3(234.6f, -802.9f, 30.4f)
                },
                90,
                3
            );
            Parkings.Add(CentralParking);

            ParkingInfo PaletoParking = new ParkingInfo(
                "Paleto Parking",
                "Parking de Paleto Bay",
                new Vector3(110.7f, 6605.2f, 31.8f),
                new Vector3(114.5f, 6611.6f, 31.8f),
                new List<Vector3>()
                {
                    new Vector3(118.9f, 6599.5f, 32),
                    new Vector3(123.5f, 6594.7f, 32),
                    new Vector3(126.8f, 6590, 32)
                },
                90,
                3
            );
            Parkings.Add(PaletoParking);

            ParkingInfo VinewoodParking = new ParkingInfo(
                "Vinewood Parking",
                "Parking de Vinewood",
                new Vector3(65.7f, 13.7f, 69),
                new Vector3(57.2f, 28.8f, 70),
                new List<Vector3>()
                {
                    new Vector3(60.5f, 17.6f, 69.1f),
                    new Vector3(54.4f, 19.3f, 69.5f)
                },
                90,
                2
            );
            Parkings.Add(VinewoodParking);

            ParkingInfo HippodromeParking = new ParkingInfo(
                "Hippodrome Parking",
                "Parking de l'hippodrome",
                new Vector3(1118.3f, 234.8f, 80.8f),
                new Vector3(1115.6f, 264.4f, 80.5f),
                new List<Vector3>()
                {
                    new Vector3(1123.5f, 243.2f, 80.8f),
                    new Vector3(1127.4f, 249.4f, 80.8f),
                    new Vector3(1113.7f, 252.8f, 80.8f)
                },
                90,
                3
            );
            Parkings.Add(HippodromeParking);

            ParkingInfo CubePlaceParking = new ParkingInfo(
                "Cube Place Parking",
                "Parking souterrain Cube Place",
                new Vector3(214.5f, -915.1f, 18.2f),
                new Vector3(231.7f, -874, 18.2f),
                new List<Vector3>()
                {
                    new Vector3(211, -883.7f, 18.2f),
                    new Vector3(222.4f, -879.3f, 18.2f),
                    new Vector3(231.5f, -885.5f, 18.2f)
                },
                90,
                3
            );
            Parkings.Add(CubePlaceParking);
        }

        private void CreateParkingBlips()
        {
            foreach (var parking in Parkings)
            {
                Blip myBlip = World.CreateBlip(parking.MenuPosition);
                myBlip.Sprite = BlipSprite.Garage;
                myBlip.Name = $"Parking - {parking.Name}";
                myBlip.Color = BlipColor.Blue;
                myBlip.IsShortRange = true;
                myBlip.Scale = 0.8f;
            }
        }

        public void SendVehicleInfo(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                ShowNotification("~r~Erreur: Le véhicule est null", ErrorColor);
                return;
            }

            if (!vehicle.Exists())
            {
                ShowNotification("~r~Vous n'êtes pas dans une voiture", ErrorColor);
                return;
            }

            var vehicleMods = vehicle.Mods;
            if (vehicleMods == null)
            {
                ShowNotification("~r~Erreur: Les modifications du véhicule sont null", ErrorColor);
                return;
            }

            if (vehicle.Exists())
            {
                VehicleInfo info = new VehicleInfo
                {
                    Model = API.GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash),
                    Plate = vehicle.Mods.LicensePlate,
                    Boot = new List<BootInfo>(),
                    EngineLevel = vehicle.Mods[VehicleModType.Engine].Index,
                    BrakeLevel = vehicle.Mods[VehicleModType.Brakes].Index,
                    ColorPrimary = (int)vehicle.Mods.PrimaryColor,
                    ColorSecondary = (int)vehicle.Mods.SecondaryColor,
                    Spoiler = vehicle.Mods[VehicleModType.Spoilers].Index,
                    Bumber_F = vehicle.Mods[VehicleModType.FrontBumper].Index,
                    Bumber_R = vehicle.Mods[VehicleModType.RearBumper].Index,
                    Skirt = vehicle.Mods[VehicleModType.SideSkirt].Index,
                    Exhaust = vehicle.Mods[VehicleModType.Exhaust].Index,
                    Chassis = vehicle.Mods[VehicleModType.Frame].Index,
                    Grill = vehicle.Mods[VehicleModType.Grille].Index,
                    Bonnet = vehicle.Mods[VehicleModType.Hood].Index,
                    Wing_L = vehicle.Mods[VehicleModType.Fender].Index,
                    Wing_R = vehicle.Mods[VehicleModType.RightFender].Index,
                    Roof = vehicle.Mods[VehicleModType.Roof].Index,
                    Engine = vehicle.Mods[VehicleModType.Engine].Index,
                    Brakes = vehicle.Mods[VehicleModType.Brakes].Index,
                    Gearbox = vehicle.Mods[VehicleModType.Transmission].Index,
                    Horn = vehicle.Mods[VehicleModType.Horns].Index,
                    Suspension = vehicle.Mods[VehicleModType.Suspension].Index,
                    Armour = vehicle.Mods[VehicleModType.Armor].Index,
                    Subwoofer = vehicle.Mods[VehicleModType.Speakers].Index,
                    Hydraulics = vehicle.Mods[VehicleModType.Hydraulics].Index,
                    Wheels = vehicle.Mods[VehicleModType.FrontWheel].Index,
                    WheelsRearOrHydraulics = vehicle.Mods[VehicleModType.RearWheel].Index,
                    PLTHolder = vehicle.Mods[VehicleModType.PlateHolder].Index,
                    PLTVanity = vehicle.Mods[VehicleModType.VanityPlates].Index,
                    Interior1 = vehicle.Mods[VehicleModType.TrimDesign].Index,
                    Interior2 = vehicle.Mods[VehicleModType.Ornaments].Index,
                    Interior3 = vehicle.Mods[VehicleModType.Dashboard].Index,
                    Interior4 = vehicle.Mods[VehicleModType.DialDesign].Index,
                    Interior5 = vehicle.Mods[VehicleModType.DoorSpeakers].Index,
                    Seats = vehicle.Mods[VehicleModType.Seats].Index,
                    Steering = vehicle.Mods[VehicleModType.SteeringWheels].Index,
                    Knob = vehicle.Mods[VehicleModType.ColumnShifterLevers].Index,
                    Plaque = vehicle.Mods[VehicleModType.Plaques].Index,
                    Ice = vehicle.Mods[VehicleModType.Speakers].Index,
                    Trunk = vehicle.Mods[VehicleModType.Trunk].Index,
                    Hydro = vehicle.Mods[VehicleModType.Hydraulics].Index,
                    EngineBay1 = vehicle.Mods[VehicleModType.EngineBlock].Index,
                    EngineBay2 = vehicle.Mods[VehicleModType.Struts].Index,
                    EngineBay3 = vehicle.Mods[VehicleModType.ArchCover].Index,
                    Chassis2 = vehicle.Mods[VehicleModType.Aerials].Index,
                    Chassis3 = vehicle.Mods[VehicleModType.Trim].Index,
                    Chassis4 = vehicle.Mods[VehicleModType.Trim].Index,
                    Chassis5 = vehicle.Mods[VehicleModType.Tank].Index,
                    Door_L = vehicle.Mods[VehicleModType.Windows].Index,
                    Door_R = vehicle.Mods[VehicleModType.Windows].Index,
                    LiveryMod = vehicle.Mods[VehicleModType.Livery].Index
                };

                BaseScript.TriggerServerEvent("core:sendVehicleInfo", JsonConvert.SerializeObject(info));

                BaseScript.Delay(1000).ContinueWith(_ =>
                {
                    RefreshVehicles();
                    ShowNotification("~g~✓ Véhicule enregistré avec succès", SuccessColor);
                });
            }
        }

        public void RefreshVehicles()
        {
            BaseScript.TriggerServerEvent("core:getVehicleInfo");
        }

        private string GetColorName(int colorId)
        {
            if (colorId >= 0 && colorId <= 12) return "~b~Noir";
            if (colorId >= 13 && colorId <= 25) return "~g~Vert";
            if (colorId >= 26 && colorId <= 36) return "~o~Orange";
            if (colorId >= 37 && colorId <= 49) return "~r~Rouge";
            if (colorId >= 50 && colorId <= 62) return "~p~Rose";
            if (colorId >= 63 && colorId <= 75) return "~q~Violet";
            if (colorId >= 76 && colorId <= 88) return "~u~Bleu";
            if (colorId >= 89 && colorId <= 101) return "~c~Jaune";
            if (colorId >= 102 && colorId <= 110) return "~h~Blanc";
            return "~s~Personnalisée";
        }

        private string GetVehicleClass(string model)
        {
            uint hash = (uint)GetHashKey(model);
            int vehClass = GetVehicleClassFromName(hash);

            switch (vehClass)
            {
                case 0: return "Compacte";
                case 1: return "Berline";
                case 2: return "SUV";
                case 3: return "Coupé";
                case 4: return "Muscle";
                case 5: return "Sport Classic";
                case 6: return "Sport";
                case 7: return "Super";
                case 8: return "Moto";
                case 9: return "Tout-terrain";
                default: return "Véhicule";
            }
        }

        public void ParkingMenu()
        {
            var playerPedId = PlayerPedId();
            var playerCoords = GetEntityCoords(playerPedId, false);
            bool isPlayerInVehicle = IsPedInAnyVehicle(playerPedId, false);

            NativeMenu menu = null;

            foreach (var parking in Parkings)
            {
                if (!isPlayerInVehicle)
                {
                    var dist = parking.MenuPosition.DistanceToSquared(playerCoords);

                    if (dist < 100)
                    {
                        DrawAdvancedMarker(parking.MenuPosition);
                    }

                    if (dist < 2 && menu == null)
                    {
                        DrawInteractionPrompt(parking.Name);

                        if (IsControlPressed(0, 38))
                        {
                            menu = CreateEnhancedParkingMenu(parking);
                            menu.Visible = true;
                        }
                    }
                }
            }

            foreach (var parking in Parkings)
            {
                if (isPlayerInVehicle)
                {
                    var distEntrer = parking.DeleteVehicle.DistanceToSquared(playerCoords);

                    if (distEntrer < 100)
                    {
                        DrawAdvancedMarker(parking.DeleteVehicle, ErrorColor);
                    }

                    if (distEntrer < 3)
                    {
                        DrawInteractionPrompt("Ranger le véhicule", "~r~");

                        if (IsControlPressed(0, 38))
                        {
                            var vehicle = GetVehiclePedIsIn(playerPedId, false);

                            ShowNotification("~y~⏳ Rangement du véhicule...", WarningColor);

                            BaseScript.Delay(1500).ContinueWith(_ =>
                            {
                                SetEntityAsMissionEntity(vehicle, true, true);
                                DeleteVehicle(ref vehicle);
                                ShowNotification("~g~Véhicule rangé avec succès", SuccessColor);
                                PlaySoundFrontend(-1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", false);
                            });
                        }
                    }
                }
            }
        }

        private NativeMenu CreateEnhancedParkingMenu(ParkingInfo parkingInfo)
        {
            var parkingList = parkingInfo.VehiclesPosition;
            int availableSpots = GetAvailableSpots(parkingList);
            int usedSpots = parkingInfo.Capacity - availableSpots;

            var menu = new NativeMenu(
                $"{parkingInfo.Name}",
                $"~b~~w~Places: ~g~{availableSpots}~w~/~o~{parkingInfo.Capacity}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 150
            };

            var statsMenu = new NativeMenu("Statistiques", "Informations sur le parking")
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            Pool.Add(statsMenu);
            menu.AddSubMenu(statsMenu);

            var infoItem = new NativeItem(
                "~h~Informations générales",
                $"~b~Nom:~w~ {parkingInfo.Name}\n" +
                $"~b~Places disponibles:~w~ ~g~{availableSpots}~w~/~o~{parkingInfo.Capacity}\n" +
                $"~b~Places occupées:~w~ ~r~{usedSpots}\n" +
                $"~b~Vos véhicules:~w~ ~b~{CarList.Count}"
            )
            {
                Enabled = false
            };
            statsMenu.Add(infoItem);

            menu.Add(new NativeItem("~b~---------------------", "") { Enabled = false });

            if (CarList.Count > 0)
            {
                var vehicleHeader = new NativeItem("~h~MES VÉHICULES", "")
                {
                    Enabled = false
                };
                menu.Add(vehicleHeader);

                var groupedVehicles = CarList.GroupBy(v => GetVehicleClass(v.Model));

                foreach (var group in groupedVehicles.OrderBy(g => g.Key))
                {
                    var classMenu = new NativeMenu(
                        $"{group.Key} ({group.Count()})",
                        $"{group.Key}"
                    )
                    {
                        MouseBehavior = MenuMouseBehavior.Disabled
                    };
                    Pool.Add(classMenu);
                    menu.AddSubMenu(classMenu);

                    foreach (var car in group.OrderBy(c => c.Model))
                    {
                        string colorPrimary = GetColorName(car.ColorPrimary);
                        string colorSecondary = GetColorName(car.ColorSecondary);

                        NativeItem item = new NativeItem(
                            $"~h~{car.Model.ToUpper()}",
                            $"~b~---------------------\n" +
                            $"~b~Plaque:~w~ ~y~{car.Plate}\n" +
                            $"~b~Classe:~w~ {GetVehicleClass(car.Model)}\n" +
                            $"~b~Couleur:~w~ {colorPrimary} / {colorSecondary}\n" +
                            $"~b~Moteur:~w~ Niveau ~g~{car.EngineLevel + 1}~w~/4\n" +
                            $"~b~Freins:~w~ Niveau ~g~{car.BrakeLevel + 1}~w~/4\n" +
                            $"~b~---------------------\n" +
                            $"~g~> Appuyez pour sortir le véhicule"
                        );

                        classMenu.Add(item);

                        item.Activated += (sender, e) =>
                        {
                            SpawnVehicleWithEffects(car, parkingList, parkingInfo);
                            menu.Visible = false;
                            classMenu.Visible = false;
                        };
                    }
                }
            }
            else
            {
                var noVehicle = new NativeItem(
                    "~r~Aucun véhicule",
                    "Vous n'avez aucun véhicule dans ce parking"
                )
                {
                    Enabled = false
                };
                menu.Add(noVehicle);
            }

            // Séparateur final
            menu.Add(new NativeItem("~b~---------------------", "") { Enabled = false });

            // Option de rafraîchissement
            var refreshItem = new NativeItem("🔄 Rafraîchir la liste", "~y~Met à jour la liste des véhicules");
            menu.Add(refreshItem);
            refreshItem.Activated += (sender, e) =>
            {
                ShowNotification("~y~⏳ Rafraîchissement en cours...", WarningColor);
                RefreshVehicles();
                BaseScript.Delay(1000).ContinueWith(_ =>
                {
                    ShowNotification("~g~✓ Liste mise à jour", SuccessColor);
                    menu.Visible = false;
                    Pool.Remove(menu);
                });
            };

            Pool.Add(menu);
            return menu;
        }

        private void SpawnVehicleWithEffects(VehicleInfo car, List<Vector3> parkingList, ParkingInfo parkingInfo)
        {
            List<Vector3> localParkingList = new List<Vector3>(parkingList);
            var parkingListOccupied = new List<Vector3>();
            List<Vector3> localParkingListOccupied = new List<Vector3>(parkingListOccupied);

            if (localParkingList.Count > 0)
            {
                Vector3 spawnPosition = GetAvailableSpawnPosition(localParkingList, localParkingListOccupied);

                if (spawnPosition != Vector3.Zero)
                {
                    ShowNotification("~y~⏳ Préparation de votre véhicule...", WarningColor);
                    PlaySoundFrontend(-1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", false);

                    var model = new Model(car.Model);
                    model.Request();
                    SetEntityAsMissionEntity(model, true, false);

                    World.CreateVehicle(model, spawnPosition, heading: parkingInfo.Rotation).ContinueWith(vehTask =>
                    {
                        if (!vehTask.IsFaulted && vehTask.Result != null && vehTask.Result.Exists())
                        {
                            var vehicle = vehTask.Result;

                            SetVehicleNumberPlateText(vehicle.Handle, $"{car.Plate}");
                            SetVehicleColours(vehicle.Handle, car.ColorPrimary, car.ColorSecondary);
                            SetVehicleDoorsLocked(vehicle.Handle, 2);
                            SetVehicleLivery(model, car.LiveryMod);
                            SetVehicleEngineOn(vehicle.Handle, false, true, false);

                            PlaySoundFromEntity(-1, "VEHICLE_UNLOCK", vehicle.Handle, "GTAO_FM_EVENTS_SOUNDSET", false, 0);

                            BaseScript.Delay(300).ContinueWith(_ =>
                            {
                                StartVehicleHorn(vehicle.Handle, 200, (uint)GetHashKey("HELDDOWN"), false);
                            });

                            FlashVehicleLights(vehicle.Handle);

                            ShowNotification($"~g~{car.Model.ToUpper()} sorti avec succès\n~w~Plaque: ~y~{car.Plate}", SuccessColor);
                            PlaySoundFrontend(-1, "GARAGE_DOOR_OPEN", "GTAO_EXEC_SECUROSERV_GARAGE_DOOR_SOUNDS", false);
                        }
                    });
                }
                else
                {
                    ShowNotification("~r~Toutes les places sont occupées\n~w~Revenez plus tard", ErrorColor);
                    PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                }
            }
            else
            {
                ShowNotification("~r~Aucune place disponible", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        private async void FlashVehicleLights(int vehicleHandle)
        {
            for (int i = 0; i < 3; i++)
            {
                SetVehicleLights(vehicleHandle, 2); // Allume
                await BaseScript.Delay(150);
                SetVehicleLights(vehicleHandle, 0); // Eteint
                await BaseScript.Delay(150);
            }
        }

        private int GetAvailableSpots(List<Vector3> parkingList)
        {
            int available = 0;
            foreach (Vector3 position in parkingList)
            {
                if (!IsPositionOccupied(position.X, position.Y, position.Z, 1, false, true, true, false, false, 0, false))
                {
                    available++;
                }
            }
            return available;
        }

        public Vector3 GetAvailableSpawnPosition(List<Vector3> parkingList, List<Vector3> parkingListOccupied)
        {
            foreach (Vector3 position in parkingList)
            {
                if (!IsPositionOccupied(position.X, position.Y, position.Z, 1, false, true, true, false, false, 0, false)
                    && !parkingListOccupied.Contains(position))
                {
                    return position;
                }
            }
            return Vector3.Zero;
        }

        private void DrawAdvancedMarker(Vector3 position, System.Drawing.Color? color = null)
        {
            var markerColor = color ?? MenuAccentColor;

            // Marker principal
            World.DrawMarker(
                MarkerType.VerticalCylinder,
                position,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(1.5f, 1.5f, 1.0f),
                markerColor,
                true,
                false,
                true
            );

            // Effet de pulsation
            float pulseSize = 1.0f + (float)Math.Sin(Game.GameTime / 200.0f) * 0.1f;
            World.DrawMarker(
                MarkerType.HorizontalCircleFat,
                position + new Vector3(0, 0, 0.1f),
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(pulseSize, pulseSize, 0.1f),
                System.Drawing.Color.FromArgb(100, markerColor.R, markerColor.G, markerColor.B),
                true,
                false,
                true
            );
        }

        private void DrawInteractionPrompt(string text, string color = "~b~")
        {
            SetTextFont(4);
            SetTextScale(0.5f, 0.5f);
            SetTextProportional(false);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextCentre(true);
            SetTextEntry("STRING");
            AddTextComponentString($"{color}[E]~w~ {text}");
            DrawText(0.50f, 0.90f);
        }

        private void ShowNotification(string message, System.Drawing.Color color)
        {
            Format.ShowAdvancedNotification("Parking System", "ShurikenRP", message);
        }

        public void OnTick()
        {
            ParkingMenu();

            var playerPos = Game.PlayerPed.Position;
            foreach (var parking in Parkings)
            {
                float safeZoneRadius = 30;

                if (World.GetDistance(playerPos, parking.MenuPosition) < safeZoneRadius)
                {
                    NetworkSetFriendlyFireOption(false);
                    Game.DisableControlThisFrame(0, Control.Attack);
                    Game.DisableControlThisFrame(2, Control.SelectWeapon);
                    Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed, true);
                    Game.DisableControlThisFrame(2, Control.MeleeAttack1);
                }
                else
                {
                    NetworkSetFriendlyFireOption(true);
                }
            }
        }
    }

    class ParkingInfo
    {
        public string Name;
        public string Description;
        public Vector3 MenuPosition;
        public Vector3 DeleteVehicle;
        public List<Vector3> VehiclesPosition;
        public int Rotation;
        public int Capacity;

        public ParkingInfo(string name, string description, Vector3 menuPosition, Vector3 deleteVehicle, List<Vector3> vehiclesPosition, int rotation, int capacity)
        {
            Name = name;
            Description = description;
            MenuPosition = menuPosition;
            DeleteVehicle = deleteVehicle;
            VehiclesPosition = vehiclesPosition;
            Rotation = rotation;
            Capacity = capacity;
        }
    }
}