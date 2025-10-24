using CitizenFX.Core;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Shared;
using static CitizenFX.Core.Native.API;
using System.ComponentModel;

namespace Core.Client
{
    public class ClothShop
    {
        ClientMain Client;
        Format Format;
        ObjectPool Pool = new ObjectPool();
        PlayerMenu PlayerMenu;
        private ClothShopInfo currentShop = null;
        private const float MAX_MENU_DISTANCE = 15f;
        List<ClothShopInfo> ClothShopList = new List<ClothShopInfo>();

        // Palette de couleurs
        private readonly System.Drawing.Color AccentColor = System.Drawing.Color.FromArgb(255, 255, 87, 34);
        private readonly System.Drawing.Color SuccessColor = System.Drawing.Color.FromArgb(255, 76, 175, 80);
        private readonly System.Drawing.Color ErrorColor = System.Drawing.Color.FromArgb(255, 244, 67, 54);
        private readonly System.Drawing.Color InfoColor = System.Drawing.Color.FromArgb(255, 33, 150, 243);

        // Sauvegarde temporaire pour annulation
        private Dictionary<int, int> originalClothing = new Dictionary<int, int>();
        private Dictionary<int, int> originalTextures = new Dictionary<int, int>();
        private Dictionary<int, int> originalProps = new Dictionary<int, int>();
        private Dictionary<int, int> originalPropTextures = new Dictionary<int, int>();

        // Prix des articles
        private const int HAT_PRICE = 150;
        private const int TOP_PRICE = 250;
        private const int LEGS_PRICE = 200;
        private const int SHOES_PRICE = 180;
        private const int GLASSES_PRICE = 120;
        private const int WATCH_PRICE = 150;
        private const int MASK_PRICE = 100;
        private const int COMPLETE_OUTFIT_PRICE = 800;

        public ClothShop(ClientMain caller)
        {
            Client = caller;
            Pool = caller.Pool;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            InitializeShops();
            CreateShopBlips();
        }

        private void InitializeShops()
        {
            ClothShopInfo shop0 = new ClothShopInfo("Cube Place", "Boutique tendance du centre", new Vector3(197, -876.5f, 30.6f), new Vector3(200.4f, -870.9f, 29.8f));
            ClothShopList.Add(shop0);

            ClothShopInfo shop1 = new ClothShopInfo("Strawberry", "Mode urbaine", new Vector3(75.7f, -1394.2f, 29.3f), new Vector3(73.9f, -1393.1f, 28.6f));
            ClothShopList.Add(shop1);

            ClothShopInfo shop2 = new ClothShopInfo("Textile City", "Vêtements de qualité", new Vector3(424.9f, -804.8f, 29.4f), new Vector3(427.2f, -805.9f, 28.6f));
            ClothShopList.Add(shop2);

            ClothShopInfo shop3 = new ClothShopInfo("Hawick", "Fashion district", new Vector3(123, -223.7f, 54.5f), new Vector3(127.1f, -224.5f, 53.6f));
            ClothShopList.Add(shop3);

            ClothShopInfo shop4 = new ClothShopInfo("Burton", "Style élégant", new Vector3(-162.1f, -303.2f, 39.7f), new Vector3(-164.9f, -302, 38.8f));
            ClothShopList.Add(shop4);

            ClothShopInfo shop5 = new ClothShopInfo("Rockford Hills", "Luxe et prestige", new Vector3(-711, -153.5f, 37.3f), new Vector3(-708.3f, -152.6f, 36.6f));
            ClothShopList.Add(shop5);

            ClothShopInfo shop6 = new ClothShopInfo("Vespucci", "Mode plage", new Vector3(-823.1f, -1074.5f, 11.2f), new Vector3(-823.3f, -1072.2f, 10.4f));
            ClothShopList.Add(shop6);

            ClothShopInfo shop7 = new ClothShopInfo("Del Perro", "Casual chic", new Vector3(-1191, -769, 17.3f), new Vector3(-1193.8f, -766.5f, 16.4f));
            ClothShopList.Add(shop7);

            ClothShopInfo shop8 = new ClothShopInfo("Chumash", "Style décontracté", new Vector3(-3173.7f, 1043.5f, 20.7f), new Vector3(-3169.1f, 1042.7f, 19.9f));
            ClothShopList.Add(shop8);

            ClothShopInfo shop9 = new ClothShopInfo("Harmony", "Boutique locale", new Vector3(616.6f, 2763.9f, 42), new Vector3(612.7f, 2763.4f, 41.2f));
            ClothShopList.Add(shop9);

            ClothShopInfo shop10 = new ClothShopInfo("Grapeseed", "Mode rurale", new Vector3(1693.1f, 4824.3f, 42), new Vector3(1695.3f, 4823.3f, 41.2f));
            ClothShopList.Add(shop10);

            ClothShopInfo shop11 = new ClothShopInfo("Grand Senora Desert", "Style western", new Vector3(1195.4f, 2710, 38.1f), new Vector3(1196.4f, 2711.7f, 37.4f));
            ClothShopList.Add(shop11);

            ClothShopInfo shop12 = new ClothShopInfo("Paleto Bay", "Vêtements du nord", new Vector3(5, 6513.8f, 31.7f), new Vector3(5.9f, 6511.3f, 30.9f));
            ClothShopList.Add(shop12);

            ClothShopInfo shop13 = new ClothShopInfo("Casino", "Collection exclusive", new Vector3(1102, 198.7f, -50f), new Vector3(1100.4f, 195.3f, -50));
            ClothShopList.Add(shop13);
        }

        private void CreateShopBlips()
        {
            foreach (var cloth in ClothShopList)
            {
                Blip myBlip = World.CreateBlip(cloth.Checkout);
                myBlip.Sprite = BlipSprite.Clothes;
                myBlip.Name = $"{cloth.ShopName}";
                myBlip.Color = BlipColor.TrevorOrange;
                myBlip.IsShortRange = true;
                myBlip.Scale = 0.8f;
            }
        }

        public async void CreatePeds()
        {
            foreach (var cloth in ClothShopList)
            {
                var pedHash = PedHash.Malibu01AMM;
                RequestModel((uint)pedHash);
                while (!HasModelLoaded((uint)pedHash))
                {
                    await BaseScript.Delay(100);
                }
                var ped = World.CreatePed(pedHash, cloth.PNJCoords);
                FreezeEntityPosition(ped.Result.Handle, true);
                SetEntityInvincible(ped.Result.Handle, true);
                SetBlockingOfNonTemporaryEvents(ped.Result.Handle, true);
                Client.PedId.Add(ped.Result.Handle);
            }
        }

        private void SaveCurrentClothing()
        {
            originalClothing.Clear();
            originalTextures.Clear();
            originalProps.Clear();
            originalPropTextures.Clear();

            int ped = GetPlayerPed(-1);

            // Sauvegarder les vêtements
            for (int i = 0; i < 12; i++)
            {
                originalClothing[i] = GetPedDrawableVariation(ped, i);
                originalTextures[i] = GetPedTextureVariation(ped, i);
            }

            // Sauvegarder les accessoires
            for (int i = 0; i < 7; i++)
            {
                originalProps[i] = GetPedPropIndex(ped, i);
                originalPropTextures[i] = GetPedPropTextureIndex(ped, i);
            }
        }

        private void RestoreOriginalClothing()
        {
            int ped = GetPlayerPed(-1);

            foreach (var item in originalClothing)
            {
                SetPedComponentVariation(ped, item.Key, item.Value, originalTextures[item.Key], 2);
            }

            foreach (var item in originalProps)
            {
                if (item.Value >= 0)
                {
                    SetPedPropIndex(ped, item.Key, item.Value, originalPropTextures[item.Key], false);
                }
            }

            ShowNotification("~y~Tenue d'origine restaurée", InfoColor);
        }

        private void RotatePlayer()
        {
            int ped = GetPlayerPed(-1);
            float heading = GetEntityHeading(ped);
            SetEntityHeading(ped, heading + 20f);
        }

        public void NewClothMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), true);

            foreach (var cloth in ClothShopList)
            {
                var distance = playerCoords.DistanceToSquared(cloth.Checkout);

                if (distance < 100)
                {
                    DrawAdvancedMarker(cloth.Checkout);
                }

                if (distance < 8)
                {
                    DrawInteractionPrompt($"{cloth.ShopName}");

                    if (IsControlJustPressed(0, 38))
                    {
                        SaveCurrentClothing();
                        OpenMainClothingMenu(cloth);
                    }
                }
            }
        }

        private void OpenMainClothingMenu(ClothShopInfo cloth)
        {
            currentShop = cloth;
            SaveCurrentClothing();

            var menu = new NativeMenu(
                "Magasin de Vêtements",
                $"~o~{cloth.ShopName}~w~ - {cloth.Description}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };
            Pool.Add(menu);
            menu.Visible = true;

            // Séparateur
            menu.Add(CreateSeparator());

            // Section Tenues complètes
            var outfitMenu = CreateOutfitMenu();
            menu.AddSubMenu(outfitMenu);
            Pool.Add(outfitMenu);

            // Header Accessoires
            var headerAccessories = new NativeItem("~h~ACCESSOIRES", "")
            {
                Enabled = false
            };
            menu.Add(headerAccessories);
            menu.Add(CreateSeparator());

            // Chapeaux
            var hatMenu = CreateHatMenu();
            menu.AddSubMenu(hatMenu);
            Pool.Add(hatMenu);

            // Lunettes
            var glassesMenu = CreateGlassesMenu();
            menu.AddSubMenu(glassesMenu);
            Pool.Add(glassesMenu);

            // Montres
            var watchesMenu = CreateWatchesMenu();
            menu.AddSubMenu(watchesMenu);
            Pool.Add(watchesMenu);

            // Masques
            var masksMenu = CreateMasksMenu();
            menu.AddSubMenu(masksMenu);
            Pool.Add(masksMenu);

            // Séparateur
            menu.Add(CreateSeparator());

            // Header Vêtements
            var headerClothing = new NativeItem("~h~VÊTEMENTS", "")
            {
                Enabled = false
            };
            menu.Add(headerClothing);
            menu.Add(CreateSeparator());

            // Haut
            var topMenu = CreateTopMenu();
            menu.AddSubMenu(topMenu);
            Pool.Add(topMenu);

            // Bas
            var legsMenu = CreateLegsMenu();
            menu.AddSubMenu(legsMenu);
            Pool.Add(legsMenu);

            // Chaussures
            var shoesMenu = CreateShoesMenu();
            menu.AddSubMenu(shoesMenu);

            // Séparateur
            menu.Add(CreateSeparator());

            // Options
            var rotateItem = new NativeItem("Tourner le personnage", "~y~Faire pivoter pour voir tous les angles");
            menu.Add(rotateItem);
            rotateItem.Activated += (sender, e) =>
            {
                RotatePlayer();
                PlaySoundFrontend(-1, "CLICK_BACK", "WEB_NAVIGATION_SOUNDS_PHONE", false);
            };

            var resetItem = new NativeItem("Annuler les modifications", "~r~Restaurer la tenue d'origine");
            menu.Add(resetItem);
            resetItem.Activated += (sender, e) =>
            {
                RestoreOriginalClothing();
                PlaySoundFrontend(-1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Closing += (sender, e) =>
            {
                currentShop = null;
                BaseScript.TriggerServerEvent("core:setClothes");
            };
        }

        private NativeMenu CreateOutfitMenu()
        {
            var menu = new NativeMenu(
                "Tenues Complètes",
                $"~b~~w~Achetez une tenue complète ~g~${COMPLETE_OUTFIT_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            var casualItem = new NativeItem("Style Casual", "~w~Tenue décontractée pour tous les jours", $"~g~${COMPLETE_OUTFIT_PRICE}");
            var businessItem = new NativeItem("Style Business", "~w~Look professionnel et élégant", $"~g~${COMPLETE_OUTFIT_PRICE}");
            var sportItem = new NativeItem("Style Sport", "~w~Confortable pour l'activité physique", $"~g~${COMPLETE_OUTFIT_PRICE}");
            var elegantItem = new NativeItem("Style Élégant", "~w~Pour les grandes occasions", $"~g~${COMPLETE_OUTFIT_PRICE}");

            menu.Add(casualItem);
            menu.Add(businessItem);
            menu.Add(sportItem);
            menu.Add(elegantItem);

            return menu;
        }

        private NativeMenu CreateHatMenu()
        {
            var menu = new NativeMenu(
                "Chapeaux",
                $"~b~~w~Casquettes, bérets, chapeaux ~g~${HAT_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            var hatsList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 0)).ToList();
            NativeListItem<int> itemsHat = new NativeListItem<int>("~h~Modèle", "", hatsList.ToArray());
            menu.Add(itemsHat);

            var hatsTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 0, itemsHat.SelectedIndex)).ToList();
            NativeListItem<int> itemHatTexture = new NativeListItem<int>("~h~Couleur/Style", "", hatsTextureList.ToArray());
            menu.Add(itemHatTexture);

            itemsHat.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, 0, false);
                itemHatTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            itemHatTexture.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, itemHatTexture.SelectedIndex, false);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            var previewInfo = new NativeItem(
                "Aperçu en direct",
                "~y~Les modifications sont visibles en temps réel\n~w~Utilisez ~b~Tourner~w~ pour voir sous tous les angles"
            )
            {
                Enabled = false
            };
            menu.Add(previewInfo);

            var removeItem = new NativeItem("Retirer le chapeau", "~r~Enlever l'accessoire actuel");
            menu.Add(removeItem);
            removeItem.Activated += (sender, e) =>
            {
                ClearPedProp(GetPlayerPed(-1), 0);
                PlaySoundFrontend(-1, "CANCEL", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            var buyItem = new NativeItem("Acheter", $"~g~Prix: ${HAT_PRICE}");
            menu.Add(buyItem);
            buyItem.Activated += async (sender, e) =>
            {
                await PurchaseClothing(HAT_PRICE, "Chapeau", () =>
                {
                    var clothingSet = new ClothingSet { Name = "Chapeau" };
                    clothingSet.Components.Add(new ClothesComponent(0)
                    {
                        Drawable = itemsHat.SelectedIndex,
                        Texture = itemHatTexture.SelectedIndex,
                        Palette = 0
                    });
                    return clothingSet;
                });
            };

            return menu;
        }

        private NativeMenu CreateTopMenu()
        {
            var menu = new NativeMenu(
                "Hauts",
                $"~b~~w~T-shirts, chemises, vestes ~g~${TOP_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            // UNDERSHIRT
            var undershirtList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 8)).ToList();
            NativeListItem<int> undershirts = new NativeListItem<int>("~h~Sous-vêtement", "", undershirtList.ToArray());
            menu.Add(undershirts);

            var undershirtsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 8, undershirts.SelectedIndex)).ToList();
            NativeListItem<int> undershirtsTexture = new NativeListItem<int>("~h~Style", "", undershirtsTextureList.ToArray());
            menu.Add(undershirtsTexture);

            undershirts.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, 0, 1);
                undershirtsTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            undershirtsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, undershirtsTexture.SelectedIndex, 1);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            // ARMS
            var armsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 3)).ToList();
            NativeListItem<int> arms = new NativeListItem<int>("~h~Bras/Torso", "", armsList.ToArray());
            menu.Add(arms);

            var armsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 3, arms.SelectedIndex)).ToList();
            NativeListItem<int> armsTexture = new NativeListItem<int>("~h~Style", "", armsTextureList.ToArray());
            menu.Add(armsTexture);

            arms.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, 0, 1);
                armsTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            armsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, armsTexture.SelectedIndex, 1);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            // TOP
            var topsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 11)).ToList();
            NativeListItem<int> tops = new NativeListItem<int>("~h~Veste/Haut", "", topsList.ToArray());
            menu.Add(tops);

            var topsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 11, tops.SelectedIndex)).ToList();
            NativeListItem<int> topsTexture = new NativeListItem<int>("~h~Style", "", topsTextureList.ToArray());
            menu.Add(topsTexture);

            tops.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, 0, 1);
                topsTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            topsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, topsTexture.SelectedIndex, 1);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            var buyItem = new NativeItem("Acheter le haut complet", $"~g~Prix: ${TOP_PRICE}");
            menu.Add(buyItem);
            buyItem.Activated += async (sender, e) =>
            {
                await PurchaseClothing(TOP_PRICE, "Haut", () =>
                {
                    var clothingSet = new ClothingSet { Name = "Haut" };
                    clothingSet.Components.Add(new ClothesComponent(8)
                    {
                        Drawable = undershirts.SelectedIndex,
                        Texture = undershirtsTexture.SelectedIndex,
                        Palette = 1
                    });
                    clothingSet.Components.Add(new ClothesComponent(3)
                    {
                        Drawable = arms.SelectedIndex,
                        Texture = armsTexture.SelectedIndex,
                        Palette = 1
                    });
                    clothingSet.Components.Add(new ClothesComponent(11)
                    {
                        Drawable = tops.SelectedIndex,
                        Texture = topsTexture.SelectedIndex,
                        Palette = 1
                    });
                    return clothingSet;
                });
            };

            return menu;
        }

        private NativeMenu CreateLegsMenu()
        {
            var menu = new NativeMenu(
                "Bas",
                $"~b~~w~Pantalons, jeans, shorts ~g~${LEGS_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            var legsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 4)).ToList();
            NativeListItem<int> legsItem = new NativeListItem<int>("~h~Modèle", "", legsList.ToArray());
            menu.Add(legsItem);

            var legsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 4, legsItem.SelectedIndex)).ToList();
            NativeListItem<int> legsTexture = new NativeListItem<int>("~h~Couleur/Style", "", legsTextureList.ToArray());
            menu.Add(legsTexture);

            legsItem.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, 0, 1);
                legsTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            legsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, legsTexture.SelectedIndex, 1);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            var buyItem = new NativeItem("Acheter", $"~g~Prix: ${LEGS_PRICE}");
            menu.Add(buyItem);
            buyItem.Activated += async (sender, e) =>
            {
                await PurchaseClothing(LEGS_PRICE, "Pantalon", () =>
                {
                    var clothingSet = new ClothingSet { Name = "Pantalon" };
                    clothingSet.Components.Add(new ClothesComponent(4)
                    {
                        Drawable = legsItem.SelectedIndex,
                        Texture = legsTexture.SelectedIndex,
                        Palette = 1
                    });
                    return clothingSet;
                });
            };

            return menu;
        }

        private NativeMenu CreateShoesMenu()
        {
            var menu = new NativeMenu(
                "Chaussures",
                $"~b~~w~Baskets, bottes, chaussures ~g~${SHOES_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            var shoesList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 6)).ToList();
            NativeListItem<int> shoesItem = new NativeListItem<int>("~h~Modèle", "", shoesList.ToArray());
            menu.Add(shoesItem);

            var shoesTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 6, shoesItem.SelectedIndex)).ToList();
            NativeListItem<int> shoesTexture = new NativeListItem<int>("~h~Couleur/Style", "", shoesTextureList.ToArray());
            menu.Add(shoesTexture);

            shoesItem.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, 0, 1);
                shoesTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            shoesTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, shoesTexture.SelectedIndex, 1);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            var buyItem = new NativeItem("Acheter", $"~g~Prix: ${SHOES_PRICE}");
            menu.Add(buyItem);
            buyItem.Activated += async (sender, e) =>
            {
                await PurchaseClothing(SHOES_PRICE, "Chaussures", () =>
                {
                    var clothingSet = new ClothingSet { Name = "Chaussures" };
                    clothingSet.Components.Add(new ClothesComponent(6)
                    {
                        Drawable = shoesItem.SelectedIndex,
                        Texture = shoesTexture.SelectedIndex,
                        Palette = 1
                    });
                    return clothingSet;
                });
            };

            return menu;
        }

        private NativeMenu CreateGlassesMenu()
        {
            var menu = new NativeMenu(
                "Lunettes",
                $"~b~~w~Lunettes de soleil et optiques ~g~${GLASSES_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            var glassesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 1)).ToList();
            NativeListItem<int> itemsGlasses = new NativeListItem<int>("~h~Modèle", "", glassesList.ToArray());
            menu.Add(itemsGlasses);

            var glassesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex)).ToList();
            NativeListItem<int> itemGlassesTexture = new NativeListItem<int>("~h~Couleur", "", glassesTextureList.ToArray());
            menu.Add(itemGlassesTexture);

            itemsGlasses.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, 0, false);
                itemGlassesTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            itemGlassesTexture.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, itemGlassesTexture.SelectedIndex, false);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            var removeItem = new NativeItem("Retirer les lunettes", "");
            menu.Add(removeItem);
            removeItem.Activated += (sender, e) =>
            {
                ClearPedProp(GetPlayerPed(-1), 1);
                PlaySoundFrontend(-1, "CANCEL", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            var buyItem = new NativeItem("Acheter", $"~g~Prix: ${GLASSES_PRICE}");
            menu.Add(buyItem);
            buyItem.Activated += async (sender, e) =>
            {
                await PurchaseClothing(GLASSES_PRICE, "Lunettes", () =>
                {
                    var clothingSet = new ClothingSet { Name = "Lunettes" };
                    clothingSet.Components.Add(new ClothesComponent(1)
                    {
                        Drawable = itemsGlasses.SelectedIndex,
                        Texture = itemGlassesTexture.SelectedIndex,
                        Palette = 0
                    });
                    return clothingSet;
                });
            };

            return menu;
        }

        private NativeMenu CreateWatchesMenu()
        {
            var menu = new NativeMenu(
                "Montres",
                $"~b~~w~Montres et bracelets ~g~${WATCH_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            var watchesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 6)).ToList();
            NativeListItem<int> itemsWatches = new NativeListItem<int>("~h~Modèle", "", watchesList.ToArray());
            menu.Add(itemsWatches);

            var watchesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex)).ToList();
            NativeListItem<int> itemWatchesTexture = new NativeListItem<int>("~h~Couleur", "", watchesTextureList.ToArray());
            menu.Add(itemWatchesTexture);

            itemsWatches.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, 0, false);
                itemWatchesTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            itemWatchesTexture.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, itemWatchesTexture.SelectedIndex, false);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            var removeItem = new NativeItem("❌ Retirer la montre", "");
            menu.Add(removeItem);
            removeItem.Activated += (sender, e) =>
            {
                ClearPedProp(GetPlayerPed(-1), 6);
                PlaySoundFrontend(-1, "CANCEL", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            var buyItem = new NativeItem("Acheter", $"~g~Prix: ${WATCH_PRICE}");
            menu.Add(buyItem);
            buyItem.Activated += async (sender, e) =>
            {
                await PurchaseClothing(WATCH_PRICE, "Montre", () =>
                {
                    var clothingSet = new ClothingSet { Name = "Montre" };
                    clothingSet.Components.Add(new ClothesComponent(6)
                    {
                        Drawable = itemsWatches.SelectedIndex,
                        Texture = itemWatchesTexture.SelectedIndex,
                        Palette = 0
                    });
                    return clothingSet;
                });
            };

            return menu;
        }

        private NativeMenu CreateMasksMenu()
        {
            var menu = new NativeMenu(
                "Masques",
                $"~b~~w~Masques et accessoires visage ~g~${MASK_PRICE}"
            )
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };

            var masksList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 1)).ToList();
            NativeListItem<int> masksItem = new NativeListItem<int>("~h~Modèle", "", masksList.ToArray());
            menu.Add(masksItem);

            var masksTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 1, masksItem.SelectedIndex)).ToList();
            NativeListItem<int> masksTexture = new NativeListItem<int>("~h~Style", "", masksTextureList.ToArray());
            menu.Add(masksTexture);

            masksItem.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 1, masksItem.SelectedIndex, 0, 1);
                masksTexture.SelectedIndex = 0;
                PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            masksTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 1, masksItem.SelectedIndex, masksTexture.SelectedIndex, 1);
                PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            menu.Add(CreateSeparator());

            var removeItem = new NativeItem("Retirer le masque", "");
            menu.Add(removeItem);
            removeItem.Activated += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 1, 0, 0, 2);
                PlaySoundFrontend(-1, "CANCEL", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            };

            var buyItem = new NativeItem("💰 Acheter", $"~g~Prix: ${MASK_PRICE}");
            menu.Add(buyItem);
            buyItem.Activated += async (sender, e) =>
            {
                await PurchaseClothing(MASK_PRICE, "Masque", () =>
                {
                    var clothingSet = new ClothingSet { Name = "Masque" };
                    clothingSet.Components.Add(new ClothesComponent(1)
                    {
                        Drawable = masksItem.SelectedIndex,
                        Texture = masksTexture.SelectedIndex,
                        Palette = 1
                    });
                    return clothingSet;
                });
            };

            return menu;
        }

        private async Task PurchaseClothing(int price, string itemName, Func<ClothingSet> getClothingSet)
        {
            if (PlayerMenu.PlayerInst.Money >= price)
            {
                var textInput = await Format.GetUserInput("Nom de la tenue", itemName, 20);

                if (string.IsNullOrEmpty(textInput))
                {
                    ShowNotification("~r~Nom invalide", ErrorColor);
                    return;
                }

                var clothingSet = getClothingSet();
                clothingSet.Name = textInput;

                var json = JsonConvert.SerializeObject(clothingSet);
                BaseScript.TriggerServerEvent("core:buyTopClothes", price, json);

                ShowNotification($"~g~{itemName} acheté avec succès\n~w~Nom: ~b~{textInput}\n~r~-${price}", SuccessColor);
                PlaySoundFrontend(-1, "PURCHASE", "HUD_LIQUOR_STORE_SOUNDSET", false);
            }
            else
            {
                ShowNotification($"~r~Fonds insuffisants\n~w~Il vous manque ~r~${price - PlayerMenu.PlayerInst.Money}", ErrorColor);
                PlaySoundFrontend(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        private NativeItem CreateSeparator()
        {
            return new NativeItem("~b~---------------------", "")
            {
                Enabled = false
            };
        }

        private void DrawAdvancedMarker(Vector3 position)
        {
            float pulseSize = 1.0f + (float)Math.Sin(Game.GameTime / 200.0f) * 0.15f;

            World.DrawMarker(
                MarkerType.VerticalCylinder,
                position,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(1.2f, 1.2f, 0.8f),
                AccentColor,
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
                System.Drawing.Color.FromArgb(80, AccentColor.R, AccentColor.G, AccentColor.B),
                true,
                false,
                true
            );
        }

        private void DrawInteractionPrompt(string shopName)
        {
            SetTextFont(4);
            SetTextScale(0.5f, 0.5f);
            SetTextProportional(false);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextCentre(true);
            SetTextEntry("STRING");
            AddTextComponentString($"~o~[E]~w~ {shopName}");
            DrawText(0.50f, 0.90f);
        }

        private void ShowNotification(string message, System.Drawing.Color color)
        {
            Format.ShowAdvancedNotification("Magasin de Vêtements", "ShurikenRP", message);
        }

        public void OnTick()
        {
            NewClothMenu();

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

                    RestoreOriginalClothing();
                    ShowNotification("~r~Vous vous êtes trop éloigné du magasin", ErrorColor);
                    PlaySoundFrontend(-1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    currentShop = null;
                }
            }
        }
        
    }

    public class ClothShopInfo
    {
        public string ShopName { get; set; }
        public string Description { get; set; }
        public Vector3 Checkout { get; set; }
        public Vector3 PNJCoords { get; set; }

        public ClothShopInfo(string shopName, string description, Vector3 checkout, Vector3 pnjCoords)
        {
            ShopName = shopName;
            Description = description;
            Checkout = checkout;
            PNJCoords = pnjCoords;
        }
    }
}