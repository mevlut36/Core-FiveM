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
using Core.Shared;

namespace Core.Client
{
    public class Bank
    {
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public PlayerMenu PlayerMenu;

        List<BankInfo> Banks = new List<BankInfo>();

        List<PlayerInstance> PlayersRobbery = new List<PlayerInstance>();
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
                var distance = playerCoords.DistanceToSquared(bank.Checkout);
                if (distance < 5)
                {
                    Format.SetMarker(bank.Checkout, MarkerType.HorizontalCircleFat);
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

        public void GetPlayers(string json)
        {
            var players = JsonConvert.DeserializeObject<List<PlayerInstance>>(json);
            Debug.WriteLine($"players: {json}");
            PlayersRobbery.AddRange(players);
        }
        public void BankRobbery()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var players = new List<PlayerInstance>();
            players.Clear();
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
                            UseMouse = false
                        };
                        Pool.Add(menu);
                        menu.Visible = true;
                        menu.UseMouse = false;
                        var robbery = new NativeItem("Démarrer le ~r~braquage", "<b>Outils nécessaires:</b> \n- Ordinateur\n- Perceuse");
                        menu.Add(robbery);
                        robbery.Activated += async (sender, e) =>
                        {
                            var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerMenu.PlayerInst.Inventory);

                            var pc = items.FirstOrDefault(item => item.Item == "Ordinateur");
                            var drill = items.FirstOrDefault(item => item.Item == "Perceuse");
                            // Format.PlayAnimation("anim@heists@fleeca_bank@drilling", "intro", 10000);

                            if (pc.Quantity > 0 && drill.Quantity > 0)
                            {
                                Format.PlayAnimation("anim@heists@fleeca_bank@drilling", "drill_straight_start", 8, (AnimationFlags)49);
                                await Format.AddPropToPlayer("hei_prop_heist_drill", 28422, 0, 0, 0, 0, 0, 0, 10000);
                                PlaySoundFrontend(-1, "Drill_Pin_Break", "DLC_HEIST_FLEECA_SOUNDSET", true);
                                Format.SendNotif("~r~Braquage en cours...");
                                uint streetNameHash = 0;
                                uint crossingRoadHash = 0;
                                GetStreetNameAtCoord(playerCoords.X, playerCoords.Y, playerCoords.Z, ref streetNameHash, ref crossingRoadHash);
                                string streetName = GetStreetNameFromHashKey(streetNameHash);
                                BaseScript.TriggerServerEvent("core:bankRobbery", streetName);
                            } else
                            {
                                Format.SendNotif("Vous n'avez pas les outils nécessaires");
                            }
                            menu.Visible = false;
                            Pool.Remove(menu);
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
