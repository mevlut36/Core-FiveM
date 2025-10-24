using CitizenFX.Core;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using Core.Shared;

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
        }

        PlayerInstance PlayerRegister = new PlayerInstance
        {
            Skin = "",
            Firstname = "",
            Lastname = "",
            Rank = "",
            Birth = "",
            Clothes = new List<ClothingSet>(),
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
                MouseBehavior = MenuMouseBehavior.Disabled,
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
                    PlayerRegister.Inventory = new List<InventoryItem>();
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
                MouseBehavior = MenuMouseBehavior.Disabled,
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
                SetPedHeadOverlay(player, 2, Skin.Eyebrow, 10 * 0.1f);
                SetPedHeadOverlayColor(player, 1, 1, 1, 1);
            };

            eyebrowColorItem.ItemChanged += (sender, e) =>
            {
                Skin.EyebrowOpacity = eyebrowColorItem.SelectedItem;
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

        void UpdateClothingComponent(ClothingSet set, int componentId, int drawable, int texture, int palette)
        {
            var component = set.Components.FirstOrDefault(c => c.ComponentId == componentId);

            if (component == null)
            {
                component = new ClothesComponent(componentId);
                set.Components.Add(component);
            }

            component.Drawable = drawable;
            component.Texture = texture;
            component.Palette = palette;
        }

        public void DressMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), true);
            var clothesList = new List<ClothingSet>();
            var menu = new NativeMenu("Magasin de vêtements", $"Choisissez vos vêtements")
            {
                MouseBehavior = MenuMouseBehavior.Disabled,
                HeldTime = 100
            };
            Pool.Add(menu);
            menu.Visible = true;

            var clothingSetHat = new ClothingSet
            {
                Name = "Chapeau"
            };

            var hatsList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 0)).ToList();
            NativeListItem<int> itemsHat = new NativeListItem<int>("~h~Chapeaux~s~", hatsList.ToArray());
            menu.Add(itemsHat);

            var hatsTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 0, itemsHat.SelectedIndex)).ToList();
            NativeListItem<int> itemHatTexture = new NativeListItem<int>($"Style", hatsTextureList.ToArray());
            menu.Add(itemHatTexture);

            itemsHat.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, 0, false);
                itemHatTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetHat, 0, itemsHat.SelectedIndex, itemHatTexture.SelectedIndex, 0);
            };
            
            itemHatTexture.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 0, itemsHat.SelectedIndex, itemHatTexture.SelectedIndex, false);
                UpdateClothingComponent(clothingSetHat, 0, itemsHat.SelectedIndex, itemHatTexture.SelectedIndex, 0);
            };

            clothingSetHat.Components.Add(new ClothesComponent(0)  // Hat
            {
                Drawable = itemsHat.SelectedIndex,
                Texture = itemHatTexture.SelectedIndex,
                Palette = 0
            });

            // UNDERSHIRT
            var undershirtList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 8)).ToList();
            NativeListItem<int> undershirts = new NativeListItem<int>("~h~Sous haut~s~", undershirtList.ToArray());
            menu.Add(undershirts);

            var undershirtsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 8, undershirts.SelectedIndex)).ToList();
            NativeListItem<int> undershirtsTexture = new NativeListItem<int>("Style", undershirtsTextureList.ToArray());
            menu.Add(undershirtsTexture);

            var clothingSetTop = new ClothingSet
            {
                Name = "Haut"
            };

            undershirts.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, 0, 1);
                undershirtsTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetTop, 8, undershirts.SelectedIndex, undershirtsTexture.SelectedIndex, 1);
            };

            undershirtsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 8, undershirts.SelectedIndex, undershirtsTexture.SelectedIndex, 1);
                UpdateClothingComponent(clothingSetTop, 8, undershirts.SelectedIndex, undershirtsTexture.SelectedIndex, 1);
            };

            // ARMS
            var armsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 3)).ToList();
            NativeListItem<int> arms = new NativeListItem<int>("~h~Bras~s~", armsList.ToArray());
            menu.Add(arms);

            var armsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 3, arms.SelectedIndex)).ToList();
            NativeListItem<int> armsTexture = new NativeListItem<int>("Style", armsTextureList.ToArray());
            menu.Add(armsTexture);

            arms.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, 0, 1);
                armsTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetTop, 3, arms.SelectedIndex, armsTexture.SelectedIndex, 1);
            };

            armsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 3, arms.SelectedIndex, armsTexture.SelectedIndex, 1);
                UpdateClothingComponent(clothingSetTop, 3, arms.SelectedIndex, armsTexture.SelectedIndex, 1);
            };

            // TOP
            var topsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 11)).ToList();
            NativeListItem<int> tops = new NativeListItem<int>("~h~Haut~s~", topsList.ToArray());
            menu.Add(tops);

            var topsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 11, tops.SelectedIndex)).ToList();
            NativeListItem<int> topsTexture = new NativeListItem<int>("Style", topsTextureList.ToArray());
            menu.Add(topsTexture);

            tops.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, 0, 1);
                topsTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetTop, 11, tops.SelectedIndex, topsTexture.SelectedIndex, 1);
            };

            topsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 11, tops.SelectedIndex, topsTexture.SelectedIndex, 1);
                UpdateClothingComponent(clothingSetTop, 11, tops.SelectedIndex, topsTexture.SelectedIndex, 1);
            };

            clothingSetTop.Components.Add(new ClothesComponent(8)  // Undershirt
            {
                Drawable = undershirts.SelectedIndex,
                Texture = undershirtsTexture.SelectedIndex,
                Palette = 1
            });

            clothingSetTop.Components.Add(new ClothesComponent(3)  // Torso
            {
                Drawable = arms.SelectedIndex,
                Texture = armsTexture.SelectedIndex,
                Palette = 1
            });

            clothingSetTop.Components.Add(new ClothesComponent(11)  // Top
            {
                Drawable = tops.SelectedIndex,
                Texture = topsTexture.SelectedIndex,
                Palette = 1
            });


            var legsList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 4)).ToList();
            NativeListItem<int> legsItem = new NativeListItem<int>("~h~Bas~s~", legsList.ToArray());
            menu.Add(legsItem);

            var legsTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 4, legsItem.SelectedIndex)).ToList();
            NativeListItem<int> legsTexture = new NativeListItem<int>("Style", legsTextureList.ToArray());
            menu.Add(legsTexture);

            var clothingSetLegs = new ClothingSet
            {
                Name = "Bas"
            };

            legsItem.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, 0, 1);
                legsTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetLegs, 4, legsItem.SelectedIndex, legsTexture.SelectedIndex, 1);
            };

            legsTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 4, legsItem.SelectedIndex, legsTexture.SelectedIndex, 1);
                UpdateClothingComponent(clothingSetLegs, 4, legsItem.SelectedIndex, legsTexture.SelectedIndex, 1);
            };

            clothingSetLegs.Components.Add(new ClothesComponent(4)  // Legs
            {
                Drawable = legsItem.SelectedIndex,
                Texture = legsTexture.SelectedIndex,
                Palette = 1
            });

            var shoesList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), 6)).ToList();
            NativeListItem<int> shoesItem = new NativeListItem<int>("~h~Chaussures~s~", shoesList.ToArray());
            menu.Add(shoesItem);

            var shoesTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), 6, shoesItem.SelectedIndex)).ToList();
            NativeListItem<int> shoesTexture = new NativeListItem<int>("Style", shoesTextureList.ToArray());
            menu.Add(shoesTexture);

            var clothingSetShoes = new ClothingSet
            {
                Name = "Chaussures"
            };

            shoesItem.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, 0, 1);
                shoesTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetShoes, 6, shoesItem.SelectedIndex, shoesTexture.SelectedIndex, 1);
            };

            shoesTexture.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(GetPlayerPed(-1), 6, shoesItem.SelectedIndex, shoesTexture.SelectedIndex, 1);
                UpdateClothingComponent(clothingSetShoes, 6, shoesItem.SelectedIndex, shoesTexture.SelectedIndex, 1);
            };

            clothingSetShoes.Components.Add(new ClothesComponent(6)  // Shoes
            {
                Drawable = shoesItem.SelectedIndex,
                Texture = shoesTexture.SelectedIndex,
                Palette = 1
            }); 


            var glassesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 1)).ToList();
            NativeListItem<int> itemsGlasses = new NativeListItem<int>("~h~Lunette~s~", glassesList.ToArray());
            menu.Add(itemsGlasses);

            var glassesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex)).ToList();
            NativeListItem<int> itemGlassesTexture = new NativeListItem<int>($"Style", glassesTextureList.ToArray());
            menu.Add(itemGlassesTexture);

            var clothingSetGlasses = new ClothingSet
            {
                Name = "Lunette"
            };

            itemsGlasses.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, 0, false);
                itemGlassesTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetGlasses, 1, itemsGlasses.SelectedIndex, itemGlassesTexture.SelectedIndex, 0);
            };

            itemGlassesTexture.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 1, itemsGlasses.SelectedIndex, itemGlassesTexture.SelectedIndex, false);
                UpdateClothingComponent(clothingSetGlasses, 1, itemsGlasses.SelectedIndex, itemGlassesTexture.SelectedIndex, 0);
            };

            clothingSetGlasses.Components.Add(new ClothesComponent(1)  // Glasses
            {
                Drawable = itemsGlasses.SelectedIndex,
                Texture = itemGlassesTexture.SelectedIndex,
                Palette = 0
            });

            var watchesList = Enumerable.Range(0, GetNumberOfPedPropDrawableVariations(GetPlayerPed(-1), 6)).ToList();
            NativeListItem<int> itemsWatches = new NativeListItem<int>("~h~Montre~s~", watchesList.ToArray());
            menu.Add(itemsWatches);

            var watchesTextureList = Enumerable.Range(0, GetNumberOfPedPropTextureVariations(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex)).ToList();
            NativeListItem<int> itemWatchesTexture = new NativeListItem<int>($"Style", watchesTextureList.ToArray());
            menu.Add(itemWatchesTexture);

            var clothingSetWatches = new ClothingSet
            {
                Name = "Montre"
            };

            itemsWatches.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, 0, false);
                itemWatchesTexture.SelectedIndex = 0;
                UpdateClothingComponent(clothingSetWatches, 6, itemsWatches.SelectedIndex, itemWatchesTexture.SelectedIndex, 0);
            };

            itemWatchesTexture.ItemChanged += (sender, e) =>
            {
                SetPedPropIndex(GetPlayerPed(-1), 6, itemsWatches.SelectedIndex, itemWatchesTexture.SelectedIndex, false);
                UpdateClothingComponent(clothingSetWatches, 6, itemsWatches.SelectedIndex, itemWatchesTexture.SelectedIndex, 0);
            };

            clothingSetWatches.Components.Add(new ClothesComponent(6)  // Watches
            {
                Drawable = itemsWatches.SelectedIndex,
                Texture = itemWatchesTexture.SelectedIndex,
                Palette = 0
            });
            
            var submit = new NativeItem("Envoyer");
            menu.Add(submit);

            submit.Activated += (sender, e) =>
            {
                clothesList.Add(clothingSetHat);
                clothesList.Add(clothingSetTop);
                clothesList.Add(clothingSetLegs);
                clothesList.Add(clothingSetShoes);
                clothesList.Add(clothingSetGlasses);
                clothesList.Add(clothingSetWatches);

                PlayerRegister.ClothesList = JsonConvert.SerializeObject(clothesList);
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
