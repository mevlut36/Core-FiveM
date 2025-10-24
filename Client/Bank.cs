using CitizenFX.Core;
using CitizenFX.Core.Native;
using Core.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class Bank : BaseScript
    {
        public Format Format;
        public PlayerMenu PlayerMenu;
        private NUIManager _nuiManager;
        private bool isUIOpen = false;
        private float lastEPress = 0f;

        List<BankInfo> Banks = new List<BankInfo>();
        List<PlayerInstance> PlayersRobbery = new List<PlayerInstance>();

        public Bank(ClientMain caller)
        {
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;
            _nuiManager = NUIManager.Instance;

            // Initialisation des banques
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

            // Création des blips
            foreach (var bank in Banks)
            {
                Blip myBlip = World.CreateBlip(bank.Checkout);
                myBlip.Sprite = BlipSprite.DollarSign;
                myBlip.Color = BlipColor.Green;
                myBlip.Name = "Banque";
                myBlip.IsShortRange = true;
            }

            // Enregistrement des callbacks via NUIManager
            RegisterCallbacks();
            Debug.WriteLine("[BANK] Callbacks NUI enregistrés via NUIManager !");
        }

        private void RegisterCallbacks()
        {
            Debug.WriteLine("[BANK] ===== DÉBUT ENREGISTREMENT CALLBACKS =====");

            // Callback pour les transactions bancaires (dépôt/retrait)
            _nuiManager.RegisterCallback("bank:transaction", (data) =>
            {
                Debug.WriteLine("[BANK] Callback 'bank:transaction' reçu");
                var dict = data as IDictionary<string, object>;
                if (dict != null)
                {
                    OnBankTransaction(dict);
                }
                else
                {
                    Debug.WriteLine("[BANK] ERREUR : Impossible de convertir data en IDictionary");
                }
            });
            Debug.WriteLine("[BANK] ✅ 'bank:transaction' enregistré");

            // Callback pour démarrer un braquage
            _nuiManager.RegisterCallback("bank:startRobbery", async (data) =>
            {
                Debug.WriteLine("[BANK] Callback 'bank:startRobbery' reçu");
                await OnStartRobbery();
            });
            Debug.WriteLine("[BANK] ✅ 'bank:startRobbery' enregistré");

            // Callback pour rafraîchir les données
            _nuiManager.RegisterCallback("bank:refresh", (data) =>
            {
                Debug.WriteLine("[BANK] Callback 'bank:refresh' reçu");
                RefreshBankData();
            });
            Debug.WriteLine("[BANK] ✅ 'bank:refresh' enregistré");

            EventHandlers["nui:closed"] += new Action(() =>
            {
                Debug.WriteLine("[BANK] Event nui:closed reçu");
                isUIOpen = false;
            });

            Debug.WriteLine("[BANK] ===== FIN ENREGISTREMENT CALLBACKS =====");
        }

        public void BankMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var items = PlayerMenu.PlayerInst.Inventory;

            foreach (var bank in Banks)
            {
                var distance = playerCoords.DistanceToSquared(bank.Checkout);

                if (distance < 25)
                {
                    Format.SetMarker(bank.Checkout, MarkerType.HorizontalCircleFat);

                    if (distance < 4)
                    {
                        Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                        if (IsControlJustPressed(0, 38))
                        {
                            float currentTime = GetGameTimer() / 1000f;
                            if (currentTime - lastEPress < 0.5f) return;
                            lastEPress = currentTime;

                            // CORRECTION: Force cleanup si une UI est déjà ouverte
                            if (isUIOpen || _nuiManager.IsOpen)
                            {
                                Debug.WriteLine($"[BANK] UI déjà ouverte ({_nuiManager.CurrentNUI}), nettoyage forcé");
                                _nuiManager.CloseNUI();
                                isUIOpen = false;
                                // Attendre un frame avant de réouvrir
                                return;
                            }

                            Debug.WriteLine($"[BANK] Ouverture de la banque : {bank.BankName}");
                            OpenBankUI(bank.BankName);
                        }
                    }
                }
            }
        }

        private void OpenBankUI(string bankName)
        {
            Debug.WriteLine($"[BANK] OpenBankUI pour : {bankName}");

            isUIOpen = true;

            var dollarsItem = PlayerMenu.PlayerInst.Inventory.FirstOrDefault(item => item.Item == "Dollars");

            var bankData = new
            {
                bankName = bankName,
                moneyInBank = PlayerMenu.PlayerInst.Money,
                moneyInPocket = dollarsItem?.Quantity ?? 0,
                playerName = GetPlayerName(PlayerId())
            };

            Debug.WriteLine($"[BANK] Données envoyées : Bank=${bankData.moneyInBank}, Pocket=${bankData.moneyInPocket}");

            _nuiManager.OpenNUI("bank", bankData);
        }

        private void CloseBankUI()
        {
            Debug.WriteLine("[BANK] CloseBankUI appelé");
            isUIOpen = false;

            // NUIManager.CloseNUI() gère déjà SetNuiFocus
            _nuiManager.CloseNUI();

            Debug.WriteLine("[BANK] Interface fermée");
        }

        private void RefreshBankData()
        {
            var dollarsItem = PlayerMenu.PlayerInst.Inventory.FirstOrDefault(item => item.Item == "Dollars");

            _nuiManager.SendNUIMessage(new
            {
                action = "updateData",
                data = new
                {
                    moneyInBank = PlayerMenu.PlayerInst.Money,
                    moneyInPocket = dollarsItem?.Quantity ?? 0
                }
            });
        }

        private void OnBankTransaction(IDictionary<string, object> data)
        {
            Debug.WriteLine("=== [BANK] OnBankTransaction APPELÉ ===");
            Debug.WriteLine($"[BANK] Data brut : {JsonConvert.SerializeObject(data)}");

            try
            {
                if (!data.ContainsKey("action") || !data.ContainsKey("amount"))
                {
                    Debug.WriteLine("[BANK] ERREUR : Clés 'action' ou 'amount' manquantes !");
                    ShowNotification("Erreur : données invalides", "error");
                    return;
                }

                string action = data["action"].ToString();
                int amount = Convert.ToInt32(data["amount"]);

                Debug.WriteLine($"[BANK] Action : {action}, Montant : ${amount}");

                var items = PlayerMenu.PlayerInst.Inventory;
                var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");

                Debug.WriteLine($"[BANK] Inventaire actuel - Banque: ${PlayerMenu.PlayerInst.Money}, Dollars: ${dollarsItem?.Quantity ?? 0}");

                bool success = false;
                string message = "";

                if (action == "withdraw")
                {
                    if (amount <= PlayerMenu.PlayerInst.Money)
                    {
                        PlayerMenu.PlayerInst.Money -= amount;
                        if (dollarsItem != null)
                        {
                            dollarsItem.Quantity += amount;
                        }
                        else
                        {
                            items.Add(new InventoryItem { Item = "Dollars", Quantity = amount, Type = "item" });
                        }
                        success = true;
                        message = $"Retrait de ${amount} effectué";
                    }
                    else
                    {
                        message = "Solde insuffisant";
                    }
                }
                else if (action == "deposit")
                {
                    if (dollarsItem != null && amount <= dollarsItem.Quantity)
                    {
                        PlayerMenu.PlayerInst.Money += amount;
                        dollarsItem.Quantity -= amount;
                        success = true;
                        message = $"Dépôt de ${amount} effectué";
                    }
                    else
                    {
                        message = "Vous n'avez pas assez d'argent sur vous";
                    }
                }

                if (success)
                {
                    PlayerMenu.PlayerInst.Inventory = items;
                    BaseScript.TriggerServerEvent("core:bankTransaction", action, amount);
                    ShowNotification(message, "success");
                    RefreshBankData();
                    Debug.WriteLine($"[BANK] Transaction réussie - Nouveau solde Bank: ${PlayerMenu.PlayerInst.Money}");
                }
                else
                {
                    ShowNotification(message, "error");
                    Debug.WriteLine($"[BANK] Transaction échouée : {message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BANK] ERREUR : {ex.Message}\n{ex.StackTrace}");
                ShowNotification("Erreur lors de la transaction", "error");
            }
        }

        private async Task OnStartRobbery()
        {
            Debug.WriteLine("[BANK] Démarrage du braquage !");

            try
            {
                var items = PlayerMenu.PlayerInst.Inventory;
                var pc = items.FirstOrDefault(item => item.Item == "Ordinateur");
                var drill = items.FirstOrDefault(item => item.Item == "Perceuse");

                if (pc != null && pc.Quantity > 0 && drill != null && drill.Quantity > 0)
                {
                    // Fermer l'interface
                    isUIOpen = false;
                    _nuiManager.CloseNUI();

                    Format.ShowAdvancedNotification("ShurikenRP", "Bank Sys.", "~r~Braquage en cours...");
                    Format.PlayAnimation("missbigscore2aswitch", "switch_mic_car_fra_laptop_hacker", 8, (AnimationFlags)50);
                    await Format.AddPropToPlayer("hei_prop_hst_laptop", 28422, 0, 0, 0, 0, 0, 0, 5000);
                    Format.StopAnimation("missbigscore2aswitch", "switch_mic_car_fra_laptop_hacker");
                    Format.PlayAnimation("anim@heists@fleeca_bank@drilling", "drill_straight_start", 8, (AnimationFlags)50);
                    PlaySoundFrontend(-1, "Drill_Pin_Break", "DLC_HEIST_FLEECA_SOUNDSET", true);
                    await Format.AddPropToPlayer("hei_prop_heist_drill", 28422, 0, 0, 0, 0, 0, 0, 5000);
                    Format.StopAnimation("anim@heists@fleeca_bank@drilling", "drill_straight_start");

                    var playerCoords = GetEntityCoords(PlayerPedId(), false);
                    uint streetNameHash = 0;
                    uint crossingRoadHash = 0;
                    GetStreetNameAtCoord(playerCoords.X, playerCoords.Y, playerCoords.Z, ref streetNameHash, ref crossingRoadHash);
                    string streetName = GetStreetNameFromHashKey(streetNameHash);
                    BaseScript.TriggerServerEvent("core:bankRobbery", streetName);

                    Debug.WriteLine("[BANK] Braquage réussi !");
                }
                else
                {
                    Format.ShowAdvancedNotification("ShurikenRP", "Bank Sys.", "Vous n'avez pas les outils nécessaires");
                    ShowNotification("Outils manquants : Ordinateur et Perceuse requis", "error");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BANK] ERREUR dans OnStartRobbery : {ex.Message}");
                ShowNotification("Erreur lors du braquage", "error");
            }
        }

        private void ShowNotification(string message, string type = "info")
        {
            _nuiManager.SendNUIMessage(new
            {
                action = "notification",
                data = new
                {
                    message = message,
                    type = type
                }
            });
        }

        public void BankRobbery()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);

            foreach (var bank in Banks)
            {
                var distance = GetDistanceBetweenCoords(bank.Robbery.X, bank.Robbery.Y, bank.Robbery.Z,
                    playerCoords.X, playerCoords.Y, playerCoords.Z, false);

                if (distance < 4)
                {
                    Format.SetMarker(bank.Robbery, MarkerType.HorizontalCircleFat);
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour commencer le braquage");

                    if (IsControlJustPressed(0, 38))
                    {
                        // CORRECTION: Même logique que BankMenu
                        if (isUIOpen || _nuiManager.IsOpen)
                        {
                            Debug.WriteLine($"[BANK] UI déjà ouverte pour braquage, nettoyage forcé");
                            _nuiManager.CloseNUI();
                            isUIOpen = false;
                            return;
                        }

                        Debug.WriteLine($"[BANK] Ouverture interface braquage");
                        OpenRobberyUI(bank);
                    }
                }
            }
        }

        private void OpenRobberyUI(BankInfo bank)
        {
            Debug.WriteLine($"[BANK] OpenRobberyUI pour : {bank.BankName}");

            isUIOpen = true;

            var items = PlayerMenu.PlayerInst.Inventory;
            var pc = items.FirstOrDefault(item => item.Item == "Ordinateur");
            var drill = items.FirstOrDefault(item => item.Item == "Perceuse");

            var robberyData = new
            {
                bankName = bank.BankName,
                hasPC = pc != null && pc.Quantity > 0,
                hasDrill = drill != null && drill.Quantity > 0
            };

            Debug.WriteLine($"[BANK] Données braquage : PC={robberyData.hasPC}, Drill={robberyData.hasDrill}");

            _nuiManager.OpenNUI("bankRobbery", robberyData);
        }

        public void GetPlayers(string json)
        {
            var players = JsonConvert.DeserializeObject<List<PlayerInstance>>(json);
            PlayersRobbery.AddRange(players);
        }

        public void OnTick()
        {
            BankMenu();
            BankRobbery();
        }
    }
}