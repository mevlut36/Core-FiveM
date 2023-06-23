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
    public class Bank
    {
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public PlayerMenu PlayerMenu;

        List<BankInfo> Banks = new List<BankInfo>();

        public Bank(ClientMain caller)
        {
            Pool = caller.Pool;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            BankInfo bank1 = new BankInfo("Pacific", new Vector3(252.4f, 220.38f, 106.28f), new Vector3(253.5f, 228.2f, 101.68f));
            Banks.Add(bank1);

            BankInfo bank2 = new BankInfo("Vespucci", new Vector3(150, -1039.8f, 28.5f), new Vector3(146.8f, -1045.9f, 29.35f));
            Banks.Add(bank2);
            
            BankInfo bank3 = new BankInfo("Alta", new Vector3(314.5f, -277.9f, 54.1f), new Vector3(311, -284.2f, 54.1f));
            Banks.Add(bank3);
            
            BankInfo bank4 = new BankInfo("Burton", new Vector3(-350.6f, -48.9f, 49), new Vector3(-353.9f, -54.9f, 49));
            Banks.Add(bank4);
            
            BankInfo bank5 = new BankInfo("Rockford Hills", new Vector3(-1212.8f, -329.8f, 37.7f), new Vector3(-1211.1f, -336.4f, 37.7f));
            Banks.Add(bank5);
            
            BankInfo bank6 = new BankInfo("Bahnham Canyon", new Vector3(-2964, 482.8f, 15.7f), new Vector3(-2956.8f, 481.5f, 15.7f));
            Banks.Add(bank6);
            
            BankInfo bank7 = new BankInfo("Blaine County", new Vector3(-112.3f, 6468.6f, 31.6f), new Vector3(-105.2f, 6470.9f, 31.6f));
            Banks.Add(bank7);
            foreach (var bank in Banks)
            {
                Blip myBlip = World.CreateBlip(bank.Checkout);
                myBlip.Sprite = BlipSprite.DollarSign;
                myBlip.Color = BlipColor.Green;
                myBlip.Name = "Banque";
                myBlip.IsShortRange = true;
            }
        }

        public void BankMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerMenu.PlayerInst.Inventory);

            foreach (var bank in Banks)
            {
                var distance = GetDistanceBetweenCoords(bank.Checkout.X, bank.Checkout.Y, bank.Checkout.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);

                if (distance < 4)
                {
                    Format.SetMarker(bank.Checkout, MarkerType.HorizontalCircleFat);
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                    if (IsControlPressed(0, 38))
                    {
                        var menu = new NativeMenu("Banque", "Bienvenue")
                        {
                            TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                            UseMouse = false
                        };
                        Pool.Add(menu);
                        menu.Visible = true;

                        var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");
                        var dollarsAmount = dollarsItem?.Quantity ?? 0;

                        var myMoneyInBank = new NativeItem($"En banque: ~g~${PlayerMenu.PlayerInst.Money}", "Rien ne se crée, tout se transactionne");
                        var myMoney = new NativeItem($"En possession: ~g~${dollarsItem?.Quantity ?? 0}");

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
                                    dollarsAmount += parsedInput;
                                } else
                                {
                                    Format.SendNotif("~r~La somme est trop élevé");
                                }
                            }
                            else if (actionBank.SelectedItem == "~g~<b>Déposer</b>")
                            {
                                if (parsedInput <= dollarsAmount)
                                {
                                    PlayerMenu.PlayerInst.Money += parsedInput;
                                    dollarsAmount -= parsedInput;
                                } else
                                {
                                    Format.SendNotif("~r~La somme est trop élevé");
                                }
                            }

                            PlayerMenu.PlayerInst.Inventory = JsonConvert.SerializeObject(items);
                            BaseScript.TriggerServerEvent("core:bankTransaction", actionBank.SelectedItem, parsedInput);
                            BaseScript.TriggerServerEvent("core:requestPlayerData");
                            menu.Visible = false;
                        };

                        menu.Add(myMoneyInBank);
                        menu.Add(myMoney);
                        menu.Add(actionBank);
                    }
                }
            }
        }

        public void BankRobbery()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            foreach (var bank in Banks)
            {
                var distance = GetDistanceBetweenCoords(bank.Robbery.X, bank.Robbery.Y, bank.Robbery.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);
                if (distance < 4)
                {
                    Format.SetMarker(bank.Robbery, MarkerType.HorizontalCircleFat);
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour commencer le braquage");
                    if (IsControlPressed(0, 38))
                    {
                        var menu = new NativeMenu("Banque", "Braquage")
                        {
                            TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                            UseMouse = false
                        };
                        Pool.Add(menu);
                        menu.Visible = true;
                        var robberyItem = new NativeItem("Démarrer le ~r~braquage", "<b>Outils nécessaires:</b> \n- Ordinateur\n- Perceuse");
                        menu.Add(robberyItem);
                        robberyItem.Activated += async (sender, e) =>
                        {
                            Format.SendNotif("Début du ~r~braquage...");
                            await BaseScript.Delay(5000);
                            Format.SendNotif("En cours de dev part chakal");
                        };
                    }
                }
            }

        }


        public void OnTick()
        {
            BankMenu();
            BankRobbery();
        }
    }
}
