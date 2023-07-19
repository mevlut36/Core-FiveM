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
        public void SetVehicleDoorsState()
        {
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            Vehicle closestVehicle = World.GetClosest<Vehicle>(playerCoords, World.GetAllVehicles());
            if (closestVehicle != null && GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, closestVehicle.Position.X, closestVehicle.Position.Y, closestVehicle.Position.Z, true) < 10)
            {
                string plate = GetVehicleNumberPlateText(closestVehicle.Handle);
                var isLock = GetVehicleDoorLockStatus(closestVehicle.Handle);
                BaseScript.TriggerServerEvent("core:changeStateVehicle", closestVehicle.Handle, plate, isLock);
            }
            else
            {
                Debug.WriteLine("Aucun véhicule trouvé à proximité.");
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
                                                    info.Boot.Remove(boot);
                                                    pick.Remove(item);
                                                }
                                                BaseScript.TriggerServerEvent("core:requestPlayerData");
                                            }
                                            pick.Visible = false;
                                        };
                                    } else if (boot.Type == "weapon")
                                    {
                                        var item = new NativeItem($"{boot.Item}");
                                        pick.Add(item);
                                        item.Activated += (sender, e) =>
                                        {
                                            BaseScript.TriggerServerEvent("core:addItem", boot.Item, 1);
                                            BaseScript.TriggerServerEvent("core:removeItemFromBoot", plate, boot.Item, 1);
                                            if (boot.Quantity <= 0)
                                            {
                                                info.Boot.Remove(boot);
                                                pick.Remove(item);
                                            }
                                            BaseScript.TriggerServerEvent("core:requestPlayerData");
                                            BaseScript.TriggerServerEvent("core:getVehicleInfo");
                                            pick.Visible = false;
                                        };
                                    }
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

            }
            if (IsControlJustPressed(0, 303))
            {
                SetVehicleDoorsState();
            }

            if (IsControlJustPressed(0, 311))
            {
                GetVehicleBoot();
            }
        }
    }
}
