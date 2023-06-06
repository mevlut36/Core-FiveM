using CitizenFX.Core;
using Core.Client;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Game = CitizenFX.Core.Game;

namespace Core.Client
{
    public class ConcessAuto
    {
        public ClientMain Client;
        public Format Format;
        public Parking Parking;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;

        public Vector3 vehicleOut = new Vector3(-47.1f, -1113.3f, 26.44f);

        public Vector3 Vendeur = new Vector3(-41.5f, -1114.1f, 25.4f);

        public ConcessAuto(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            Parking = caller.Parking;
        }

        public void SpawnPnj()
        {
            Client.SpawnPnj(PedHash.Michael, Vendeur.X, Vendeur.Y, Vendeur.Z, 30);
        }

        public void AddCarEvent(Model model)
        {
            if (IsPositionOccupied(vehicleOut.X, vehicleOut.Y, vehicleOut.Z, 1, false, true, true, false, false, 0, false) != true)
            {
                model.Request();
                SetEntityAsMissionEntity(model, true, false);
                World.CreateVehicle(model, vehicleOut, heading: 90);
            }
            else
            {
                ClearAreaOfEverything(vehicleOut.X, vehicleOut.Y, vehicleOut.Z, 10, false, false, false, false);
                Format.SendNotif("~r~Vous ne pouvez pas sortir ce véhicule. Quelque chose lui en empêche.");
            }
        }

        bool Previsualisation_state = false;
        int Car = 0;
        Vehicle MyVehicle;
        string car_save = "";
        Model model_save = 0;
        List<string> carInfo = new List<string>();

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
                var menu = new NativeMenu("Prévisualisation");
                menu.TitleFont = CitizenFX.Core.UI.Font.ChaletLondon;
                Pool.Add(menu);
                var couleur = new NativeListItem<string>("Couleur", "Choisis une couleur", color_list.Keys.ToArray());
                menu.Add(couleur);
                couleur.ItemChanged += (sender, e) =>
                {
                    var key = couleur.SelectedItem;
                    var rgb = 1;
                    var matches = color_list.Where(item => item.Key.Equals(key) || item.Value.Contains(rgb));

                    foreach (var match in matches)
                    {
                        SetVehicleCustomPrimaryColour(Car, match.Value[0], match.Value[1], match.Value[2]);
                    }

                };
                var buy = new NativeItem("~g~Acheter");
                menu.Add(buy);
                buy.Activated += (sender, e) =>
                {
                    Format.SendNotif("~g~Vous avez bien acheté le véhicule, rendez vous derrière le Concessionnaire.");
                    model_save = new Model(car_save);
                    Parking.SendVehicleInfo(MyVehicle);
                    BaseScript.TriggerServerEvent("core:getVehicleInfo");
                    menu.Visible = false;
                };
                menu.Visible = true;
                menu.UseMouse = false;

                menu.Closed += (sender, e) =>
                {
                    Previsualisation_state = false;
                    DeleteEntity(ref Car);
                };
            }
        }
        public void SpawnModel(Model model, string color)
        {
            SetEntityAsMissionEntity(model, true, false);
            var car = CreateVehicle((uint)model.Hash, -31, -1091, 26.5f, 325, true, false);
            model.Request();
            if (color == "Rouge")
            {
                SetVehicleCustomPrimaryColour(car, 255, 0, 0);
                carInfo.Add("[255, 0, 0]");
            }
            if (color == "Bleu")
            {
                SetVehicleCustomPrimaryColour(car, 0, 0, 255);
                carInfo.Add("[0, 0, 255]");
            }
            if (color == "Vert")
            {
                SetVehicleCustomPrimaryColour(car, 0, 255, 0);
                carInfo.Add("[0, 255, 0]");
            }
            if (color == "Jaune")
            {
                SetVehicleCustomPrimaryColour(car, 255, 255, 0);
                carInfo.Add("[255, 255, 0]");
            }
            if (color == "Noir")
            {
                SetVehicleCustomPrimaryColour(car, 0, 0, 0);
                carInfo.Add("[0, 0, 0]");
            }
            if (color == "Violet")
            {
                SetVehicleCustomPrimaryColour(car, 140, 0, 255);
                carInfo.Add("[140, 0, 255]");
            }
            SetVehicleNumberPlateText(car, $"{GeneratePlate()}");
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

        public void MenuShop()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var distance = GetDistanceBetweenCoords(Vendeur.X, Vendeur.Y, Vendeur.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);
            if (distance < 4)
            {
                Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir le catalogue");
                var mainMenu = new NativeMenu("Catalogue", "Bienvenue sur le catalogue")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var vehicleNames = Enum.GetNames(typeof(VehicleHash));
                var vehicleClasses = new Dictionary<int, List<string>>();

                foreach (var vehicleName in vehicleNames)
                {
                    Enum.TryParse(vehicleName, out VehicleHash vehicleHash);
                    var vehicleClass = GetVehicleClassFromName((uint)vehicleHash);
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
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                        UseMouse = false
                    }; ;
                    mainMenu.AddSubMenu(classMenu);

                    foreach (var vehicleName in vehicleClasses[vehicleClass])
                    {
                        var vehicleItem = new NativeItem(vehicleName, $"Acheter {vehicleName}");
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
            if (IsControlPressed(0, 38))
            {
                MenuShop();
            }
            if (Previsualisation_state)
            {
                var playerCoords = GetEntityCoords(PlayerPedId(), false);
                float dist = vehicleOut.DistanceToSquared(playerCoords);
                if (dist > 50)
                {
                    Previsualisation_state = false;
                    Format.SendNotif("Et non gamin, où croyais-tu aller comme ça ?");
                    DeleteEntity(ref Car);
                }
            }
        }

    }
}
