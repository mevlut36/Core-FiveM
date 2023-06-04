using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using System.Collections.Generic;
using LemonUI.Menus;
using System.Linq;
using LemonUI;
using CitizenFX.Core.Native;
using Mono.CSharp;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Core.Client
{
    public class PlayerInstance
    {
        public string Gender { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Birth { get; set; }
        public string Clothes { get; set; }
    }

    public class ClientMain : BaseScript
    {
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public Vector3 dressPos = new Vector3(151.1f, -751.4f, 258.1f);
        public string Result = "";

        PlayerInstance Player = new PlayerInstance
        {
            Gender = "",
            Firstname = "",
            Lastname = "",
            Birth = "",
            Clothes = ""
        };

        public ClientMain()
        {
            Debug.WriteLine("Hi from Core.Client!");
            EventHandlers["playerSpawned"] += new Action(OnPlayerConnecting);
            EventHandlers["baseevents:onPlayerDied"] += new Action<int, dynamic>(OnDeath);
            Format = new Format(this);
        }

        public void OnPlayerConnecting()
        {
            TriggerServerEvent("core:isPlayerRegistered");
        }

        public void OnDeath(int killerType, dynamic deathCoords)
        {
            var LocalPlayer = GetLocalPlayer();
            Format.SendNotif("Vous êtes mort");
            SetEntityCoordsNoOffset(GetPlayerPed(-1), LocalPlayer.Character.Position.X, LocalPlayer.Character.Position.Y, LocalPlayer.Character.Position.Z, false, false, false);
            NetworkResurrectLocalPlayer(LocalPlayer.Character.Position.X, LocalPlayer.Character.Position.Y, LocalPlayer.Character.Position.Z, LocalPlayer.Character.Heading, true, false);
            SetPlayerInvincible(GetPlayerPed(-1), false);
            DeathAnimation();
        }

        public async void DeathAnimation()
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, "anim@gangops@morgue@table@");
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "anim@gangops@morgue@table@")) await BaseScript.Delay(50);
            Game.PlayerPed.Task.ClearAllImmediately();
            AnimationFlags flags = AnimationFlags.Loop;
            Game.PlayerPed.Task.PlayAnimation("anim@gangops@morgue@table@", "ko_front", -1, 5000, flags);
            ClearPedBloodDamage(GetPlayerPed(-1));
        }

        [EventHandler("core:getGender")]
        public async void GetGender(string gender)
        {
            if (gender == "Femme")
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
                    TriggerServerEvent("core:setClothes");
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
                    TriggerServerEvent("core:setClothes");
                }
            }
        }

        [EventHandler("core:getClothes")]
        public void GetClothes(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            var properties = jsonObject.Properties();

            foreach (var property in properties)
            {
                var valueObject = property.Value as JObject;

                if (valueObject != null)
                {
                    var subProperties = valueObject.Properties();
                    foreach (var subProperty in subProperties)
                    {
                        var subKey = subProperty.Name;
                        var subValue = (int)subProperty.Value;
                        Debug.WriteLine($"{property.Name} {subKey} {subValue}");
                        DressPed(int.Parse(property.Name), int.Parse(subKey), subValue);
                    }
                }
            }
        }

        public void DressPed(int componentId, int drawableId, int textureId)
        {
            SetPedComponentVariation(GetPlayerPed(-1), componentId, drawableId, textureId, 0);
        }

        [EventHandler("core:teleportLastPosition")]
        public void TeleportLastPosition(string json)
        {
            var coords = JsonConvert.DeserializeObject<Vector3>(json);
            SetEntityCoords(GetPlayerPed(-1), coords.X, coords.Y, coords.Z, true, false, false, true);
        }

        [EventHandler("core:sendNotif")]
        public void ServerSendNotif(string text)
        {
            Format.SendNotif(text);
        }

        [EventHandler("core:createCharacter")]
        public void CreateCharacter()
        {
            SetEntityCoords(GetPlayerPed(-1), dressPos.X, dressPos.Y, dressPos.Z, true, false, false, true);
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

            string gender = "";
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
                } else
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
                var textInput = await Format.GetUserInput("Date de naissance", "20/04/1889", 20);
                if (IsValidDateFormat(textInput))
                {
                    birthItem.AltTitle = textInput;
                    birth = textInput;
                }
            };

            submit.Activated += (sender, e) =>
            {
                menu.Visible = false;
                Format.CustomNotif("Vos informations ont bien été enregistré");
                DressMenu();
                Player.Gender = gender;
                Player.Firstname = firstname;
                Player.Lastname = lastname;
                Player.Birth = birth;
            };

            menu.Closing += (sender, e) => {
                if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(birth))
                {
                    e.Cancel = true;
                    Format.CustomNotif("Vous devez renseigner toutes les informations");
                }
            };

            Pool.Add(menu);
        }

        private bool IsValidDateFormat(string date)
        {
            var regex = new Regex(@"^\d{2}/\d{2}/\d{4}$");
            return regex.IsMatch(date);
        }


        [EventHandler("core:createSkin")]
        public void CreateSkin()
        {
            DressMenu();
        }

        private void ChangeHair()
        {
            var menu = new NativeMenu("Coiffures")
            {
                Visible = true,
                UseMouse = false,
                HeldTime = 100
            };

            var hairItem = new NativeListItem<int>("Cheveux", 0);
            
            for (int i = 0; i <= GetNumHairColors(); i++)
            {
                hairItem.Items.Add(i);
            }
            var colorItem = new NativeListItem<int>("Couleur", 0);
            for (int i = 1; i <= GetNumHairColors(); i++)
            {
                colorItem.Items.Add(i);
            }

            menu.Add(hairItem);
            menu.Add(colorItem);

            Pool.Add(menu);

            hairItem.ItemChanged += (sender, e) =>
            {
                ChangeHair(hairItem.SelectedIndex, colorItem.SelectedIndex);
            };

            colorItem.ItemChanged += (sender, e) =>
            {
                ChangeHair(hairItem.SelectedIndex, colorItem.SelectedIndex);
            };
        }

        private void ChangeHair(int hair, int color)
        {
            var ped = Game.PlayerPed.Handle;

            SetPedComponentVariation(ped, 2, hair, color, 0);
            SetPedHairColor(ped, color, 1);
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

            foreach (var clothes in componentDict)
            {
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
                };

                itemTexture.ItemChanged += (sender, e) =>
                {
                    SetPedComponentVariation(GetPlayerPed(-1), clothes.Key, itemDrawable.SelectedIndex, itemTexture.SelectedIndex, 1);
                };
            }

            menu.Closing += (sender, e) =>
            {
                var clothesDict = new Dictionary<int, Dictionary<int, int>>();
                foreach (var clothes in componentDict)
                {
                    var itemDrawable = menu.Items.OfType<NativeListItem<int>>().FirstOrDefault(item => item.Title == clothes.Value);
                    var itemTexture = menu.Items.OfType<NativeListItem<int>>().FirstOrDefault(item => item.Title == $"~h~Texture~s~ {clothes.Value}");

                    if (itemDrawable != null && itemTexture != null)
                    {
                        int selectedDrawableIndex = itemDrawable.SelectedIndex;
                        int selectedTextureIndex = itemTexture.SelectedIndex;

                        clothesDict.Add(clothes.Key, new Dictionary<int, int>
                        {
                            { selectedDrawableIndex, selectedTextureIndex }
                        });
                    }
                }

                string jsonClothes = JsonConvert.SerializeObject(clothesDict);
                Player.Clothes = jsonClothes;

                string json = JsonConvert.SerializeObject(Player);
                TriggerServerEvent("core:sendPlayerData", json);
                SetEntityCoords(GetPlayerPed(-1), -283.2f, -939.4f, 31.2f, true, false, false, true);
            };
        }

        [Tick]
        public Task OnTick()
        {
            Pool.Process();
            
            if (IsControlJustPressed(0, 167))
            {
                ChangeHair();
            }
            return Task.FromResult(0);
        }

        public Player GetLocalPlayer() => this.LocalPlayer;
        public bool IsPedMale() => IsPedModel(PlayerPedId(), (uint)PedHash.FreemodeMale01);
    }
}
