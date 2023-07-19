using CitizenFX.Core;
using Core.Client;
using LemonUI;
using LemonUI.Menus;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class ConcessAuto
    {
        ClientMain Client;
        Format Format;
        Parking Parking;
        ObjectPool Pool = new ObjectPool();

        public Vector3 vehicleOut = new Vector3(-47.1f, -1113.3f, 26.44f);
        public Vector3 Vendeur = new Vector3(-41.5f, -1114.1f, 25.6f);
        bool Previsualisation_state = false;
        int Car = 0;
        Vehicle MyVehicle;

        public ConcessAuto(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            Parking = caller.Parking;
            Blip myBlip = World.CreateBlip(Vendeur);
            myBlip.Sprite = BlipSprite.PersonalVehicleCar;
            myBlip.Color = BlipColor.Yellow;
            myBlip.Name = "Concess Auto";
            myBlip.IsShortRange = true;

            BaseScript.TriggerServerEvent("core:spawnPnj", "cs_josh", Vendeur);
        }

        public async void Previsualisation(string vehicle)
        {
            if (IsPositionOccupied(vehicleOut.X, vehicleOut.Y, vehicleOut.Z, 1, false, true, true, false, false, 0, false) != true)
            {
                Previsualisation_state = true;
                var model = new Model(vehicle);

                if (!model.IsInCdImage || !model.IsValid)
                {
                    Debug.WriteLine($"Le modèle du véhicule {vehicle} n'est pas valide ou n'est pas présent dans les fichiers du jeu.");
                    return;
                }

                model.Request();

                while (!model.IsLoaded)
                {
                    await BaseScript.Delay(0);
                }

                var car = await World.CreateVehicle(model.Hash, vehicleOut, 90);

                if (car != null && car.Exists())
                {
                    car.IsPersistent = true;
                    Car = car.Handle;
                    MyVehicle = car;
                }
                else
                {
                    Debug.WriteLine($"La création du véhicule {vehicle} a échoué.");
                }
            }else
            {
                Previsualisation_state = false;
                DeleteVehicle(ref Car);
                ClearAreaOfEverything(vehicleOut.X, vehicleOut.Y, vehicleOut.Z, 40, false, false, false, false);
                Format.SendNotif("~r~Vous ne pouvez pas sortir ce véhicule. Quelque chose lui en empêche.");
            }
        }

        public void Previsualisation_Menu(Model model)
        {
            if (Previsualisation_state)
            {
                SetPedIntoVehicle(GetPlayerPed(-1), model.Hash, 1);
                Format.SendNotif("~y~Prévisualisation de votre véhicule.\n ~w~Regarder derrière vous");

                var color_list = new Dictionary<string, List<int>>()
                {
                    { "Rouge", new List<int>() { 255, 0, 0} },
                    { "Bleu", new List<int>() { 0, 0, 255} },
                    { "Vert", new List<int>() { 0, 255, 0} },
                    { "Jaune", new List<int>() { 255, 255, 0} },
                    { "Noir", new List<int>() { 0, 0, 0} },
                    { "Violet", new List<int>() { 140, 0, 255} }
                };

                var menu = new NativeMenu("Prévisualisation")
                {
                    Visible = true,
                    UseMouse = false
                };
                Pool.Add(menu);

                var couleur = new NativeListItem<string>("Couleur", "Choisis une couleur", color_list.Keys.ToArray());
                menu.Add(couleur);

                couleur.ItemChanged += (sender, e) =>
                {
                    var key = couleur.SelectedItem;
                    if (color_list.TryGetValue(key, out var rgb))
                    {
                        SetVehicleCustomPrimaryColour(Car, rgb[0], rgb[1], rgb[2]);
                    }
                };

                var buy = new NativeItem("~g~Acheter");
                menu.Add(buy);

                buy.Activated += async (sender, e) =>
                {
                    var cost = GetVehicleModelValue(model);
                    BaseScript.TriggerServerEvent("core:getPlayerMoney");
                    await BaseScript.Delay(100);

                    if (Client.PlayerMoney >= cost)
                    {
                        Format.SendNotif("~g~Vous avez bien acheté le véhicule.\n Il sera livré dans votre garage dans quelques minutes...");
                        BaseScript.TriggerServerEvent("core:transaction", cost);

                        Parking.SendVehicleInfo(MyVehicle);
                        BaseScript.TriggerServerEvent("core:getVehicleInfo");
                        menu.Visible = false;
                    }
                    else
                    {
                        Format.SendNotif("~r~Vous n'avez pas assez d'argent.");
                    }
                };

                menu.Closed += (sender, e) =>
                {
                    Previsualisation_state = false;
                    DeleteEntity(ref Car);
                    MenuShop();
                };
            }
        }

        public static string GeneratePlate()
        {
            string alphabet = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            var num = "0123456789";
            Random random = new Random();
            var first = new char[2];
            var second = new char[4];
            var third = new char[2];
            for (int i = 0; i < 2; i++)
            {
                first[i] = num[random.Next(0, num.Length)];
            }
            for (int i = 0; i < 4; i++)
            {
                second[i] = alphabet[random.Next(0, alphabet.Length)];
            }
            for (int i = 0; i < 2; i++)
            {
                third[i] = num[random.Next(0, num.Length)];
            }
            return new string(first) + new string(second) + new string(third);
        }

        private bool IsExcludedVehicleClass(int vehicleClass)
        {
            int[] excludedClasses = { 10, 11, 14, 15, 16, 17, 18, 19, 20, 21, 22 };
            return excludedClasses.Contains(vehicleClass);
        }

        private bool IsCringeVehicle(string vehicleName)
        {
            List<string> cringeVehicles = new List<string> { "Oppressor", "Kuruma2", "Ruiner2", "Ruiner3", "Dukes2", "Boxville3", "Boxville4", "Boxville5", "Speedo2", "Burrito4", "Pony2", "Rumpo3", "Burrito2", "Burrito3", "Taco", "Romero", "Cog552", "Schafter6", "Asea2", "Emperor3", "Schafter5", "Cognoscenti2", "Limo2", "Dune2", "Dune", "NightShark", "Technical2", "Marshall", "Technical3", "Blazer2", "Dune5", "TrophyTruck2", "Dune4", "Monster", "RancherXL2", "RangerXL2", "Insurgent2", "Mesa2", "Mesa3", "XLS2", "Dubsta2", "Baller5", "Baller6", "Baller4", "JB700", "Dilettante2", "Voltic2" };
            return cringeVehicles.Contains(vehicleName);
        }


        public void MenuShop()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var distance = playerCoords.DistanceToSquared(Vendeur);
            if (distance < 4)
            {
                var mainMenu = new NativeMenu("Catalogue", "Bienvenue sur le catalogue")
                {
                    UseMouse = false,
                    HeldTime = 100
                };
                var vehicleNames = System.Enum.GetNames(typeof(VehicleHash));
                var vehicleClasses = new Dictionary<int, List<string>>
                {
                    { 25, new List<string>(){ "twingo" } }
                };

                foreach (var vehicleName in vehicleNames)
                {
                    System.Enum.TryParse(vehicleName, out VehicleHash vehicleHash);
                    var vehicleClass = GetVehicleClassFromName((uint)vehicleHash);

                    if (IsExcludedVehicleClass(vehicleClass) || IsCringeVehicle(vehicleName))
                    {
                        continue;
                    }

                    if (!vehicleClasses.ContainsKey(vehicleClass))
                    {
                        vehicleClasses.Add(vehicleClass, new List<string>());
                    }
                    vehicleClasses[vehicleClass].Add(vehicleName);
                }


                foreach (var vehicleClass in vehicleClasses.Keys)
                {
                    var className = GetVehicleClassName(vehicleClass);
                    var classMenu = new NativeMenu(className, className)
                    {
                        UseMouse = false
                    }; ;
                    mainMenu.AddSubMenu(classMenu);

                    foreach (var vehicleName in vehicleClasses[vehicleClass])
                    {
                        var vehicleHash = GetHashKey(vehicleName);
                        var vehicleItem = new NativeItem(vehicleName, $"Acheter {vehicleName}", $"{GetVehicleModelMonetaryValue(vehicleHash)}");
                        classMenu.Add(vehicleItem);

                        vehicleItem.Activated += (sender, args) =>
                        {
                            Previsualisation(vehicleName);
                            Previsualisation_Menu(vehicleName);
                            classMenu.Visible = false;
                        };
                    }
                    Pool.Add(classMenu);
                }

                Pool.Add(mainMenu);
                mainMenu.Visible = true;
            }
        }

        public string GetVehicleClassName(int vehicleClass)
        {
            switch (vehicleClass)
            {
                case 0: return "Compacts";
                case 1: return "Sedans";
                case 2: return "SUVs";
                case 3: return "Coupes";
                case 4: return "Muscle";
                case 5: return "Sports Classics";
                case 6: return "Sports";
                case 7: return "Super";
                case 8: return "Motorcycles";
                case 9: return "Off-road";
                case 10: return "Industrial";
                case 11: return "Utility";
                case 12: return "Vans";
                case 13: return "Cycles";
                case 14: return "Boats";
                case 15: return "Helicopters";
                case 16: return "Planes";
                case 17: return "Service";
                case 18: return "Emergency";
                case 19: return "Military";
                case 20: return "Commercial";
                case 21: return "Trains";
                default: return "Unknown";
            }
        }

        public void OnTick()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var dist = Vendeur.DistanceToSquared(playerCoords);
            
            if (dist < 30)
            {
                Format.SetMarker(Vendeur, MarkerType.CarSymbol);
                if (dist < 4)
                {
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir le catalogue");
                }
            }
            if (IsControlPressed(0, 38))
            {
                MenuShop();
            }
            if (Previsualisation_state)
            {
                
                float carDist = vehicleOut.DistanceToSquared(playerCoords);
                if (carDist > 50)
                {
                    Previsualisation_state = false;
                    Format.SendNotif("Et non gamin, où croyais-tu aller comme ça ?");
                    DeleteEntity(ref Car);
                }
            }
        }

    }
}