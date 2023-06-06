using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
using LemonUI.Menus;
using Mono.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class VehicleInfo
    {
        public string Model { get; set; }
        public string Plate { get; set; }
        public int EngineLevel { get; set; }
        public int BrakeLevel { get; set; }
        public int ColorPrimary { get; set; }
        public int ColorSecondary { get; set; }
    }

    public class Parking
    {
        public ClientMain Client;
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;
        
        public Dictionary<Vector3, List<Vector3>> parkingDict = new Dictionary<Vector3, List<Vector3>>();
        public List<Vector3> parkingEnterList = new List<Vector3>();

        public Vector3 redParkingKey = new Vector3(-285.5f, -887.2f, 31);
        List<Vector3> redParkingValues = new List<Vector3>()
        {
            new Vector3(-292.8f, -886, 31),
            new Vector3(-300.3f, -884.6f, 31),
            new Vector3(-298.5f, -899.5f, 31)
        };

        Vector3 centralParkingKey = new Vector3(216.8f, -810, 30.7f);
        List<Vector3> centralParkingValues = new List<Vector3>()
        {
            new Vector3(227.6f, -789.1f, 30.6f),
            new Vector3(239.6f, -787.7f, 30.5f),
            new Vector3(234.6f, -802.9f, 30.4f)
        };
        

        Vector3 paletoParkingKey = new Vector3(110.7f, 6605.2f, 31.8f);
        List<Vector3> paletoParkingValues = new List<Vector3>()
        {
            new Vector3(118.9f, 6599.5f, 32),
            new Vector3(123.5f, 6594.7f, 32),
            new Vector3(126.8f, 6590, 32)
        };

        Vector3 vinewoodParkingKey = new Vector3(65.7f, 13.7f, 69);
        List<Vector3> vinewoodParkingValues = new List<Vector3>()
        {
            new Vector3(60.5f, 17.6f, 69.1f),
            new Vector3(54.4f, 19.3f, 69.5f)
        };

        public List<VehicleInfo> CarList = new List<VehicleInfo>();
        public Parking(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;

            parkingDict.Add(redParkingKey, redParkingValues);
            parkingDict.Add(centralParkingKey, centralParkingValues);
            parkingDict.Add(paletoParkingKey, paletoParkingValues);
            parkingDict.Add(vinewoodParkingKey, vinewoodParkingValues);

            parkingEnterList.Add(new Vector3(-358.6f, -891.4f, 31f));
            parkingEnterList.Add(new Vector3(225.5f, -755.3f, 30.8f));
            parkingEnterList.Add(new Vector3(114.5f, 6611.6f, 31.8f));
            parkingEnterList.Add(new Vector3(57.2f, 28.8f, 70));
            Debug.WriteLine("Concess");
            BaseScript.TriggerServerEvent("core:getVehicleInfo");
        }

        public void RegisterAllEvents()
        {
            Client.AddEvent("core:sendVehicleInfos", new Action<string, int>(GetVehicles));
        }

        public void GetVehicles(string json, int player)
        {
            if (json == null)
            {
                return;
            }

            List<VehicleInfo> vehicles = JsonConvert.DeserializeObject<List<VehicleInfo>>(json);
            if (vehicles != null && vehicles.Count > 0)
            {
                foreach (VehicleInfo info in vehicles)
                {
                    CarList.Add(info);
                }
            }
        }

        public void SendVehicleInfo(Vehicle vehicle)
        {
            Ped ped = Game.PlayerPed;
            if (vehicle.Exists())
            {
                VehicleInfo info = new VehicleInfo
                {
                    Model = API.GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash),
                    Plate = vehicle.Mods.LicensePlate,
                    EngineLevel = vehicle.Mods[VehicleModType.Engine].Index,
                    BrakeLevel = vehicle.Mods[VehicleModType.Brakes].Index,
                    ColorPrimary = (int)vehicle.Mods.PrimaryColor,
                    ColorSecondary = (int)vehicle.Mods.SecondaryColor
                };

                List<VehicleInfo> cars = new List<VehicleInfo>();
                cars.Add(info);

                string json = JsonConvert.SerializeObject(cars);

                BaseScript.TriggerServerEvent("core:sendVehicleInfo", json);
            }
            else
            {
                Format.SendNotif("Vous n'êtes pas dans une voiture");
            }
        }


        public void ParkingMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            float dist = 0;
            foreach (var entry in parkingDict)
            {
                Format.SetMarker(entry.Key, MarkerType.CarSymbol);
                dist = entry.Key.DistanceToSquared(playerCoords);
                if (dist < 2)
                {
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir la liste des véhicules");
                    // Sortie de véhicule
                    if (IsControlPressed(0, 38))
                    {
                        var menu = new NativeMenu("Garage", "Sortir un véhicule");
                        var parkingList = entry.Value;
                        var parkingListOccupied = new List<Vector3>();
                        foreach (var car in CarList)
                        {
                            NativeItem item = new NativeItem(car.Model.ToString().ToUpper());
                            menu.Add(item);
                            item.Activated += (sender, e) =>
                            {
                                List<Vector3> localParkingList = new List<Vector3>(parkingList);
                                List<Vector3> localParkingListOccupied = new List<Vector3>(parkingListOccupied);

                                if (localParkingList.Count > 0)
                                {
                                    Vector3 spawnPosition = GetAvailableSpawnPosition(localParkingList, localParkingListOccupied);
                                    if (spawnPosition != Vector3.Zero)
                                    {
                                        var model = new Model(car.Model);
                                        model.Request();
                                        SetEntityAsMissionEntity(model, true, false);
                                        World.CreateVehicle(model, spawnPosition, heading: 30).ContinueWith(vehTask =>
                                        {
                                            if (!vehTask.IsFaulted && vehTask.Result != null && vehTask.Result.Exists())
                                            {
                                                SetVehicleNumberPlateText(vehTask.Result.Handle, $"{car.Plate}");
                                                SetVehicleColours(vehTask.Result.Handle, car.ColorPrimary, car.ColorSecondary);
                                            }
                                            else
                                            {
                                                Format.SendNotif("~r~Erreur lors de la création du véhicule.");
                                            }
                                        });
                                    }
                                    else
                                    {
                                        Format.SendNotif("~r~Toutes les positions de stationnement sont occupées.");
                                    }
                                }
                                else
                                {
                                    Format.SendNotif("~r~Vous ne pouvez pas sortir ce véhicule. Quelque chose l'en empêche.");
                                }
                            };
                        }

                        menu.UseMouse = false;
                        Pool.Add(menu);
                        menu.Visible = true;
                    }
                }
            }
            foreach(var entry in parkingEnterList)
            {
                float distEntrer = entry.DistanceToSquared(playerCoords);
                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == true)
                {
                    Format.SetMarker(entry, MarkerType.CarSymbol);
                    if (distEntrer < 3)
                    {
                        // Entrer de véhicule
                        Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour faire rentrer le véhicule");
                        if (IsControlPressed(0, 38))
                        {
                            var vehicle = GetVehiclePedIsIn(GetPlayerPed(-1), false);
                            SetEntityAsMissionEntity(vehicle, true, true);
                            DeleteVehicle(ref vehicle);
                        }
                    }
                }
            }
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

        public void OnTick()
        {
            ParkingMenu();
            if (IsControlJustPressed(0, 167))
            {
            }
        }

    }
}