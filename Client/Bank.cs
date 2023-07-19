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
                if (distance < 4)
                {
                    Format.SetMarker(bank.Checkout, MarkerType.HorizontalCircleFat);
                }
                if (distance < 1)
                {
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
                        var beginRobbery = new NativeMenu("Braquage", "Démarrer le braquage");
                        
                        Pool.Add(beginRobbery);
                        menu.AddSubMenu(beginRobbery);
                        beginRobbery.UseMouse = false;
                        var playersList = new NativeMenu("Participants", "Sélectionner les participants");
                        Pool.Add(playersList);
                        beginRobbery.AddSubMenu(playersList);
                        playersList.UseMouse = false;
                        var playerTarget = World.GetClosest(playerCoords, World.GetAllPeds());
                        var listBefore = new List<int>();

                        var addPlayer = new NativeItem("Ajouter");
                        playersList.Add(addPlayer);

                        addPlayer.Activated += (sender, e) =>
                        {
                            if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                            {
                                listBefore.Add(playerTarget.NetworkId);
                                Format.SendNotif($"Joueur N°{playerTarget.NetworkId} ajouté au braquage");
                            }
                        };
                        
                        var submit = new NativeItem("Enregistrer");
                        playersList.Add(submit);

                        NativeItem playerItem = null;
                        submit.Activated += async (sender, e) =>
                        {
                            var json = JsonConvert.SerializeObject(listBefore);
                            BaseScript.TriggerServerEvent("core:getPlayersRobbery", json);
                            await BaseScript.Delay(5000);
                            Debug.WriteLine($"count {PlayersRobbery.Count}");
                            Debug.WriteLine($"json {json}");

                            foreach (var player in PlayersRobbery)
                            {
                                Debug.WriteLine($"Joueur: {player.Lastname}");
                                playerItem = new NativeItem($"{player.Firstname} {player.Lastname}");
                                playersList.Add(playerItem);
                            }
                        };

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
                                Format.PlayAnimation("anim@heists@fleeca_bank@drilling", "drill_straight_start", 10000);
                                await AddPropToPlayer("hei_prop_heist_drill", 28422, 0, 0, 0, 0, 0, 0, 10000);
                                PlaySoundFrontend(-1, "Drill_Pin_Break", "DLC_HEIST_FLEECA_SOUNDSET", true);
                                pc.Quantity -= 1;
                                drill.Quantity -= 1;

                                BaseScript.TriggerServerEvent("core:removeItem", pc.Item);
                                BaseScript.TriggerServerEvent("core:removeItem", drill.Item);
                                await BaseScript.Delay(5000);

                                BaseScript.TriggerServerEvent("giveMoney", 200000);
                            } else
                            {
                                Format.SendNotif("Vous n'avez pas les outils nécessaires");
                            }
                            
                        };
                    }
                }
            }

        }

        public async Task AddPropToPlayer(string prop1, int bone, float off1, float off2, float off3, float rot1, float rot2, float rot3, int duration)
        {
            int player = PlayerPedId();
            Vector3 playerCoords = GetEntityCoords(player, true);

            RequestModel((uint)GetHashKey(prop1));

            int prop = CreateObject(GetHashKey(prop1), playerCoords.X, playerCoords.Y, playerCoords.Z + 0.2f, true, true, true);
            AttachEntityToEntity(prop, player, GetPedBoneIndex(player, bone), off1, off2, off3, rot1, rot2, rot3, true, true, false, true, 1, true);

            await BaseScript.Delay(duration);

            DeleteEntity(ref prop);

            SetModelAsNoLongerNeeded((uint)GetHashKey(prop1));
        }


        public void OnTick()
        {
            BankMenu();
            BankRobbery();
        }
    }
}
