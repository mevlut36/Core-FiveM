using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class PlayerMenu
    {
        public Format Format;
        public ClientMain Client;
        public ObjectPool Pool;
        public Parking Parking;

        public PlayerMenu(ClientMain caller)
        {
            Client = caller;
            Pool = caller.Pool;
            Format = caller.Format;
            Parking = caller.Parking;
        }

        public PlayerInstance PlayerInst = new PlayerInstance
        {
            Firstname = "",
            Lastname = "",
            Bitcoin = 0,
            Birth = "",
            Clothes = "",
            Money = 0,
            Bills = "",
            Inventory = ""
        };

        public void F5Menu()
        {
            if (IsControlPressed(0, 166))
            {
                var mainMenu = new NativeMenu("F5", "Menu personnel")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    Visible = true,
                    UseMouse = false
                };

                var shopMenu = new NativeMenu("Boutique", "~o~Boutique")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                shopMenu.Add(new NativeItem("[V.I.P] Aucun"));
                shopMenu.Add(new NativeItem($"Bitcoins: {PlayerInst.Bitcoin}"));

                var vip = new NativeMenu("VIP", "VIP")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var vip_1 = new NativeItem("~f~~h~VIP~h~", "", "~g~500~w~ Bitcoins /mois");
                var vip_2 = new NativeItem("~y~~h~VIP+~h~", "", "~g~700~w~ Bitcoins /mois");
                var vip_3 = new NativeItem("~q~~h~MVP~h~", "", "~g~1000~w~ Bitcoins /mois");
                vip.Add(vip_1);
                vip.Add(vip_2);
                vip.Add(vip_3);
                shopMenu.AddSubMenu(vip);

                var carImport = new NativeMenu("Véhicules import", "Véhicules import")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                VehicleImport[] values = (VehicleImport[])Enum.GetValues(typeof(VehicleImport));

                foreach (VehicleImport vehicle in values)
                {
                    DisplayAttribute displayAttribute = Format.GetDisplayAttribute(vehicle);
                    var vehicleItem = new NativeItem($"{displayAttribute.Name}", "", $"~y~{displayAttribute.Price} BTC");
                    carImport.Add(vehicleItem);
                    vehicleItem.Activated += async (sender, args) =>
                    {
                        BaseScript.TriggerServerEvent("core:bitcoinTransaction", displayAttribute.Price);
                        var vehicleImport = await World.CreateVehicle(new Model(GetHashKey(displayAttribute.VehicleName)), GetEntityCoords(GetPlayerPed(-1), true));
                        Parking.SendVehicleInfo(vehicleImport);
                        BaseScript.TriggerServerEvent("core:getVehicleInfo");
                    };
                }
                shopMenu.AddSubMenu(carImport);

                var lootbox = new NativeMenu("Caisse du mois", "Caisse du mois")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                shopMenu.AddSubMenu(lootbox);

                Pool.Add(vip);
                Pool.Add(carImport);
                Pool.Add(lootbox);

                var menuInfo = new NativeMenu("Informations", "Informations")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                menuInfo.Add(new NativeItem($"~g~${PlayerInst.Money}"));
                menuInfo.Add(new NativeItem($"{PlayerInst.Firstname} {PlayerInst.Lastname}"));
                menuInfo.Add(new NativeItem($"Né le {PlayerInst.Birth}"));

                var inventoryMenu = new NativeMenu("Inventaire", "Inventaire")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerInst.Inventory);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item != null && item.Item != null)
                        {
                            var invItem = new NativeListItem<string>($"{item.Item} ({item.Quantity})", "", "Utiliser", "Donner");
                            inventoryMenu.Add(invItem);
                            invItem.Activated += (sender, e) =>
                            {
                                ItemAction(invItem.SelectedItem, item.Item);
                            };
                        }
                    }
                }

                var clothMenu = new NativeMenu("Vêtements", "Vêtements")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };

                var billsMenu = new NativeMenu("Factures", "Factures")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var bills = JsonConvert.DeserializeObject<List<Bills>>(PlayerInst.Bills);
                if (bills != null)
                {
                    foreach (var billsItem in bills)
                    {
                        var billItem = new NativeItem($"{billsItem.Company}", $"Par {billsItem.Author} le {billsItem.Date}", $"~g~${billsItem.Amount}");
                        billsMenu.Add(billItem);
                        billItem.Activated += (sender, e) =>
                        {
                            var selectedBill = bills.FirstOrDefault(b => b.Company == billsItem.Company && b.Amount == billsItem.Amount);
                            if (selectedBill != null)
                            {
                                if (PlayerInst.Money >= billsItem.Amount)
                                {
                                    bills.Remove(selectedBill);
                                    billsMenu.Remove(billItem);
                                    BaseScript.TriggerServerEvent("core:payBill", billsItem.Company, billsItem.Amount);
                                }
                                else
                                {
                                    Format.SendNotif("Tu n'as pas assez d'~r~argent...");
                                }
                            }
                        };
                    }
                }

                var carMenu = new NativeMenu("Gestion du véhicule", "Gestion du véhicule")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };

                var weaponsMenu = new NativeMenu("Armes", "Armes")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var weapons = JsonConvert.DeserializeObject<List<Weapon>>(PlayerInst.Inventory);
                if (weapons != null)
                {
                    foreach (var weapon in weapons)
                    {
                        var ammo = 0;
                        foreach (var item in items)
                        {
                            if (item.Item == "Munitions")
                            {
                                ammo = item.Quantity;
                                break;
                            }
                        }
                        if (weapon.WeaponName != null)
                        {
                            var weaponItem = new NativeListItem<string>($"{weapon.WeaponName}", "", "Équiper", "Déséquiper");
                            weaponsMenu.Add(weaponItem);
                            weaponItem.Activated += (sender, e) =>
                            {
                                if (weaponItem.SelectedItem == "Équiper")
                                {
                                    GiveWeaponToPed(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weapon.WeaponName}"), 0, false, false);
                                    SetPedAmmo(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weapon.WeaponName}"), ammo);
                                }
                                else
                                {
                                    RemoveWeaponFromPed(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weapon.WeaponName}"));
                                }
                            };
                        }
                    }
                }

                SetSubmenu(mainMenu, shopMenu, menuInfo, inventoryMenu, clothMenu, billsMenu, carMenu, weaponsMenu);
                SetPool(mainMenu, shopMenu, menuInfo, inventoryMenu, clothMenu, billsMenu, carMenu, weaponsMenu);
            }
        }

        public void F6Menu()
        {

        }

        public void ItemAction(string action, string item)
        {
            if (action == "Utiliser")
            {
                switch(item)
                {
                    case "Pain":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 3000);
                        SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1))+10);
                        break;
                    case "Eau":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 3000);
                        break;
                    case "Outil de crochetage":
                        Format.PlayAnimation("anim@heists@humane_labs@emp@hack_door", "hack_intro", 10000);
                        break;
                    case "Dollars":
                        BaseScript.TriggerServerEvent("appart:callPolice"); // TEST
                        break;
                    case "Phone":
                        break;
                }
            }
        }

        public void SetPool(params NativeMenu[] menus)
        {
            foreach (var element in menus)
            {
                Pool.Add(element);
            }
        }

        public void SetSubmenu(NativeMenu mainMenu, params NativeMenu[] subMenus)
        {
            foreach (var element in subMenus)
            {
                mainMenu.AddSubMenu(element);
            }
        }

        public void GetPlayerData(string json)
        {
            var player = JsonConvert.DeserializeObject<PlayerInstance>(json);
            PlayerInst.Firstname = player.Firstname;
            PlayerInst.Lastname = player.Lastname;
            PlayerInst.Bitcoin = player.Bitcoin;
            PlayerInst.Birth = player.Birth;
            PlayerInst.Clothes = player.Clothes;
            PlayerInst.Money = player.Money;
            PlayerInst.Bills = player.Bills;
            PlayerInst.Inventory = player.Inventory;
        }

    }
}
