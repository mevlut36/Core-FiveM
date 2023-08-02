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

namespace Core.Client
{
    public class VehicleSystem
    {
        ClientMain Client;
        Format Format;
        PlayerMenu PlayerMenu;
        ObjectPool Pool = new ObjectPool();

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
                Format.SendNotif("~g~Vous avez ouvert votre voiture");
            }
            else if (isLock == 1)
            {
                SetVehicleDoorsLocked(id, 2);
                PlayVehicleDoorOpenSound(id, 2);
                Format.SendNotif("~r~Vous avez fermé votre voiture");
            }
            else
            {
                Format.SendNotif("Valeur de verrouillage non valide");
            }
        }
        public void GetVehicleBoot()
        {
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            Vehicle closestVehicle = World.GetClosest<Vehicle>(playerCoords, World.GetAllVehicles());
            var isLock = GetVehicleDoorLockStatus(closestVehicle.Handle);
            string plate = GetVehicleNumberPlateText(closestVehicle.Handle);
            var menu = new NativeMenu($"{plate}", "Coffre de la voiture")
            {
                UseMouse = false
            };
            
            var distance = GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, closestVehicle.Position.X, closestVehicle.Position.Y, closestVehicle.Position.Z, true);
            if (closestVehicle != null)
            {
                if (distance < 2.5)
                {
                    if (isLock != 2)
                    {
                        Pool.Add(menu);
                        menu.Visible = true;
                        var pick = new NativeMenu("Retirer", "Retirer")
                        {
                            UseMouse = false
                        };
                        menu.AddSubMenu(pick);

                        var drop = new NativeMenu("Déposer", "Déposer")
                        {
                            UseMouse = false
                        };
                        menu.AddSubMenu(drop);
                        Pool.Add(pick);
                        Pool.Add(drop);

                        List<BootInfo> bootsToRemove = new List<BootInfo>();
                        foreach (VehicleInfo info in Client.vehicles)
                        {
                            if (info.Plate == plate)
                            {
                                foreach (BootInfo boot in info.Boot)
                                {
                                    if (boot.Type == "item")
                                    {
                                        var item = new NativeItem($"{boot.Item} ({boot.Quantity})");
                                        pick.Add(item);
                                        item.Activated += async (sender, e) =>
                                        {
                                            var textInput = await Format.GetUserInput("Quantité", "1", 4);
                                            var parsedInput = int.Parse(textInput);
                                            if (parsedInput <= boot.Quantity)
                                            {
                                                BaseScript.TriggerServerEvent("core:addItem", boot.Item, parsedInput);
                                                BaseScript.TriggerServerEvent("core:removeItemFromBoot", plate, boot.Item, parsedInput);
                                                boot.Quantity -= parsedInput;
                                                if (boot.Quantity <= 0)
                                                {
                                                    bootsToRemove.Add(boot);
                                                    pick.Remove(item);
                                                }
                                                BaseScript.TriggerServerEvent("core:requestPlayerData");
                                            }
                                            pick.Visible = false;
                                        };
                                    }
                                }
                                foreach (BootInfo boot in bootsToRemove)
                                {
                                    info.Boot.Remove(boot);
                                }

                                var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerMenu.PlayerInst.Inventory);
                                if (items != null)
                                {
                                    foreach (var item in items)
                                    {
                                        if (item != null && item.Item != null)
                                        {
                                            if (item.ItemType == "item" && item.Quantity != 0)
                                            {
                                                var invItem = new NativeItem($"{item.Item} ({item.Quantity})");
                                                drop.Add(invItem);
                                                invItem.Activated += async (sender, e) =>
                                                {
                                                    var textInput = await Format.GetUserInput("Quantité", "1", 4);
                                                    var parsedInput = int.Parse(textInput);
                                                    if (parsedInput <= item.Quantity)
                                                    {
                                                        BaseScript.TriggerServerEvent("core:removeItem", item.Item, parsedInput);
                                                        BaseScript.TriggerServerEvent("core:addItemInBoot", plate, item.Item, parsedInput, "item");
                                                        BaseScript.TriggerServerEvent("core:requestPlayerData");
                                                        BaseScript.TriggerServerEvent("core:getVehicleInfo");
                                                    }
                                                    drop.Visible = false;
                                                };
                                            } else if (item.ItemType == "weapon" && item.Quantity != 0)
                                            {
                                                var invItem = new NativeItem($"{item.Item}");
                                                drop.Add(invItem);
                                                invItem.Activated += (sender, e) =>
                                                {
                                                    BaseScript.TriggerServerEvent("core:removeItem", item.Item, 1);
                                                    BaseScript.TriggerServerEvent("core:addItemInBoot", plate, item.Item, 1, "weapon");
                                                    BaseScript.TriggerServerEvent("core:requestPlayerData");
                                                    BaseScript.TriggerServerEvent("core:getVehicleInfo");
                                                    drop.Visible = false;
                                                };
                                            }
                                        
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                
            }
            else
            {
                Debug.WriteLine("Aucun véhicule trouvé à proximité.");
                return;
            }
        }

        public void VehicleMenu()
        {
            var menu = new NativeMenu($"{GetVehicleNumberPlateText(GetVehiclePedIsIn(GetPlayerPed(-1), false))}", "Option du véhicule");
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;

            var engineHealth = (GetVehicleEngineHealth(GetVehiclePedIsIn(PlayerPedId(), false))) * 100.0 / 5000.0 * 5;
            var vehicleStatus = new NativeItem($"Etat du moteur: {String.Format("{0:0.##}", engineHealth)}%");
            menu.Add(vehicleStatus);

            var driftMode = new NativeCheckboxItem("Activer le mode Drift");
            menu.Add(driftMode);
            var driftState = false;
            driftMode.Activated += (sender, e) =>
            {
                driftState = !driftState;
                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == true)
                {
                    if (driftState)
                    {
                        SetVehicleReduceGrip(GetVehiclePedIsIn(PlayerPedId(), false), true);
                    }
                    else
                    {
                        SetVehicleReduceGrip(GetVehiclePedIsIn(PlayerPedId(), false), false);
                    }
                }
                else
                {
                    Format.SendNotif("~r~Vous n'êtes pas dans un véhicule");
                }
            };

            var doorsItem = new NativeListItem<string>("Ouvrir / Fermer une porte", "", "Avant gauche", "Avant droit", "Arrière gauche", "Arrière droit", "Capot", "Coffre", "Toutes les portes");
            var doors = new List<string>() { "Front Left", "Front Right", "Rear Left", "Rear Right", "Hood", "Trunk" };
            menu.Add(doorsItem);

            doorsItem.Activated += (sender, e) =>
            {
                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == true)
                {
                    if (GetPedInVehicleSeat(GetVehiclePedIsIn(PlayerPedId(), false), -1) == GetPlayerPed(-1))
                    {
                        var index = doorsItem.SelectedIndex;
                        if (index <= 5)
                        {
                            bool open = GetVehicleDoorAngleRatio(GetVehiclePedIsIn(GetPlayerPed(-1), false), index) > 0.1f ? true : false;

                            if (open)
                            {
                                SetVehicleDoorShut(GetVehiclePedIsIn(GetPlayerPed(-1), false), index, false);
                            }
                            else
                            {
                                SetVehicleDoorOpen(GetVehiclePedIsIn(GetPlayerPed(-1), false), index, false, false);
                            }
                        }
                        else if (doorsItem.SelectedItem == "Toutes les portes")
                        {
                            var open = false;
                            for (var door = 0; door < 5; door++)
                            {
                                open = !open;

                                if (open)
                                {
                                    SetVehicleDoorsShut(GetVehiclePedIsIn(GetPlayerPed(-1), false), false);

                                }
                                else
                                {
                                    SetVehicleDoorOpen(GetVehiclePedIsIn(GetPlayerPed(-1), false), door, false, false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Format.SendNotif("~r~Vous n'êtes pas dans un véhicule");
                }
            };

        }

        public void OnTick()
        {
            if (IsPedInAnyVehicle(GetPlayerPed(-1), false))
            {
                var seatList = new List<int> { 157, 158, 160, 164 };
                for (int i = 0; i < seatList.Count; i++)
                {
                    if (IsControlJustPressed(0, seatList[i]))
                    {
                        SetPedIntoVehicle(GetPlayerPed(-1), GetVehiclePedIsIn(GetPlayerPed(-1), false), i);
                    }
                }

                if (IsControlJustPressed(0, 80))
                {
                    VehicleMenu();
                }
            }
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            var closestVehicle = World.GetClosest(playerCoords, World.GetAllVehicles());
            if (closestVehicle != null && GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, closestVehicle.Position.X, closestVehicle.Position.Y, closestVehicle.Position.Z, true) < 10)
            {
                if (IsControlJustPressed(0, 303))
                {
                    string plate = GetVehicleNumberPlateText(closestVehicle.Handle);
                    var isLock = GetVehicleDoorLockStatus(closestVehicle.Handle);
                    BaseScript.TriggerServerEvent("core:changeStateVehicle", closestVehicle.Handle, plate, isLock);
                }
            }

            if (IsControlJustPressed(0, 311))
            {
                GetVehicleBoot();
            }
        }
    }
}
