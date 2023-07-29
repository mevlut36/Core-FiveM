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
using Core.Shared;

namespace Core.Client
{
    public class LTDShop
    {
        ClientMain Client;
        Format Format;
        ObjectPool Pool = new ObjectPool();
        PlayerMenu PlayerMenu;

        List<LTDShopInfo> LTDShops = new List<LTDShopInfo>();
        List<LTDItems> LTDItems = new List<LTDItems>();

        public LTDShop(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            // LTD's ITEM
            LTDItems item1 = new LTDItems("Pain", "N'oubliez pas de dire Bismillah avant de manger :)", 25);
            LTDItems.Add(item1);

            LTDItems item2 = new LTDItems("Eau", "N'oubliez pas de dire Bismillah avant de boire :)", 25);
            LTDItems.Add(item2);

            LTDItems item3 = new LTDItems("Phone", "Excellent téléphone", 1000);
            LTDItems.Add(item3);

            // LTD LIST
            LTDShopInfo ltd0 = new LTDShopInfo("Cube Place", new Vector3(190.1f, -889.8f, 29.8f), new Vector3(188.4f, -889.3f, 29.8f), new Vector3(187.6f, -899.5f, 30.6f));
            LTDShops.Add(ltd0);
            
            LTDShopInfo ltd1 = new LTDShopInfo("Strawberry", new Vector3(26.2f, -1346.9f, 28.5f), new Vector3(24.3f, -1346.6f, 28.5f), new Vector3(33.1f, -1347.9f, 29.3f));
            LTDShops.Add(ltd1);

            LTDShopInfo ltd2 = new LTDShopInfo("Davis", new Vector3(-48, -1756.8f, 28.5f), new Vector3(-46.4f, -1758.1f, 28.6f), new Vector3(-56.7f, -1752.3f, 29.3f));
            LTDShops.Add(ltd2);

            LTDShopInfo ltd3 = new LTDShopInfo("Chumash", new Vector3(-3243.2f, 1001.8f, 11.9f), new Vector3(-3243.2f, 999.9f, 11.9f), new Vector3(-3240.9f, 1008.7f, 12.8f));
            LTDShops.Add(ltd3);

            LTDShopInfo ltd4 = new LTDShopInfo("Banham Canyon", new Vector3(-3040.9f, 585.5f, 7.1f), new Vector3(-3040.9f, 583.9f, 7.1f), new Vector3(-3040.9f, 592.9f, 7.9f));
            LTDShops.Add(ltd4);

            LTDShopInfo ltd5 = new LTDShopInfo("Banham Canyon", new Vector3(-2968.3f, 390.3f, 14.2f), new Vector3(-2966.1f, 390.3f, 14.2f), Vector3.Zero);
            LTDShops.Add(ltd5);

            LTDShopInfo ltd6 = new LTDShopInfo("Richman Glen", new Vector3(-1820.7f, 792.6f, 137.3f), new Vector3(-1819.2f, 792.6f, 137.3f), new Vector3(-1827.1f, 785.4f, 138.2f));
            LTDShops.Add(ltd6);

            LTDShopInfo ltd7 = new LTDShopInfo("Morningwood", new Vector3(-1487.2f, -379.4f, 39.3f), new Vector3(-1485.6f, -378.1f, 39.2f), Vector3.Zero);
            LTDShops.Add(ltd7);
            
            LTDShopInfo ltd8 = new LTDShopInfo("Monts Tataviam", new Vector3(2555.6f, 382.5f, 107.8f), new Vector3(2555.6f, 380.8f, 107.8f), new Vector3(2558.1f, 389.3f, 108.6f));
            LTDShops.Add(ltd8);
            
            LTDShopInfo ltd9 = new LTDShopInfo("Sandy Shores", new Vector3(1392.3f, 3604.4f, 33.1f), new Vector3(1392.3f, 3606.4f, 34.1f), Vector3.Zero);
            LTDShops.Add(ltd9);

            LTDShopInfo ltd10 = new LTDShopInfo("Grapeseed", new Vector3(1698.1f, 4924.8f, 41.2f), new Vector3(1696.9f, 4923.1f, 41.2f), new Vector3(1702.7f, 4933.1f, 42));
            LTDShops.Add(ltd10);
            
            LTDShopInfo ltd11 = new LTDShopInfo("Mirror Park", new Vector3(1163.4f, -323.9f, 68.6f), new Vector3(1165.2f, -323.6f, 68.4f), new Vector3(1154.1f, -326.6f, 69.2f));
            LTDShops.Add(ltd11);
            
            LTDShopInfo ltd12 = new LTDShopInfo("Vinewood", new Vector3(374.1f, 325.9f, 102.7f), new Vector3(372.2f, 326.5f, 102.7f), new Vector3(380.9f, 323.6f, 103.5f));
            LTDShops.Add(ltd12);
            
            LTDShopInfo ltd13 = new LTDShopInfo("Vespucci", new Vector3(-1223.5f, -907.1f, 11.7f), new Vector3(-1222.2f, -908.8f, 11.6f), Vector3.Zero);
            LTDShops.Add(ltd13);
            
            LTDShopInfo ltd14 = new LTDShopInfo("Murrieta Heights", new Vector3(1136.1f, -981.7f, 45.6f), new Vector3(1133.6f, -981.7f, 45.6f), Vector3.Zero);
            LTDShops.Add(ltd14);
            
            LTDShopInfo ltd15 = new LTDShopInfo("Harmony", new Vector3(547.4f, 2671.3f, 41.3f), new Vector3(549.2f, 2670.9f, 41.3f), new Vector3(540.2f, 2670.8f, 42.1f));
            LTDShops.Add(ltd15);
            
            LTDShopInfo ltd16 = new LTDShopInfo("Mont Chiliad", new Vector3(1729.0f, 6414.4f, 34.2f), new Vector3(1727.6f, 6415.3f, 34.2f), new Vector3(1735.3f, 6411, 35));
            LTDShops.Add(ltd16);
        }
        public async void CreatePeds()
        {
            foreach (var ltd in LTDShops)
            {
                Blip myBlip = World.CreateBlip(ltd.Checkout);
                myBlip.Sprite = BlipSprite.Store;
                myBlip.Name = "Magasin";
                myBlip.Color = BlipColor.Green;
                myBlip.IsShortRange = true;
                var pedHash = PedHash.Malibu01AMM;
                RequestModel((uint)pedHash);
                while (!HasModelLoaded((uint)pedHash))
                {
                    await BaseScript.Delay(100);
                }
                var ped = World.CreatePed(pedHash, ltd.PNJCoords);
                FreezeEntityPosition(ped.Result.Handle, true);
                SetEntityInvincible(ped.Result.Handle, true);
                SetBlockingOfNonTemporaryEvents(ped.Result.Handle, true);
                Client.PedId.Add(ped.Result.Handle);
            }
        }

        public void LTDMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerMenu.PlayerInst.Inventory);

            foreach (var ltd in LTDShops)
            {
                var distance = playerCoords.DistanceToSquared(ltd.Checkout);
                if (distance < 4)
                {
                    Format.SetMarker(ltd.Checkout, MarkerType.HorizontalCircleFat);
                }
                if (distance < 1)
                {
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                    if (IsControlJustPressed(0, 38))
                    {
                        var menu = new NativeMenu("LTD", $"LTD - {ltd.LTDName}")
                        {
                            UseMouse = false
                        };
                        Pool.Add(menu);
                        menu.Visible = true;

                        var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");

                        var myMoney = new NativeItem($"Mon argent: ~g~${dollarsItem}");
                        foreach (var item in LTDItems)
                        {
                            var itemDefault = items.FirstOrDefault(i => i.Item == item.Name);
                            var _ = new NativeItem($"{item.Name}", $"{item.Description}", $"~g~${item.Price}");
                            menu.Add(_);
                            _.Activated += async (sender, e) =>
                            {
                                var textInput = await Format.GetUserInput("Quantité", "1", 4);
                                var parsedInput = Int32.Parse(textInput);
                                var result = item.Price * parsedInput;
                                if (result <= PlayerMenu.PlayerInst.Money)
                                {
                                    PlayerMenu.PlayerInst.Money -= result;
                                    PlayerMenu.PlayerInst.Inventory = JsonConvert.SerializeObject(items);
                                    BaseScript.TriggerServerEvent("core:transaction", result, item.Name, parsedInput, "item");
                                    menu.Visible = false;
                                }
                                else
                                {
                                    Format.SendNotif("~r~La somme est trop élevée");
                                }
                                
                            };
                        }

                    }
                }
            }
        }

        public void ATMMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerMenu.PlayerInst.Inventory);

            foreach (var ltd in LTDShops)
            {
                var distance = playerCoords.DistanceToSquared(ltd.ATM);
                if (distance < 4)
                {
                    Format.SetMarker(ltd.ATM, MarkerType.HorizontalCircleFat);
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                    if (IsControlPressed(0, 38))
                    {
                        var menu = new NativeMenu("Banque", "Bienvenue")
                        {
                            UseMouse = false
                        };
                        Pool.Add(menu);
                        menu.Visible = true;

                        var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");

                        var myMoneyInBank = new NativeItem($"En banque: ~g~${PlayerMenu.PlayerInst.Money}", "Rien ne se crée, tout se transactionne");
                        var myMoney = new NativeItem($"En possession: ~g~${dollarsItem.Quantity}");

                        var actionBank = new NativeListItem<string>("Action", "", "~g~<b>Retirer</b>", "~g~<b>Déposer</b>");
                        actionBank.Activated += async (sender, e) =>
                        {
                            var textInput = await Format.GetUserInput("Quantité", "1", 20);
                            var parsedInput = Int32.Parse(textInput);

                            if (actionBank.SelectedItem == "~g~<b>Retirer</b>")
                            {
                                if (parsedInput <= PlayerMenu.PlayerInst.Money)
                                {
                                    PlayerMenu.PlayerInst.Money -= parsedInput;
                                    dollarsItem.Quantity += parsedInput;
                                }
                                else
                                {
                                    Format.SendNotif("~r~La somme est trop élevé");
                                }
                            }
                            else if (actionBank.SelectedItem == "~g~<b>Déposer</b>")
                            {
                                if (parsedInput <= dollarsItem.Quantity)
                                {
                                    PlayerMenu.PlayerInst.Money += parsedInput;
                                    dollarsItem.Quantity -= parsedInput;
                                }
                                else
                                {
                                    Format.SendNotif("~r~La somme est trop élevé");
                                }
                            }

                            PlayerMenu.PlayerInst.Inventory = JsonConvert.SerializeObject(items);
                            BaseScript.TriggerServerEvent("core:bankTransaction", actionBank.SelectedItem, parsedInput);
                            menu.Visible = false;
                        };

                        menu.Add(myMoneyInBank);
                        menu.Add(myMoney);
                        menu.Add(actionBank);
                    }
                }
            }
        }


        public void OnTick()
        {
            LTDMenu();
            ATMMenu();
        }
    }
}
