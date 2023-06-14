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
    public class ClothShop
    {
        public ClientMain Client;
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;
        public PlayerMenu PlayerMenu;

        List<ClothShopInfo> ClothShopList = new List<ClothShopInfo>();

        public ClothShop(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            ClothShopInfo shop0 = new ClothShopInfo("Cube Place", new Vector3(197, -876.5f, 30.6f), new Vector3(200.4f, -870.9f, 30.6f));
            ClothShopList.Add(shop0);
            ClothShopInfo shop1 = new ClothShopInfo("Strawberry", new Vector3(75.7f, -1394.2f, 29.3f), new Vector3(73.9f, -1393.1f, 29.2f));
            ClothShopList.Add(shop1);
            ClothShopInfo shop2 = new ClothShopInfo("Textile City", new Vector3(424.9f, -804.8f, 29.4f), new Vector3(427.2f, -805.9f, 29.4f));
            ClothShopList.Add(shop2);
            ClothShopInfo shop3 = new ClothShopInfo("Hawick", new Vector3(123, -223.7f, 54.5f), new Vector3(127.1f, -224.5f, 54.4f));
            ClothShopList.Add(shop3);
            ClothShopInfo shop4 = new ClothShopInfo("Burton", new Vector3(-162.1f, -303.2f, 39.7f), new Vector3(-164.9f, -302, 39.6f));
            ClothShopList.Add(shop4);
            ClothShopInfo shop5 = new ClothShopInfo("Rockford Hills", new Vector3(-711, -153.5f, 37.3f), new Vector3(-708.3f, -152.6f, 37.4f));
            ClothShopList.Add(shop5);
            ClothShopInfo shop6 = new ClothShopInfo("Vespucci", new Vector3(-823.1f, -1074.5f, 11.2f), new Vector3(-823.3f, -1072.2f, 11.2f));
            ClothShopList.Add(shop6);
            ClothShopInfo shop7 = new ClothShopInfo("Del Perro", new Vector3(-1191, -769, 17.3f), new Vector3(-1193.8f, -766.5f, 17.2f));
            ClothShopList.Add(shop7);
            ClothShopInfo shop8 = new ClothShopInfo("Chumash", new Vector3(-3173.7f, 1043.5f, 20.7f), new Vector3(-3169.1f, 1042.7f, 20.7f));
            ClothShopList.Add(shop8);
            ClothShopInfo shop9 = new ClothShopInfo("Harmony", new Vector3(616.6f, 2763.9f, 42), new Vector3(612.7f, 2763.4f, 42));
            ClothShopList.Add(shop9);
            ClothShopInfo shop10 = new ClothShopInfo("Grapeseed", new Vector3(1693.1f, 4824.3f, 42), new Vector3(1695.3f, 4823.3f, 42));
            ClothShopList.Add(shop10);
            ClothShopInfo shop11 = new ClothShopInfo("Grand Senora Desert", new Vector3(1195.4f, 2710, 38.1f), new Vector3(1196.4f, 2711.7f, 38.1f));
            ClothShopList.Add(shop11);
            ClothShopInfo shop12 = new ClothShopInfo("Paleto Bay", new Vector3(5, 6513.8f, 31.7f), new Vector3(5.9f, 6511.3f, 31.7f));
            ClothShopList.Add(shop12);

            foreach (var cloth in ClothShopList)
            {
                Blip myBlip = World.CreateBlip(cloth.Checkout);
                myBlip.Sprite = BlipSprite.Clothes;
                myBlip.Name = "Magasin de vêtements";
                myBlip.Color = BlipColor.TrevorOrange;
                myBlip.IsShortRange = true;
            }
        }

        public void ClothMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);

            foreach (var cloth in ClothShopList)
            {
                var distance = GetDistanceBetweenCoords(cloth.Checkout.X, cloth.Checkout.Y, cloth.Checkout.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);

                if (distance < 7)
                {
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                    if (IsControlPressed(0, 38))
                    {
                        var menu = new NativeMenu("Magasin de vêtements", $"Magasin - {cloth.ShopName}")
                        {
                            TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
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
                                TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
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
            ClothMenu();
        }
    }
}
