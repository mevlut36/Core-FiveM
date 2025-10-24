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

        private ShoppingCart currentCart = new ShoppingCart();

        List<LTDShopInfo> LTDShops = new List<LTDShopInfo>();
        List<LTDItems> LTDItems = new List<LTDItems>();

        private readonly System.Drawing.Color AccentColor = System.Drawing.Color.FromArgb(255, 76, 175, 80);
        private readonly System.Drawing.Color SuccessColor = System.Drawing.Color.FromArgb(255, 46, 204, 113);
        private readonly System.Drawing.Color ErrorColor = System.Drawing.Color.FromArgb(255, 231, 76, 60);
        private readonly System.Drawing.Color InfoColor = System.Drawing.Color.FromArgb(255, 52, 152, 219);
        private readonly System.Drawing.Color WarningColor = System.Drawing.Color.FromArgb(255, 241, 196, 15);

        private LTDShopInfo currentShop = null;
        private LTDShopInfo currentATM = null;
        private const float MAX_MENU_DISTANCE = 15f;

        public LTDShop(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            InitializeItems();
            InitializeShops();
            CreateShopBlips();
        }

        private void InitializeItems()
        {
            LTDItems item1 = new LTDItems("Pain", "Nourriture de base pour récupérer de la santé", 25, "🍞", "Nourriture");
            LTDItems.Add(item1);

            LTDItems item2 = new LTDItems("Eau", "Boisson rafraîchissante pour s'hydrater", 25, "💧", "Boisson");
            LTDItems.Add(item2);

            LTDItems item3 = new LTDItems("Phone", "Smartphone dernier cri", 1000, "📱", "Électronique");
            LTDItems.Add(item3);

            LTDItems item4 = new LTDItems("Sandwich", "Repas complet et nutritif", 50, "🥪", "Nourriture");
            LTDItems.Add(item4);

            LTDItems item5 = new LTDItems("Burger", "Hamburger juteux et savoureux", 75, "🍔", "Nourriture");
            LTDItems.Add(item5);

            LTDItems item6 = new LTDItems("Coca Cola", "Boisson gazeuse rafraîchissante", 30, "🥤", "Boisson");
            LTDItems.Add(item6);

            LTDItems item7 = new LTDItems("Café", "Boost d'énergie instantané", 20, "☕", "Boisson");
            LTDItems.Add(item7);

            LTDItems item8 = new LTDItems("Cigarettes", "Paquet de 20 cigarettes", 15, "🚬", "Tabac");
            LTDItems.Add(item8);

            LTDItems item9 = new LTDItems("Briquet", "Pour allumer vos cigarettes", 5, "🔥", "Accessoire");
            LTDItems.Add(item9);
        }

        private void InitializeShops()
        {
            LTDShopInfo ltd0 = new LTDShopInfo("Cube Place", "Supermarché 24/7 du centre", new Vector3(190.1f, -889.8f, 29.8f), new Vector3(188.4f, -889.3f, 29.8f), new Vector3(187.6f, -899.5f, 30.6f));
            LTDShops.Add(ltd0);

            LTDShopInfo ltd1 = new LTDShopInfo("Strawberry", "Épicerie de quartier", new Vector3(26.2f, -1346.9f, 28.5f), new Vector3(24.3f, -1346.6f, 28.5f), new Vector3(33.1f, -1347.9f, 29.3f));
            LTDShops.Add(ltd1);

            LTDShopInfo ltd2 = new LTDShopInfo("Davis", "Convenience store", new Vector3(-48, -1756.8f, 28.5f), new Vector3(-46.4f, -1758.1f, 28.6f), new Vector3(-56.7f, -1752.3f, 29.3f));
            LTDShops.Add(ltd2);

            LTDShopInfo ltd3 = new LTDShopInfo("Chumash", "Boutique de la plage", new Vector3(-3243.2f, 1001.8f, 11.9f), new Vector3(-3243.2f, 999.9f, 11.9f), new Vector3(-3240.9f, 1008.7f, 12.8f));
            LTDShops.Add(ltd3);

            LTDShopInfo ltd4 = new LTDShopInfo("Banham Canyon", "Épicerie de montagne", new Vector3(-3040.9f, 585.5f, 7.1f), new Vector3(-3040.9f, 583.9f, 7.1f), new Vector3(-3040.9f, 592.9f, 7.9f));
            LTDShops.Add(ltd4);

            LTDShopInfo ltd5 = new LTDShopInfo("Banham Canyon 2", "Mini-mart local", new Vector3(-2968.3f, 390.3f, 14.2f), new Vector3(-2966.1f, 390.3f, 14.2f), Vector3.Zero);
            LTDShops.Add(ltd5);

            LTDShopInfo ltd6 = new LTDShopInfo("Richman Glen", "Superette de luxe", new Vector3(-1820.7f, 792.6f, 137.3f), new Vector3(-1819.2f, 792.6f, 137.3f), new Vector3(-1827.1f, 785.4f, 138.2f));
            LTDShops.Add(ltd6);

            LTDShopInfo ltd7 = new LTDShopInfo("Morningwood", "Dépanneur rapide", new Vector3(-1487.2f, -379.4f, 39.3f), new Vector3(-1485.6f, -378.1f, 39.2f), Vector3.Zero);
            LTDShops.Add(ltd7);

            LTDShopInfo ltd8 = new LTDShopInfo("Monts Tataviam", "Épicerie des collines", new Vector3(2555.6f, 382.5f, 107.8f), new Vector3(2555.6f, 380.8f, 107.8f), new Vector3(2558.1f, 389.3f, 108.6f));
            LTDShops.Add(ltd8);

            LTDShopInfo ltd9 = new LTDShopInfo("Sandy Shores", "Magasin du désert", new Vector3(1392.3f, 3604.4f, 33.1f), new Vector3(1392.3f, 3606.4f, 34.1f), Vector3.Zero);
            LTDShops.Add(ltd9);

            LTDShopInfo ltd10 = new LTDShopInfo("Grapeseed", "Épicerie rurale", new Vector3(1698.1f, 4924.8f, 41.2f), new Vector3(1696.9f, 4923.1f, 41.2f), new Vector3(1702.7f, 4933.1f, 42));
            LTDShops.Add(ltd10);

            LTDShopInfo ltd11 = new LTDShopInfo("Mirror Park", "Supermarché moderne", new Vector3(1163.4f, -323.9f, 68.6f), new Vector3(1165.2f, -323.6f, 68.4f), new Vector3(1154.1f, -326.6f, 69.2f));
            LTDShops.Add(ltd11);

            LTDShopInfo ltd12 = new LTDShopInfo("Vinewood", "Boutique chic", new Vector3(374.1f, 325.9f, 102.7f), new Vector3(372.2f, 326.5f, 102.7f), new Vector3(380.9f, 323.6f, 103.5f));
            LTDShops.Add(ltd12);

            LTDShopInfo ltd13 = new LTDShopInfo("Vespucci", "Mini-market", new Vector3(-1223.5f, -907.1f, 11.7f), new Vector3(-1222.2f, -908.8f, 11.6f), Vector3.Zero);
            LTDShops.Add(ltd13);

            LTDShopInfo ltd14 = new LTDShopInfo("Murrieta Heights", "Superette locale", new Vector3(1136.1f, -981.7f, 45.6f), new Vector3(1133.6f, -981.7f, 45.6f), Vector3.Zero);
            LTDShops.Add(ltd14);

            LTDShopInfo ltd15 = new LTDShopInfo("Harmony", "Épicerie de campagne", new Vector3(547.4f, 2671.3f, 41.3f), new Vector3(549.2f, 2670.9f, 41.3f), new Vector3(540.2f, 2670.8f, 42.1f));
            LTDShops.Add(ltd15);

            LTDShopInfo ltd16 = new LTDShopInfo("Mont Chiliad", "Station de montagne", new Vector3(1729.0f, 6414.4f, 34.2f), new Vector3(1727.6f, 6415.3f, 34.2f), new Vector3(1735.3f, 6411, 35));
            LTDShops.Add(ltd16);
        }

        private void CreateShopBlips()
        {
            foreach (var ltd in LTDShops)
            {
                Blip myBlip = World.CreateBlip(ltd.Checkout);
                myBlip.Sprite = BlipSprite.Store;
                myBlip.Name = $"🏪 {ltd.LTDName}";
                myBlip.Color = BlipColor.Green;
                myBlip.IsShortRange = true;
                myBlip.Scale = 0.7f;
            }
        }

        public async void CreatePeds()
        {
            foreach (var ltd in LTDShops)
            {
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
            var items = PlayerMenu.PlayerInst.Inventory;

            foreach (var ltd in LTDShops)
            {
                var distance = playerCoords.DistanceToSquared(ltd.Checkout);

                if (distance < 100)
                {
                    DrawAdvancedMarker(ltd.Checkout);
                }

                if (distance < 5)
                {
                    DrawInteractionPrompt($"🏪 {ltd.LTDName}");

                    if (IsControlJustPressed(0, 38))
                    {
                        currentShop = ltd;
                        OpenLTDMenu(ltd);
                    }
                }
            }
        }

        private void OpenLTDMenu(LTDShopInfo ltd)
        {
            var items = PlayerMenu.PlayerInst.Inventory;
            var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");
            int playerMoney = dollarsItem?.Quantity ?? 0;

            var menu = new NativeMenu(
                $"{ltd.LTDName}",
                $"~g~p~Panier: {currentCart.GetTotalItems()} articles (~g~${currentCart.GetTotalPrice()}~w~)"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            Pool.Add(menu);
            menu.Visible = true;

            var cartMenu = CreateCartMenu();
            menu.AddSubMenu(cartMenu);
            Pool.Add(cartMenu);

            menu.Add(CreateSeparator());

            var groupedItems = LTDItems.GroupBy(i => i.Category);

            foreach (var group in groupedItems.OrderBy(g => g.Key))
            {
                var categoryIcon = GetCategoryIcon(group.Key);
                var categoryMenu = new NativeMenu(
                    $"{categoryIcon} {group.Key}",
                    $"Articles de la catégorie {group.Key}"
                )
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                Pool.Add(categoryMenu);
                menu.AddSubMenu(categoryMenu);

                foreach (var item in group.OrderBy(i => i.Price))
                {
                    var itemInInventory = items.FirstOrDefault(i => i.Item == item.Name);
                    int ownedQuantity = itemInInventory?.Quantity ?? 0;
                    int cartQuantity = currentCart.GetItemQuantity(item.Name);

                    var menuItem = new NativeItem(
                        $"{item.Icon} {item.Name}",
                        $"~b~---------------------\n" +
                        $"{item.Description}\n" +
                        $"~b~Prix unitaire:~w~ ~g~${item.Price}\n" +
                        $"~b~En possession:~w~ {ownedQuantity}\n" +
                        (cartQuantity > 0 ? $"~b~Dans le panier:~w~ ~p~{cartQuantity}\n" : "") +
                        $"~b~---------------------\n" +
                        $"~y~➤ Cliquez pour ajouter au panier",
                        $"~g~${item.Price}"
                    );

                    categoryMenu.Add(menuItem);

                    menuItem.Activated += async (sender, e) =>
                    {
                        await AddItemToCart(item);
                        menu.Visible = false;
                        categoryMenu.Visible = false;
                        await BaseScript.Delay(100);
                        OpenLTDMenu(ltd);
                    };
                }
            }

            menu.Closing += (sender, e) =>
            {
                currentShop = null;
            };
        }

        private NativeMenu CreateCartMenu()
        {
            var cartMenu = new NativeMenu(
                "🛒 Panier d'achat",
                $"~p~{currentCart.GetTotalItems()} articles - Total: ~g~${currentCart.GetTotalPrice()}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };

            if (currentCart.Items.Count == 0)
            {
                var emptyItem = new NativeItem("🛒 Panier vide", "~y~Ajoutez des articles pour commencer vos achats")
                {
                    Enabled = false
                };
                cartMenu.Add(emptyItem);
            }
            else
            {
                foreach (var cartItem in currentCart.Items)
                {
                    var ltdItem = LTDItems.FirstOrDefault(i => i.Name == cartItem.Key);
                    if (ltdItem != null)
                    {
                        var item = new NativeListItem<string>(
                            $"{ltdItem.Icon} {cartItem.Key}",
                            $"~b~Prix unitaire: ~g~${ltdItem.Price}\n~b~Quantité: ~w~{cartItem.Value}\n~b~Sous-total: ~g~${ltdItem.Price * cartItem.Value}",
                            "Modifier quantité", "Retirer du panier"
                        );
                        cartMenu.Add(item);

                        item.Activated += async (sender, e) =>
                        {
                            if (item.SelectedItem == "Modifier quantité")
                            {
                                var newQuantity = await Format.GetUserInput($"Nouvelle quantité pour {cartItem.Key}", cartItem.Value.ToString(), 4);
                                if (int.TryParse(newQuantity, out int quantity) && quantity > 0)
                                {
                                    currentCart.UpdateItemQuantity(cartItem.Key, quantity);
                                    ShowNotification($"~g~Quantité mise à jour", SuccessColor);
                                    cartMenu.Visible = false;
                                    await BaseScript.Delay(100);
                                    OpenLTDMenu(currentShop);
                                }
                            }
                            else if (item.SelectedItem == "Retirer du panier")
                            {
                                currentCart.RemoveItem(cartItem.Key);
                                ShowNotification($"~r~{cartItem.Key} retiré du panier", ErrorColor);
                                cartMenu.Visible = false;
                                await BaseScript.Delay(100);
                                OpenLTDMenu(currentShop);
                            }
                        };
                    }
                }

                cartMenu.Add(CreateSeparator());

                var totalItem = new NativeItem(
                    "💰 Total à payer",
                    $"~b~Articles: ~w~{currentCart.GetTotalItems()}\n~b~Total: ~g~${currentCart.GetTotalPrice()}"
                )
                {
                    Enabled = false
                };
                cartMenu.Add(totalItem);

                cartMenu.Add(CreateSeparator());

                var paymentMenu = new NativeMenu(
                    "💳 Modes de paiement",
                    "~y~Choisissez votre mode de paiement"
                )
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                Pool.Add(paymentMenu);
                cartMenu.AddSubMenu(paymentMenu);

                var items = PlayerMenu.PlayerInst.Inventory;
                var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");
                int cashAmount = dollarsItem?.Quantity ?? 0;
                int bankAmount = PlayerMenu.PlayerInst.Money;

                var cashPayment = new NativeItem(
                    "💵 Payer en liquide",
                    $"~b~Argent liquide disponible: ~g~${cashAmount}\n" +
                    (cashAmount >= currentCart.GetTotalPrice() ? "~g~Fonds suffisants" : "~r~Fonds insuffisants")
                );
                paymentMenu.Add(cashPayment);

                var bankPayment = new NativeItem(
                    "💳 Payer par carte",
                    $"~b~Solde bancaire: ~g~${bankAmount}\n" +
                    (bankAmount >= currentCart.GetTotalPrice() ? "~g~Fonds suffisants" : "~r~Fonds insuffisants")
                );
                paymentMenu.Add(bankPayment);

                cashPayment.Activated += async (sender, e) =>
                {
                    await ProcessPayment(PaymentMethod.Cash);
                };

                bankPayment.Activated += async (sender, e) =>
                {
                    await ProcessPayment(PaymentMethod.Bank);
                };

                cartMenu.Add(CreateSeparator());
                var clearCart = new NativeItem("🗑️ Vider le panier", "~r~Supprimer tous les articles du panier");
                cartMenu.Add(clearCart);

                clearCart.Activated += (sender, e) =>
                {
                    currentCart.Clear();
                    ShowNotification("~r~Panier vidé", ErrorColor);
                    cartMenu.Visible = false;
                    OpenLTDMenu(currentShop);
                };
            }

            return cartMenu;
        }

        private async Task AddItemToCart(LTDItems item)
        {
            var quantity = await Format.GetUserInput($"Quantité de {item.Name}", "1", 4);

            if (int.TryParse(quantity, out int parsedQuantity) && parsedQuantity > 0)
            {
                currentCart.AddItem(item.Name, parsedQuantity, item.Price);
                ShowNotification($"~g~{item.Icon} {item.Name} x{parsedQuantity} ajouté au panier", SuccessColor);
                PlaySoundFrontend(-1, "CLICK_BACK", "WEB_NAVIGATION_SOUNDS_PHONE", false);
            }
            else
            {
                ShowNotification("~r~Quantité invalide", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        private async Task ProcessPayment(PaymentMethod method)
        {
            if (currentCart.Items.Count == 0)
            {
                ShowNotification("~r~Panier vide", ErrorColor);
                return;
            }

            var items = PlayerMenu.PlayerInst.Inventory;
            var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");
            int totalPrice = currentCart.GetTotalPrice();

            bool canPay = false;
            string paymentType = "";

            if (method == PaymentMethod.Cash)
            {
                int cashAmount = dollarsItem?.Quantity ?? 0;
                canPay = cashAmount >= totalPrice;
                paymentType = "liquide";
            }
            else if (method == PaymentMethod.Bank)
            {
                canPay = PlayerMenu.PlayerInst.Money >= totalPrice;
                paymentType = "carte bancaire";
            }

            if (!canPay)
            {
                ShowNotification($"~r~Fonds insuffisants pour le paiement {paymentType}", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                return;
            }

            ShowNotification("~y~⏳ Traitement du paiement...", WarningColor);
            PlaySoundFrontend(-1, "CLICK_BACK", "WEB_NAVIGATION_SOUNDS_PHONE", false);

            await BaseScript.Delay(1500);

            foreach (var cartItem in currentCart.Items)
            {
                var ltdItem = LTDItems.FirstOrDefault(i => i.Name == cartItem.Key);
                if (ltdItem != null)
                {
                    if (method == PaymentMethod.Cash)
                    {
                        BaseScript.TriggerServerEvent("core:transaction", ltdItem.Price * cartItem.Value, cartItem.Key, cartItem.Value, "item");
                        dollarsItem.Quantity -= ltdItem.Price * cartItem.Value;
                    }
                    else
                    {
                        PlayerMenu.PlayerInst.Money -= ltdItem.Price * cartItem.Value;
                        BaseScript.TriggerServerEvent("core:transaction", ltdItem.Price * cartItem.Value, cartItem.Key, cartItem.Value, "item");
                    }
                }
            }

            ShowNotification(
                $"~g~Achat réussi !\n" +
                $"~w~Articles: ~b~{currentCart.GetTotalItems()}\n" +
                $"~w~Total: ~r~-${totalPrice}\n" +
                $"~w~Payé par: ~y~{paymentType}",
                SuccessColor
            );
            PlaySoundFrontend(-1, "PURCHASE", "HUD_LIQUOR_STORE_SOUNDSET", false);

            currentCart.Clear();

            foreach (var menu in Pool.ToList())
            {
                if (menu.Visible)
                {
                    menu.Visible = false;
                }
            }
        }

        private async Task PurchaseItem(LTDItems item, List<InventoryItem> items, InventoryItem dollarsItem)
        {
            var textInput = await Format.GetUserInput("Quantité", "1", 4);

            if (string.IsNullOrEmpty(textInput) || !int.TryParse(textInput, out int parsedInput) || parsedInput <= 0)
            {
                ShowNotification("~r~Quantité invalide", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                return;
            }

            var result = item.Price * parsedInput;

            if (result <= PlayerMenu.PlayerInst.Money)
            {
                ShowNotification("~y~⏳ Transaction en cours...", WarningColor);
                PlaySoundFrontend(-1, "CLICK_BACK", "WEB_NAVIGATION_SOUNDS_PHONE", false);

                await BaseScript.Delay(500);

                PlayerMenu.PlayerInst.Money -= result;
                PlayerMenu.PlayerInst.Inventory = items;
                BaseScript.TriggerServerEvent("core:transaction", result, item.Name, parsedInput, "item");

                ShowNotification(
                    $"~g~Achat réussi !\n" +
                    $"~w~Article: ~b~{item.Icon} {item.Name}\n" +
                    $"~w~Quantité: ~b~x{parsedInput}\n" +
                    $"~w~Total: ~r~-${result}",
                    SuccessColor
                );
                PlaySoundFrontend(-1, "PURCHASE", "HUD_LIQUOR_STORE_SOUNDSET", false);
            }
            else
            {
                int missing = result - PlayerMenu.PlayerInst.Money;
                ShowNotification(
                    $"~r~Fonds insuffisants\n" +
                    $"~w~Prix: ~g~${result}\n" +
                    $"~w~Vous avez: ~r~${PlayerMenu.PlayerInst.Money}\n" +
                    $"~w~Il manque: ~r~${missing}",
                    ErrorColor
                );
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        public void ATMMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var items = PlayerMenu.PlayerInst.Inventory;

            foreach (var ltd in LTDShops)
            {
                if (ltd.ATM == Vector3.Zero) continue;

                var distance = playerCoords.DistanceToSquared(ltd.ATM);

                if (distance < 100)
                {
                    DrawAdvancedMarker(ltd.ATM, InfoColor);
                }

                if (distance < 4)
                {
                    DrawInteractionPrompt("🏧 Distributeur", "~b~");

                    if (IsControlPressed(0, 38))
                    {
                        currentATM = ltd;
                        OpenATMMenu(items);
                    }
                }
            }
        }

        private void OpenATMMenu(List<InventoryItem> items)
        {
            var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");

            var menu = new NativeMenu(
                "🏧 Distributeur Automatique",
                "~b~Fleeca Banking Services\n~b~---------------------"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled
            };
            Pool.Add(menu);
            menu.Visible = true;

            var accountInfo = new NativeItem(
                "💳 Informations du compte",
                $"~b~---------------------\n" +
                $"~b~Solde bancaire:~w~ ~g~${PlayerMenu.PlayerInst.Money}\n" +
                $"~b~Argent liquide:~w~ ~g~${dollarsItem?.Quantity ?? 0}\n" +
                $"~b~Total:~w~ ~g~${PlayerMenu.PlayerInst.Money + (dollarsItem?.Quantity ?? 0)}\n" +
                $"~b~---------------------"
            )
            {
                Enabled = false
            };
            menu.Add(accountInfo);

            menu.Add(CreateSeparator());

            var actionBank = new NativeListItem<string>(
                "💰 Action",
                "~y~Choisissez une opération",
                "Retirer", 
                "Déposer"
            );
            menu.Add(actionBank);

            actionBank.Activated += async (sender, e) =>
            {
                await ProcessBankTransaction(actionBank.SelectedItem, items, dollarsItem);
                menu.Visible = false;
            };

            menu.Add(CreateSeparator());

            var helpInfo = new NativeItem(
                "ℹ️ Informations",
                "~y~Les transactions sont instantanées et sécurisées\n" +
                "~w~Aucun frais bancaire n'est appliqué"
            )
            {
                Enabled = false
            };
            menu.Add(helpInfo);

            menu.Closing += (sender, e) =>
            {
                currentATM = null;
            };
        }

        private async Task ProcessBankTransaction(string action, List<InventoryItem> items, InventoryItem dollarsItem)
        {
            var textInput = await Format.GetUserInput("Montant", "100", 20);

            if (string.IsNullOrEmpty(textInput) || !int.TryParse(textInput, out int parsedInput) || parsedInput <= 0)
            {
                ShowNotification("~r~Montant invalide", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                return;
            }

            string serverAction = action == "Retirer" ? "~g~<b>Retirer</b>" : "~g~<b>Déposer</b>";

            if (action == "Retirer")
            {
                if (parsedInput <= PlayerMenu.PlayerInst.Money)
                {
                    ShowNotification("~y~⏳ Retrait en cours...", WarningColor);
                    await BaseScript.Delay(1000);

                    PlayerMenu.PlayerInst.Money -= parsedInput;
                    if (dollarsItem != null)
                    {
                        dollarsItem.Quantity += parsedInput;
                    }

                    ShowNotification(
                        $"~g~Retrait effectué\n" +
                        $"~w~Montant: ~g~${parsedInput}\n" +
                        $"~w~Nouveau solde: ~g~${PlayerMenu.PlayerInst.Money}",
                        SuccessColor
                    );
                    PlaySoundFrontend(-1, "PICK_UP", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                }
                else
                {
                    ShowNotification(
                        $"~r~Solde insuffisant\n" +
                        $"~w~Demandé: ~r~${parsedInput}\n" +
                        $"~w~Disponible: ~g~${PlayerMenu.PlayerInst.Money}",
                        ErrorColor
                    );
                    PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    return;
                }
            }
            else if (action == "Déposer")
            {
                if (parsedInput <= (dollarsItem?.Quantity ?? 0))
                {
                    ShowNotification("~y~⏳ Dépôt en cours...", WarningColor);
                    await BaseScript.Delay(1000);

                    PlayerMenu.PlayerInst.Money += parsedInput;
                    if (dollarsItem != null)
                    {
                        dollarsItem.Quantity -= parsedInput;
                    }

                    ShowNotification(
                        $"~g~Dépôt effectué\n" +
                        $"~w~Montant: ~g~${parsedInput}\n" +
                        $"~w~Nouveau solde: ~g~${PlayerMenu.PlayerInst.Money}",
                        SuccessColor
                    );
                    PlaySoundFrontend(-1, "DEPOSIT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                }
                else
                {
                    ShowNotification(
                        $"~r~Argent liquide insuffisant\n" +
                        $"~w~Demandé: ~r~${parsedInput}\n" +
                        $"~w~Disponible: ~g~${dollarsItem?.Quantity ?? 0}",
                        ErrorColor
                    );
                    PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    return;
                }
            }

            PlayerMenu.PlayerInst.Inventory = items;
            BaseScript.TriggerServerEvent("core:bankTransaction", serverAction, parsedInput);
        }

        private string GetCategoryIcon(string category)
        {
            switch (category)
            {
                case "Nourriture":
                    return "🍔";
                case "Boisson":
                    return "🥤";
                case "Électronique":
                    return "📱";
                case "Tabac":
                    return "🚬";
                case "Accessoire":
                    return "🔧";
                default:
                    return "📦";
            }
        }

        private NativeItem CreateSeparator()
        {
            return new NativeItem("~b~---------------------", "")
            {
                Enabled = false
            };
        }

        private void DrawAdvancedMarker(Vector3 position, System.Drawing.Color? color = null)
        {
            var markerColor = color ?? AccentColor;
            float pulseSize = 1.0f + (float)Math.Sin(Game.GameTime / 200.0f) * 0.15f;

            World.DrawMarker(
                MarkerType.VerticalCylinder,
                position,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(1.0f, 1.0f, 0.8f),
                markerColor,
                true,
                false,
                true
            );

            World.DrawMarker(
                MarkerType.HorizontalCircleFat,
                position + new Vector3(0, 0, 0.1f),
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(pulseSize, pulseSize, 0.1f),
                System.Drawing.Color.FromArgb(80, markerColor.R, markerColor.G, markerColor.B),
                true,
                false,
                true
            );
        }

        private void DrawInteractionPrompt(string text, string color = "~g~")
        {
            SetTextFont(4);
            SetTextScale(0.5f, 0.5f);
            SetTextProportional(false);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextCentre(true);
            SetTextEntry("STRING");
            AddTextComponentString($"{color}[E]~w~ {text}");
            DrawText(0.50f, 0.90f);
        }

        private void ShowNotification(string message, System.Drawing.Color color)
        {
            Format.ShowAdvancedNotification("🏪 Supermarché", "ShurikenRP", message);
        }

        public void OnTick()
        {
            LTDMenu();
            ATMMenu();

            if (currentShop != null)
            {
                var playerCoords = GetEntityCoords(PlayerPedId(), true);
                var distance = playerCoords.DistanceToSquared(currentShop.Checkout);

                if (distance > MAX_MENU_DISTANCE * MAX_MENU_DISTANCE)
                {
                    foreach (var menu in Pool.ToList())
                    {
                        if (menu.Visible)
                        {
                            menu.Visible = false;
                        }
                    }

                    ShowNotification("~r~Vous vous êtes trop éloigné du magasin", ErrorColor);
                    PlaySoundFrontend(-1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    currentShop = null;
                }
            }

            if (currentATM != null)
            {
                var playerCoords = GetEntityCoords(PlayerPedId(), true);
                var distance = playerCoords.DistanceToSquared(currentATM.ATM);

                if (distance > MAX_MENU_DISTANCE * MAX_MENU_DISTANCE)
                {
                    foreach (var menu in Pool.ToList())
                    {
                        if (menu.Visible)
                        {
                            menu.Visible = false;
                        }
                    }

                    ShowNotification("~r~Vous vous êtes trop éloigné du distributeur", ErrorColor);
                    PlaySoundFrontend(-1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    currentATM = null;
                }
            }
        }
    }

    public class LTDShopInfo
    {
        public string LTDName { get; set; }
        public string Description { get; set; }
        public Vector3 Checkout { get; set; }
        public Vector3 PNJCoords { get; set; }
        public Vector3 ATM { get; set; }

        public LTDShopInfo(string ltdName, string description, Vector3 checkout, Vector3 pnjCoords, Vector3 atm)
        {
            LTDName = ltdName;
            Description = description;
            Checkout = checkout;
            PNJCoords = pnjCoords;
            ATM = atm;
        }
    }

    public class ShoppingCart
    {
        public Dictionary<string, int> Items { get; private set; }
        private Dictionary<string, int> itemPrices;

        public ShoppingCart()
        {
            Items = new Dictionary<string, int>();
            itemPrices = new Dictionary<string, int>();
        }

        public void AddItem(string itemName, int quantity, int unitPrice)
        {
            if (Items.ContainsKey(itemName))
            {
                Items[itemName] += quantity;
            }
            else
            {
                Items[itemName] = quantity;
            }
            itemPrices[itemName] = unitPrice;
        }

        public void RemoveItem(string itemName)
        {
            if (Items.ContainsKey(itemName))
            {
                Items.Remove(itemName);
                itemPrices.Remove(itemName);
            }
        }

        public void UpdateItemQuantity(string itemName, int newQuantity)
        {
            if (Items.ContainsKey(itemName))
            {
                if (newQuantity <= 0)
                {
                    RemoveItem(itemName);
                }
                else
                {
                    Items[itemName] = newQuantity;
                }
            }
        }

        public int GetItemQuantity(string itemName)
        {
            return Items.ContainsKey(itemName) ? Items[itemName] : 0;
        }

        public int GetTotalItems()
        {
            return Items.Values.Sum();
        }

        public int GetTotalPrice()
        {
            return Items.Sum(item => item.Value * (itemPrices.ContainsKey(item.Key) ? itemPrices[item.Key] : 0));
        }

        public void Clear()
        {
            Items.Clear();
            itemPrices.Clear();
        }
    }
    public enum PaymentMethod
    {
        Cash,
        Bank
    }
}