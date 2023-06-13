using CitizenFX.Core;
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
        public ClientMain Client;
        public Format Format;
        public PlayerMenu PlayerMenu;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;

        public VehicleSystem(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;
        }

        public void SetVehicleDoorsState()
        {
            Vehicle closestVehicle = World.GetClosest<Vehicle>(GetEntityCoords(GetPlayerPed(-1), true), World.GetAllVehicles());
            if (closestVehicle != null)
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

        public void GetVehicleBoot()
        {
            Vehicle closestVehicle = World.GetClosest<Vehicle>(GetEntityCoords(GetPlayerPed(-1), true), World.GetAllVehicles());
            var isLock = GetVehicleDoorLockStatus(closestVehicle.Handle);
            string plate = GetVehicleNumberPlateText(closestVehicle.Handle);
            var menu = new NativeMenu($"{plate}", "Coffre de la voiture")
            {
                Visible = true,
                TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                UseMouse = false
            };
            Pool.Add(menu);

            if (closestVehicle != null)
            {
                if (isLock != 2)
                {
                    var pick = new NativeMenu("Retirer", "Retirer")
                    {
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                        UseMouse = false
                    };
                    menu.AddSubMenu(pick);

                    var drop = new NativeMenu("Déposer", "Déposer")
                    {
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
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
                                        if (item.Type == "item" && item.Quantity != 0)
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
                                        } else if (item.Type == "weapon" && item.Quantity != 0)
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
            else
            {
                Debug.WriteLine("Aucun véhicule trouvé à proximité.");
                return;
            }
        }

        public void OnTick()
        {
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
