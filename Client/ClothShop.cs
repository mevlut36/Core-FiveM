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

        List<ClothShopInfo> ClothShopList = new List<ClothShopInfo>();

        public ClothShop(ClientMain caller)
        {
            Client = caller;
            Pool = caller.Pool;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            ClothShopInfo shop0 = new ClothShopInfo("Cube Place", new Vector3(197, -876.5f, 30.6f), new Vector3(200.4f, -870.9f, 29.8f));
            ClothShopList.Add(shop0);
            ClothShopInfo shop1 = new ClothShopInfo("Strawberry", new Vector3(75.7f, -1394.2f, 29.3f), new Vector3(73.9f, -1393.1f, 28.6f));
            ClothShopList.Add(shop1);
            ClothShopInfo shop2 = new ClothShopInfo("Textile City", new Vector3(424.9f, -804.8f, 29.4f), new Vector3(427.2f, -805.9f, 28.6f));
            ClothShopList.Add(shop2);
            ClothShopInfo shop3 = new ClothShopInfo("Hawick", new Vector3(123, -223.7f, 54.5f), new Vector3(127.1f, -224.5f, 53.6f));
            ClothShopList.Add(shop3);
            ClothShopInfo shop4 = new ClothShopInfo("Burton", new Vector3(-162.1f, -303.2f, 39.7f), new Vector3(-164.9f, -302, 38.8f));
            ClothShopList.Add(shop4);
            ClothShopInfo shop5 = new ClothShopInfo("Rockford Hills", new Vector3(-711, -153.5f, 37.3f), new Vector3(-708.3f, -152.6f, 36.6f));
            ClothShopList.Add(shop5);
            ClothShopInfo shop6 = new ClothShopInfo("Vespucci", new Vector3(-823.1f, -1074.5f, 11.2f), new Vector3(-823.3f, -1072.2f, 10.4f));
            ClothShopList.Add(shop6);
            ClothShopInfo shop7 = new ClothShopInfo("Del Perro", new Vector3(-1191, -769, 17.3f), new Vector3(-1193.8f, -766.5f, 16.4f));
            ClothShopList.Add(shop7);
            ClothShopInfo shop8 = new ClothShopInfo("Chumash", new Vector3(-3173.7f, 1043.5f, 20.7f), new Vector3(-3169.1f, 1042.7f, 19.9f));
            ClothShopList.Add(shop8);
            ClothShopInfo shop9 = new ClothShopInfo("Harmony", new Vector3(616.6f, 2763.9f, 42), new Vector3(612.7f, 2763.4f, 41.2f));
            ClothShopList.Add(shop9);
            ClothShopInfo shop10 = new ClothShopInfo("Grapeseed", new Vector3(1693.1f, 4824.3f, 42), new Vector3(1695.3f, 4823.3f, 41.2f));
            ClothShopList.Add(shop10);
            ClothShopInfo shop11 = new ClothShopInfo("Grand Senora Desert", new Vector3(1195.4f, 2710, 38.1f), new Vector3(1196.4f, 2711.7f, 37.4f));
            ClothShopList.Add(shop11);
            ClothShopInfo shop12 = new ClothShopInfo("Paleto Bay", new Vector3(5, 6513.8f, 31.7f), new Vector3(5.9f, 6511.3f, 30.9f));
            ClothShopList.Add(shop12);
            ClothShopInfo shop13 = new ClothShopInfo("Casino", new Vector3(1102, 198.7f, -50f), new Vector3(1100.4f, 195.3f, -50));
            ClothShopList.Add(shop13);
        }

        public async void CreatePeds()
        {
            foreach (var cloth in ClothShopList)
            {
                Blip myBlip = World.CreateBlip(cloth.Checkout);
                myBlip.Sprite = BlipSprite.Clothes;
                myBlip.Name = "Magasin de vêtements";
                myBlip.Color = BlipColor.TrevorOrange;
                myBlip.IsShortRange = true;
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


        public void NewClothMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), true);

            foreach (var cloth in ClothShopList)
            {
                var distance = playerCoords.DistanceToSquared(cloth.Checkout);
                if (distance < 8)
                {
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                    if (IsControlJustPressed(0, 38))
                    {
                        var menu = new NativeMenu("Magasin de vêtements", $"Magasin - {cloth.ShopName}")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(menu);
                        menu.Visible = true;

                        menu.Closed += (sender, e) =>
                        {
                            BaseScript.TriggerServerEvent("core:setClothes");
                        };

                        var hat = new NativeMenu("Chapeaux, casquettes, bérets", "Chapeaux")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(hat);
                        menu.AddSubMenu(hat);

                        var hatsList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 0)).ToList();
                        NativeListItem<int> itemsHat = new NativeListItem<int>("~h~Chapeaux~s~", hatsList.ToArray());
                        hat.Add(itemsHat);

                        var hatsTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 0, itemsHat.SelectedIndex)).ToList();
                        NativeListItem<int> itemHatTexture = new NativeListItem<int>($"Style", hatsTextureList.ToArray());
                        hat.Add(itemHatTexture);

                        itemsHat.ItemChanged += (sender, e) =>
                        {
                            SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, 0, false);
                            itemHatTexture.SelectedIndex = 0;
                        };

                        itemHatTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, itemHatTexture.SelectedIndex, false);
                        };

                        var hatSubmit = new NativeItem("Acheter", "", "~g~250$");
                        hat.Add(hatSubmit);

                        hatSubmit.Activated += async (sender, e) =>
                        {
                            if (PlayerMenu.PlayerInst.Money >= 250)
                            {
                                var textInput = await Format.GetUserInput("Donnez un nom", "Chapeau", 12);

                                var clothingSet = new ClothingSet
                                {
                                    Name = textInput
                                };

                                clothingSet.Components.Add(new ClothesComponent(0)  // Hat
                                {
                                    Drawable = itemsHat.SelectedIndex,
                                    Texture = itemHatTexture.SelectedIndex,
                                    Palette = 0
                                });

                                var json = JsonConvert.SerializeObject(clothingSet);
                                BaseScript.TriggerServerEvent("core:buyTopClothes", 250, json);
                            }
                        };

                        var top = new NativeMenu("Sous-haut, haut, bras", "Haut")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(top);
                        menu.AddSubMenu(top);

                        // UNDERSHIRT
                        var undershirtList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 8)).ToList();
                        NativeListItem<int> undershirts = new NativeListItem<int>("~h~Sous haut~s~", undershirtList.ToArray());
                        top.Add(undershirts);

                        var undershirtsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 8, undershirts.SelectedIndex)).ToList();
                        NativeListItem<int> undershirtsTexture = new NativeListItem<int>("Style", undershirtsTextureList.ToArray());
                        top.Add(undershirtsTexture);

                        undershirts.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, 0, 1);
                            undershirtsTexture.SelectedIndex = 0;
                        };

                        undershirtsTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, undershirtsTexture.SelectedIndex, 1);
                        };

                        // ARMS
                        var armsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 3)).ToList();
                        NativeListItem<int> arms = new NativeListItem<int>("~h~Bras~s~", armsList.ToArray());
                        top.Add(arms);

                        var armsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 3, arms.SelectedIndex)).ToList();
                        NativeListItem<int> armsTexture = new NativeListItem<int>("Style", armsTextureList.ToArray());
                        top.Add(armsTexture);

                        arms.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, 0, 1);
                            armsTexture.SelectedIndex = 0;
                        };

                        armsTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, armsTexture.SelectedIndex, 1);
                        };

                        // TOP
                        var topsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 11)).ToList();
                        NativeListItem<int> tops = new NativeListItem<int>("~h~Haut~s~", topsList.ToArray());
                        top.Add(tops);

                        var topsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 11, tops.SelectedIndex)).ToList();
                        NativeListItem<int> topsTexture = new NativeListItem<int>("Style", topsTextureList.ToArray());
                        top.Add(topsTexture);

                        tops.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, 0, 1);
                            topsTexture.SelectedIndex = 0;
                        };

                        topsTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, topsTexture.SelectedIndex, 1);
                        };

                        var topSubmit = new NativeItem("Acheter", "", "~g~250$");
                        top.Add(topSubmit);

                        topSubmit.Activated += async (sender, e) =>
                        {
                            if (PlayerMenu.PlayerInst.Money >= 250)
                            {
                                var textInput = await Format.GetUserInput("Donnez un nom", "Habit", 12);

                                var clothingSet = new ClothingSet
                                {
                                    Name = textInput
                                };

                                clothingSet.Components.Add(new ClothesComponent(8)  // Undershirt
                                {
                                    Drawable = undershirts.SelectedIndex,
                                    Texture = undershirtsTexture.SelectedIndex,
                                    Palette = 1
                                });

                                clothingSet.Components.Add(new ClothesComponent(3)  // Torso
                                {
                                    Drawable = arms.SelectedIndex,
                                    Texture = armsTexture.SelectedIndex,
                                    Palette = 1
                                });

                                clothingSet.Components.Add(new ClothesComponent(11)  // Top
                                {
                                    Drawable = tops.SelectedIndex,
                                    Texture = topsTexture.SelectedIndex,
                                    Palette = 1
                                });

                                var json = JsonConvert.SerializeObject(clothingSet);
                                BaseScript.TriggerServerEvent("core:buyTopClothes", 250, json);
                            }
                        };



                        var legs = new NativeMenu("Bas", "Bas")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(legs);
                        menu.AddSubMenu(legs);

                        var legsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 4)).ToList();
                        NativeListItem<int> legsItem = new NativeListItem<int>("~h~Bas~s~", legsList.ToArray());
                        legs.Add(legsItem);

                        var legsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 4, legsItem.SelectedIndex)).ToList();
                        NativeListItem<int> legsTexture = new NativeListItem<int>("Style", legsTextureList.ToArray());
                        legs.Add(legsTexture);

                        legsItem.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, 0, 1);
                            legsTexture.SelectedIndex = 0;
                        };

                        legsTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, legsTexture.SelectedIndex, 1);
                        };

                        var legsSubmit = new NativeItem("Acheter", "", "~g~250$");
                        legs.Add(legsSubmit);

                        legsSubmit.Activated += async (sender, e) =>
                        {
                            if (PlayerMenu.PlayerInst.Money >= 250)
                            {
                                var textInput = await Format.GetUserInput("Donnez un nom", "Habit", 12);

                                var clothingSet = new ClothingSet
                                {
                                    Name = textInput
                                };

                                clothingSet.Components.Add(new ClothesComponent(4)  // Legs
                                {
                                    Drawable = legsItem.SelectedIndex,
                                    Texture = legsTexture.SelectedIndex,
                                    Palette = 1
                                });

                                var json = JsonConvert.SerializeObject(clothingSet);
                                BaseScript.TriggerServerEvent("core:buyTopClothes", 250, json);
                            }
                        };

                        var shoes = new NativeMenu("Chaussures", "Chaussures")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(shoes);
                        menu.AddSubMenu(shoes);

                        var shoesList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 6)).ToList();
                        NativeListItem<int> shoesItem = new NativeListItem<int>("~h~Chaussures~s~", shoesList.ToArray());
                        shoes.Add(shoesItem);

                        var shoesTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 6, shoesItem.SelectedIndex)).ToList();
                        NativeListItem<int> shoesTexture = new NativeListItem<int>("Style", shoesTextureList.ToArray());
                        shoes.Add(shoesTexture);

                        shoesItem.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, 0, 1);
                            shoesTexture.SelectedIndex = 0;
                        };

                        shoesTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, shoesTexture.SelectedIndex, 1);
                        };

                        var shoesSubmit = new NativeItem("Acheter", "", "~g~250$");
                        shoes.Add(shoesSubmit);

                        shoesItem.Activated += async (sender, e) =>
                        {
                            if (PlayerMenu.PlayerInst.Money >= 250)
                            {
                                var textInput = await Format.GetUserInput("Donnez un nom", "Habit", 12);

                                var clothingSet = new ClothingSet
                                {
                                    Name = textInput
                                };

                                clothingSet.Components.Add(new ClothesComponent(6)  // Shoes
                                {
                                    Drawable = shoesItem.SelectedIndex,
                                    Texture = shoesTexture.SelectedIndex,
                                    Palette = 1
                                });

                                var json = JsonConvert.SerializeObject(clothingSet);
                                BaseScript.TriggerServerEvent("core:buyTopClothes", 250, json);
                            }
                        };

                        var glasses = new NativeMenu("Lunettes", "Lunettes")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(glasses);
                        menu.AddSubMenu(glasses);

                        var glassesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 1)).ToList();
                        NativeListItem<int> itemsGlasses = new NativeListItem<int>("\"~h~Chapeaux~s~", glassesList.ToArray());
                        glasses.Add(itemsGlasses);

                        var glassesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex)).ToList();
                        NativeListItem<int> itemGlassesTexture = new NativeListItem<int>($"Style", glassesTextureList.ToArray());
                        glasses.Add(itemGlassesTexture);

                        itemsGlasses.ItemChanged += (sender, e) =>
                        {
                            SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, 0, false);
                            itemGlassesTexture.SelectedIndex = 0;
                        };

                        itemGlassesTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, itemGlassesTexture.SelectedIndex, false);
                        };

                        var glassesSubmit = new NativeItem("Acheter", "", "~g~250$");
                        glasses.Add(glassesSubmit);

                        glassesSubmit.Activated += async (sender, e) =>
                        {
                            if (PlayerMenu.PlayerInst.Money >= 250)
                            {
                                var textInput = await Format.GetUserInput("Donnez un nom", "Lunette", 12);

                                var clothingSet = new ClothingSet
                                {
                                    Name = textInput
                                };

                                clothingSet.Components.Add(new ClothesComponent(1)  // Glasses
                                {
                                    Drawable = itemsGlasses.SelectedIndex,
                                    Texture = itemGlassesTexture.SelectedIndex,
                                    Palette = 0
                                });

                                var json = JsonConvert.SerializeObject(clothingSet);
                                BaseScript.TriggerServerEvent("core:buyTopClothes", 250, json);
                            }
                        };

                        var watches = new NativeMenu("Montres", "Montres")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(watches);
                        menu.AddSubMenu(watches);

                        var watchesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 6)).ToList();
                        NativeListItem<int> itemsWatches = new NativeListItem<int>("\"~h~Chapeaux~s~", watchesList.ToArray());
                        watches.Add(itemsWatches);

                        var watchesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex)).ToList();
                        NativeListItem<int> itemWatchesTexture = new NativeListItem<int>($"Style", watchesTextureList.ToArray());
                        watches.Add(itemWatchesTexture);

                        itemsWatches.ItemChanged += (sender, e) =>
                        {
                            SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, 0, false);
                            itemWatchesTexture.SelectedIndex = 0;
                        };

                        itemWatchesTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, itemWatchesTexture.SelectedIndex, false);
                        };

                        var watchSubmit = new NativeItem("Acheter", "", "~g~250$");
                        watches.Add(watchSubmit);

                        watchSubmit.Activated += async (sender, e) =>
                        {
                            if (PlayerMenu.PlayerInst.Money >= 250)
                            {
                                var textInput = await Format.GetUserInput("Donnez un nom", "Montre", 12);

                                var clothingSet = new ClothingSet
                                {
                                    Name = textInput
                                };

                                clothingSet.Components.Add(new ClothesComponent(6)  // Watches
                                {
                                    Drawable = itemsWatches.SelectedIndex,
                                    Texture = itemWatchesTexture.SelectedIndex,
                                    Palette = 0
                                });

                                var json = JsonConvert.SerializeObject(clothingSet);
                                BaseScript.TriggerServerEvent("core:buyTopClothes", 250, json);
                            }
                        };

                        var masks = new NativeMenu("Masques", "Masques")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(masks);
                        menu.AddSubMenu(masks);

                        var masksList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 1)).ToList();
                        NativeListItem<int> masksItem = new NativeListItem<int>("~h~Masque~s~", masksList.ToArray());
                        masks.Add(masksItem);

                        var masksTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 1, masksItem.SelectedIndex)).ToList();
                        NativeListItem<int> masksTexture = new NativeListItem<int>("Style", masksTextureList.ToArray());
                        masks.Add(masksTexture);

                        masksItem.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 1, masksItem.SelectedIndex, 0, 1);
                            masksTexture.SelectedIndex = 0;
                        };

                        masksTexture.ItemChanged += (sender, e) =>
                        {
                            SetPedComponentVariation(GetPlayerPed(-1), 1, masksItem.SelectedIndex, masksTexture.SelectedIndex, 1);
                        };

                        var masksSubmit = new NativeItem("Acheter", "", "~g~250$");
                        masks.Add(masksSubmit);

                        masksItem.Activated += async (sender, e) =>
                        {
                            if (PlayerMenu.PlayerInst.Money >= 250)
                            {
                                var textInput = await Format.GetUserInput("Donnez un nom", "Habit", 12);

                                var clothingSet = new ClothingSet
                                {
                                    Name = textInput
                                };

                                clothingSet.Components.Add(new ClothesComponent(1)  // masks
                                {
                                    Drawable = masksItem.SelectedIndex,
                                    Texture = masksTexture.SelectedIndex,
                                    Palette = 1
                                });

                                var json = JsonConvert.SerializeObject(clothingSet);
                                BaseScript.TriggerServerEvent("core:buyTopClothes", 250, json);
                            }
                        };
                    }
                }
            }
        }

        public void ClothMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);

            foreach (var cloth in ClothShopList)
            {
                var distance = playerCoords.DistanceToSquared(cloth.Checkout);
                if (distance < 8)
                {
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                    if (IsControlPressed(0, 38))
                    {
                        var menu = new NativeMenu("Magasin de vêtements", $"Magasin - {cloth.ShopName}")
                        {
                            UseMouse = false,
                            HeldTime = 100
                        };
                        Pool.Add(menu);
                        menu.Visible = true;
                        var componentDict = new Dictionary<int, string>()
                        {
                            { 8, "Sous haut" },
                            { 11, "Haut" },
                            { 3, "Bras" },
                            { 4, "Bas" },
                            { 6, "Chaussures" },
                            { 7, "Accessoires" }
                        };

                        foreach (var component in componentDict)
                        {
                            var subMenu = new NativeMenu($"{component.Value}", $"{component.Value}")
                            {
                                UseMouse = false
                            };
                            Pool.Add(subMenu);
                            menu.AddSubMenu(subMenu);

                            var itemDrawableList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), component.Key)).ToList();
                            NativeListItem<int> itemDrawable = new NativeListItem<int>(component.Value, itemDrawableList.ToArray());
                            subMenu.Add(itemDrawable);

                            var itemTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), component.Key, itemDrawable.SelectedIndex)).ToList();
                            NativeListItem<int> itemTexture = new NativeListItem<int>($"~h~Texture~s~ {component.Value}", itemTextureList.ToArray());
                            subMenu.Add(itemTexture);

                            itemDrawable.ItemChanged += (sender, e) =>
                            {
                                SetPedComponentVariation(GetPlayerPed(-1), component.Key, itemDrawable.SelectedIndex, 0, 1);
                                itemTexture.SelectedIndex = 0;
                            };

                            itemTexture.ItemChanged += (sender, e) =>
                            {
                                SetPedComponentVariation(GetPlayerPed(-1), component.Key, itemDrawable.SelectedIndex, itemTexture.SelectedIndex, 1);
                            };
                            var submit = new NativeItem("Acheter (~g~$250~s~)");
                            subMenu.Add(submit);

                            submit.Activated += async (sender, e) =>
                            {
                                if (PlayerMenu.PlayerInst.Money >= 250)
                                {
                                    var textInput = await Format.GetUserInput("Donnez un nom", "Habit", 12);
                                    foreach (var clothes in componentDict)
                                    {
                                        var itemDrawableF = subMenu.Items.OfType<NativeListItem<int>>().FirstOrDefault(item => item.Title == clothes.Value);
                                        var itemTextureF = subMenu.Items.OfType<NativeListItem<int>>().FirstOrDefault(item => item.Title == $"~h~Texture~s~ {clothes.Value}");

                                        if (itemDrawableF != null && itemTextureF != null)
                                        {
                                            int selectedDrawableIndex = itemDrawableF.SelectedIndex;
                                            int selectedTextureIndex = itemTextureF.SelectedIndex;
                                        }
                                    }

                                    BaseScript.TriggerServerEvent("core:buyClothes", 250, textInput, component.Key, itemDrawable.SelectedIndex, itemTexture.SelectedIndex, 1);
                                    BaseScript.TriggerServerEvent("core:requestPlayerData");
                                    subMenu.Visible = false;
                                }
                                else
                                {
                                    Format.SendNotif("~r~La misère est si belle...\n ~w~Vous n'avez pas assez d'argent.");
                                }

                            };

                        }

                        

                        menu.Closed += (sender, e) =>
                        {
                            BaseScript.TriggerServerEvent("core:setClothes");
                        };
                    }
                }
            }
        }

        public void OnTick()
        {
            NewClothMenu();
            // ClothMenu();
        }
    }
}
