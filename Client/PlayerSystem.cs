using CitizenFX.Core;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace Core.Client
{
    public class PlayerSystem : BaseScript
    {
        ClientMain Client;
        ObjectPool Pool;
        Format Format;

        SkinInfo Skin = new SkinInfo();

        public PlayerSystem(ClientMain caller)
        {
            Client = caller;
            Pool = caller.Pool;
            Format = caller.Format;

            Client.AddEvent("core:createCharacter", new Action(CreateCharacter));
            Client.AddEvent("core:getSkin", new Action<string>(GetGender));
            Client.AddEvent("core:getClothes", new Action<string, string>(GetClothes));
            Client.AddEvent("core:teleportLastPosition", new Action<string>(TeleportLastPosition));
        }

        PlayerInstance PlayerRegister = new PlayerInstance
        {
            Skin = "",
            Firstname = "",
            Lastname = "",
            Rank = "",
            Birth = "",
            Clothes = "",
            ClothesList = ""
        };

        public void CreateCharacter()
        {
            Game.PlayerPed.Position = new Vector3(151.1f, -751.4f, 258.1f);
            CharacterMenu();
        }
        public async void CharacterMenu()
        {
            var model = new Model("mp_m_freemode_01");
            RequestModel(model);
            while (!model.IsLoaded)
            {
                await Delay(0);
            }

            if (IsModelInCdimage(model) && IsModelValid(model))
            {
                SetPlayerModel(PlayerId(), model);
                SetPedDefaultComponentVariation(GetPlayerPed(-1));
            }
            var menu = new NativeMenu("Création")
            {
                Visible = true,
                UseMouse = false,
                AcceptsInput = true
            };

            var genderItem = new NativeListItem<string>("Genre", "", "Homme", "Femme");
            var firstnameItem = new NativeItem("Prénom");
            var lastnameItem = new NativeItem("Nom");
            var birthItem = new NativeItem("Date de naissance");
            var submit = new NativeItem("Envoyer");

            menu.Add(genderItem);
            menu.Add(firstnameItem);
            menu.Add(lastnameItem);
            menu.Add(birthItem);
            menu.Add(submit);

            string gender = "Homme";
            string firstname = "";
            string lastname = "";
            string birth = "";

            genderItem.ItemChanged += async (sender, e) =>
            {
                gender = genderItem.SelectedItem;
                if (genderItem.SelectedItem == "Femme")
                {
                    var modelMale = new Model(PedHash.FreemodeFemale01);
                    RequestModel(modelMale);
                    while (!modelMale.IsLoaded)
                    {
                        await Delay(0);
                    }
                    if (IsModelInCdimage(modelMale) && IsModelValid(modelMale))
                    {
                        SetPlayerModel(PlayerId(), modelMale);
                        SetPedDefaultComponentVariation(GetPlayerPed(-1));
                    }
                }
                else
                {
                    var modelFemale = new Model(PedHash.FreemodeMale01);
                    RequestModel(modelFemale);
                    while (!modelFemale.IsLoaded)
                    {
                        await Delay(0);
                    }
                    if (IsModelInCdimage(modelFemale) && IsModelValid(modelFemale))
                    {
                        SetPlayerModel(PlayerId(), modelFemale);
                        SetPedDefaultComponentVariation(GetPlayerPed(-1));
                    }
                }
            };

            firstnameItem.Activated += async (sender, e) =>
            {
                var textInput = await Format.GetUserInput("Prénom", "", 20);
                firstnameItem.AltTitle = textInput;
                firstname = textInput;
            };

            lastnameItem.Activated += async (sender, e) =>
            {
                var textInput = await Format.GetUserInput("Nom", "", 20);
                lastnameItem.AltTitle = textInput;
                lastname = textInput;
            };

            birthItem.Activated += async (sender, e) =>
            {
                var textInput = await Format.GetUserInput("Date de naissance", "20/04/1990", 20);
                if (Format.IsValidDateFormat(textInput))
                {
                    birth = textInput;
                    birthItem.AltTitle = birth;
                }
            };

            submit.Activated += (sender, e) =>
            {
                if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(birth))
                {
                    Format.CustomNotif("Vous devez renseigner toutes les informations");
                }
                else
                {
                    menu.Visible = false;
                    Pool.Remove(menu);
                    Format.CustomNotif("Vos informations ont bien été enregistré");

                    Skin.Gender = gender;
                    PlayerRegister.Firstname = firstname;
                    PlayerRegister.Lastname = lastname;
                    PlayerRegister.Birth = birth;
                    PlayerRegister.Inventory = "[]";
                    PlayerRegister.Bills = "[]";
                    SkintonMenu();
                }
            };

            menu.Closing += (sender, e) =>
            {
                if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(birth))
                {
                    e.Cancel = true;
                    Format.CustomNotif("Vous devez renseigner toutes les informations");
                }
            };

            Pool.Add(menu);
        }
        private void SkintonMenu()
        {
            var ped = Game.PlayerPed.Handle;
            var player = GetPlayerPed(-1);

            List<string> fathers = Format.parentFace.Where(x => x.Substring(2, 1) == "M").ToList();
            List<string> mothers = Format.parentFace.Where(x => x.Substring(2, 1) == "F").ToList();

            var menu = new NativeMenu("Skin", "Skin maker")
            {
                Visible = true,
                UseMouse = false,
                HeldTime = 100
            };
            Pool.Add(menu);

            var hairItem = new NativeListItem<int>("Cheveux", 0);
            for (int i = 1; i <= GetNumHairColors(); i++)
            {
                hairItem.Items.Add(i);
            }
            var hairColorItem = new NativeListItem<int>("Couleur", 0);
            for (int i = 1; i <= GetNumHairColors(); i++)
            {
                hairColorItem.Items.Add(i);
            }
            var dadItem = new NativeListItem<int>("Père", "", 0, 1);
            for (int i = 0; i <= fathers.Count; i++)
            {
                dadItem.Items.Add(i);
            }
            var momItem = new NativeListItem<int>("Mère", "", 0, 1);
            for (int i = 0; i <= mothers.Count; i++)
            {
                momItem.Items.Add(i);
            }

            var eyeColorItem = new NativeListItem<int>("Couleur des yeux", "", 0);
            for (int i = 1; i <= 31; i++)
            {
                eyeColorItem.Items.Add(i);
            }

            var beardItem = new NativeListItem<int>("Barbe", "", 0);
            for (int i = 1; i <= 31; i++)
            {
                beardItem.Items.Add(i);
            }

            var beardColorItem = new NativeListItem<int>("Couleur de barbe", "", 0);
            for (int i = 1; i <= GetNumHairColors(); i++)
            {
                beardColorItem.Items.Add(i);
            }
            var eyebrowItem = new NativeListItem<int>("Sourcils", "", 0);
            for (int i = 1; i <= 33; i++)
            {
                eyebrowItem.Items.Add(i);
            }

            var eyebrowColorItem = new NativeListItem<int>("Sourcils", "", 0);
            for (int i = 1; i <= 33; i++)
            {
                eyebrowColorItem.Items.Add(i);
            }

            menu.Add(hairItem);
            menu.Add(hairColorItem);
            menu.Add(dadItem);
            menu.Add(momItem);
            menu.Add(eyeColorItem);
            menu.Add(beardItem);
            menu.Add(beardColorItem);
            menu.Add(eyebrowItem);
            menu.Add(eyebrowColorItem);

            hairItem.ItemChanged += (sender, e) =>
            {
                Skin.Hair = hairItem.SelectedIndex;
                SetPedComponentVariation(ped, 2, hairItem.SelectedIndex, hairColorItem.SelectedIndex, 0);
                SetPedHairColor(ped, hairColorItem.SelectedIndex, 1);
            };

            hairColorItem.ItemChanged += (sender, e) =>
            {
                Skin.HairColor = hairColorItem.SelectedIndex;
                SetPedComponentVariation(ped, 2, hairItem.SelectedIndex, hairColorItem.SelectedIndex, 0);
                SetPedHairColor(ped, Skin.HairColor, Skin.HairColor);
            };

            dadItem.ItemChanged += (sender, e) =>
            {
                Skin.Dad = dadItem.SelectedItem;
                SetPedHeadBlendData(player, Skin.Mom, Skin.Dad, Skin.Dad, Skin.Mom, Skin.Dad, Skin.Dad, Skin.DadMomPercent * 0.1f, Skin.DadMomPercent * 0.1f, 1.0f, false);
            };

            momItem.ItemChanged += (sender, e) =>
            {
                Skin.Mom = 20 + momItem.SelectedItem;
                SetPedHeadBlendData(player, Skin.Mom, Skin.Dad, Skin.Mom, Skin.Mom, Skin.Dad, Skin.Mom, Skin.DadMomPercent * 0.1f, Skin.DadMomPercent * 0.1f, 1.0f, false);
            };

            eyeColorItem.ItemChanged += (sender, e) =>
            {
                Skin.EyeColor = eyeColorItem.SelectedItem;
                SetPedEyeColor(player, Skin.EyeColor);
            };

            beardItem.ItemChanged += (sender, e) =>
            {
                Skin.Beard = beardItem.SelectedItem;
                Skin.BeardOpacity = 100;
                SetPedHeadOverlay(player, 1, Skin.Beard, Skin.BeardOpacity * 0.1f);
            };

            beardColorItem.ItemChanged += (sender, e) =>
            {
                Skin.BeardColor = beardColorItem.SelectedItem;
                SetPedHeadOverlayColor(player, 1, 1, Skin.BeardColor, Skin.BeardColor);
            };

            eyebrowItem.ItemChanged += (sender, e) =>
            {
                Skin.Eyebrow = eyebrowItem.SelectedItem;
                Skin.EyebrowOpacity = 10;
                SetPedHeadOverlay(player, 2, Skin.Eyebrow, 10 * 0.1f);
                SetPedHeadOverlayColor(player, 1, 1, 1, 1);
            };

            eyebrowColorItem.ItemChanged += (sender, e) =>
            {
                Skin.BeardColor = eyebrowColorItem.SelectedItem;
                SetPedHeadOverlayColor(player, 2, 1, Skin.EyebrowOpacity, Skin.EyebrowOpacity);
            };

            var submit = new NativeItem("Enregistrer", "");
            menu.Add(submit);

            submit.Activated += (sender, e) =>
            {
                menu.Visible = false;
                Pool.Remove(menu);
                PlayerRegister.Skin = JsonConvert.SerializeObject(Skin);
                DressMenu();
            };

            menu.Closing += (sender, e) =>
            {
                e.Cancel = true;
            };
        }
        public async void GetGender(string json)
        {
            var gender = JsonConvert.DeserializeObject<SkinInfo>(json);

            var pedHash = gender.Gender == "Femme" ? PedHash.FreemodeFemale01 : PedHash.FreemodeMale01;
            var model = new Model(pedHash);
            RequestModel(model);
            while (!model.IsLoaded)
            {
                await Delay(0);
            }
            if (IsModelInCdimage(model) && IsModelValid(model))
            {
                _ = LocalPlayer.ChangeModel(model);
                SetPedDefaultComponentVariation(LocalPlayer.Character.Handle);
                TriggerServerEvent("core:setClothes");
            }
        }
        public void GetClothes(string json, string skinJson)
        {
            var clothesList = JsonConvert.DeserializeObject<List<ClothesInfo>>(json);
            var ped = GetPlayerPed(-1);
            var skin = JsonConvert.DeserializeObject<SkinInfo>(skinJson);
            Skin = skin;
            foreach (var clothes in clothesList)
            {
                if (!string.IsNullOrEmpty(clothes.Name))
                {
                    DressPed(clothes.Component, clothes.Drawable, clothes.Texture);
                }
            }

            SetPedComponentVariation(ped, 2, Skin.Hair, Skin.HairColor, 0);
            SetPedHairColor(ped, Skin.HairColor, 1);
            SetPedHeadBlendData(ped, Skin.Mom, Skin.Dad, Skin.Dad, Skin.Mom, Skin.Dad, Skin.Dad, Skin.DadMomPercent * 0.1f, Skin.DadMomPercent * 0.1f, 1.0f, false);
            SetPedEyeColor(ped, Skin.EyeColor);
            SetPedHeadOverlay(ped, 1, Skin.Beard, Skin.BeardOpacity * 0.1f);
            SetPedHeadOverlayColor(ped, 1, 1, Skin.BeardColor, Skin.BeardColor);
            SetPedHeadOverlay(ped, 2, Skin.Eyebrow, 10 * 0.1f);
            SetPedHeadOverlayColor(ped, 1, 1, 1, 1);
        }
        public void DressPed(int componentId, int drawableId, int textureId)
        {
            SetPedComponentVariation(GetPlayerPed(-1), componentId, drawableId, textureId, 0);
        }
        public void TeleportLastPosition(string json)
        {
            var coords = JsonConvert.DeserializeObject<Vector3>(json);
            Game.PlayerPed.Position = coords;
        }
        public void DressMenu()
        {
            var ped = PlayerPedId();
            var menu = new NativeMenu("Garde robe", "Choisissez vos vêtements");
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;
            menu.HeldTime = 100;
            menu.AcceptsInput = true;
            var componentDict = new Dictionary<int, string>()
            {
                { 8, "Sous haut" },
                { 11, "Haut" },
                { 3, "Bras" },
                { 4, "Bas" },
                { 6, "Chaussures" },
                { 7, "Accessoires" }
            };

            var clothesInfoList = new List<ClothesInfo>();
            var classClothes = new ClothesInfo { Name = "", Component = 0, Drawable = 0, Texture = 0, Palette = 1 };
            var clothesInfoTempList = new List<ClothesInfo>();
            foreach (var clothes in componentDict)
            {
                var clothesInfo = new ClothesInfo();
                var itemDrawableList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), clothes.Key)).ToList();
                NativeListItem<int> itemDrawable = new NativeListItem<int>(clothes.Value, itemDrawableList.ToArray());
                menu.Add(itemDrawable);

                var itemTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), clothes.Key, itemDrawable.SelectedIndex)).ToList();
                NativeListItem<int> itemTexture = new NativeListItem<int>($"~h~Texture~s~ {clothes.Value}", itemTextureList.ToArray());
                menu.Add(itemTexture);

                itemDrawable.ItemChanged += (sender, e) =>
                {
                    SetPedComponentVariation(GetPlayerPed(-1), clothes.Key, itemDrawable.SelectedIndex, 0, 1);
                    itemTexture.SelectedIndex = 0;

                    clothesInfo.Name = clothes.Value;
                    clothesInfo.Component = clothes.Key;
                    clothesInfo.Drawable = itemDrawable.SelectedIndex;
                };

                itemTexture.ItemChanged += (sender, e) =>
                {
                    SetPedComponentVariation(GetPlayerPed(-1), clothes.Key, itemDrawable.SelectedIndex, itemTexture.SelectedIndex, 1);

                    clothesInfo.Texture = itemTexture.SelectedIndex;
                };
                clothesInfoTempList.Add(clothesInfo);
            }

            var submit = new NativeItem("Envoyer");
            menu.Add(submit);

            submit.Activated += (sender, e) =>
            {
                PlayerRegister.ClothesList = JsonConvert.SerializeObject(clothesInfoTempList);
                string json = JsonConvert.SerializeObject(PlayerRegister);
                TriggerServerEvent("core:registerPlayerData", json);
                SetEntityCoords(GetPlayerPed(-1), -283.2f, -939.4f, 31.2f, true, false, false, true);
                menu.Visible = false;
                Pool.Remove(menu);
            };

            menu.Closing += (sender, e) =>
            {
                e.Cancel = true;
            };
        }
    }
}
