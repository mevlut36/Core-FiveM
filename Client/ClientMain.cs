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

namespace Core.Client
{
    public class PlayerInstance
    {
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
            Firstname = "",
            Lastname = "",
            Birth = "",
            Clothes = ""
        };

        public ClientMain()
        {
            Debug.WriteLine("Hi from Core.Client!");
            EventHandlers["playerSpawned"] += new Action(OnPlayerConnecting);
            Format = new Format(this);
        }

        public void OnPlayerConnecting()
        {
            TriggerServerEvent("core:isPlayerRegistered");
        }

        [EventHandler("core:getClothes")]
        public async void GetClothes(string json)
        {
            // json : {"8":{"4":0},"11":{"4":0},"3":{"1":0},"4":{"4":0},"6":{"3":0},"7":{"0":0}}
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

            await Game.Player.ChangeModel(model);
            model.Request();
            API.SetPedComponentVariation(GetPlayerPed(-1), 1, 1, 1, 0);
            Debug.WriteLine($"{json}");

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
                        DressPed(int.Parse(property.Name), int.Parse(subKey), subValue);
                    }
                }
            }
            TriggerServerEvent("core:getLastPosition");
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
            Delay(8000);
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

            var firstnameItem = new NativeItem("Prénom");
            var lastnameItem = new NativeItem("Nom");
            var birthItem = new NativeItem("Date de naissance");
            var submit = new NativeItem("Envoyer");

            menu.Add(firstnameItem);
            menu.Add(lastnameItem);
            menu.Add(birthItem);
            menu.Add(submit);

            string firstname = "";
            string lastname = "";
            string birth = "";

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
                birthItem.AltTitle = textInput;
                birth = textInput;
            };

            submit.Activated += (sender, e) =>
            {
                menu.Visible = false;
                Format.CustomNotif("Vos informations ont bien été enregistré");
                DressMenu();
                Player.Firstname = firstname;
                Player.Lastname = lastname;
                Player.Birth = birth;
            };

            menu.Closing += (sender, e) =>
            {
                menu.Visible = true;
            };

            Pool.Add(menu);
        }


        [EventHandler("core:createSkin")]
        public void CreateSkin()
        {
            DressMenu();
        }

        public void DressMenu()
        {
            var model = new Model(PedHash.FreemodeMale01);
            model.Request();
            Game.Player.ChangeModel(model);
            Delay(3000);
            var ped = PlayerPedId();
            var menu = new NativeMenu("Garde robe", "Choisissez vos vêtements");
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;
            menu.HeldTime = 100;
            menu.AcceptsInput = true;

            /*
             * 0: Face, 1: Mask, 2: Hair, 3: Torso, 4: Leg,
             * 5: Parachute, bag, 6: Shoes, 7: Accessory,
             * 8: Undershirt, 9: Kevlar, 10: Badge, 11: Torso-2
             */

            // var jsonClothes = "{clothes:[8:{1,1}, 11:{2,2}, 3:{3,3}, 4:{4,4}, 6:{6,6}, 7:{7,7}]}";

            var componentDict = new Dictionary<int, string>()
            {
                // { 1, "Masque" },
                { 8, "Sous haut" },
                { 11, "Haut" },
                { 3, "Bras" },
                { 4, "Bas" },
                { 6, "Chaussures" },
                { 7, "Accessoires" }
                //{ 5, "Sacs" }
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
            return Task.FromResult(0);
        }

        public bool IsPedMale() => IsPedModel(PlayerPedId(), (uint)PedHash.FreemodeMale01);
    }
}