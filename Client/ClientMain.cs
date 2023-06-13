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
using System.Drawing;
using LemonUI.Scaleform;
using System.Runtime.CompilerServices;
using LemonUI.Elements;

namespace Core.Client
{
    public class ClientMain : BaseScript
    {
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public Parking Parking;
        public ConcessAuto ConcessAuto;
        public PlayerMenu PlayerMenu;
        public AmmuNation AmmuNation;
        public Bank Bank;
        public LTDShop LTDShop;
        public ClothShop ClothShop;
        public VehicleSystem VehicleSystem;
        public Vector3 dressPos = new Vector3(151.1f, -751.4f, 258.1f);
        public string Result = "";
        private Scaleform _scaleform;

        PlayerInstance Player = new PlayerInstance
        {
            Gender = "",
            Firstname = "",
            Lastname = "",
            Birth = "",
            Clothes = ""
        };

        public List<VehicleInfo> vehicles = new List<VehicleInfo>();

        public int PlayerMoney = 0;
        public Vehicle MyVehicle;
        public ClientMain()
        {
            Debug.WriteLine("Hi from Core.Client!");
            EventHandlers["playerSpawned"] += new Action(OnPlayerConnecting);
            EventHandlers["baseevents:onPlayerDied"] += new Action<int, dynamic>(OnDeath);
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientStart);
            Format = new Format(this);
            Parking = new Parking(this);
            ConcessAuto = new ConcessAuto(this);
            PlayerMenu = new PlayerMenu(this);
            AmmuNation = new AmmuNation(this);
            Bank = new Bank(this);
            LTDShop = new LTDShop(this);
            ClothShop = new ClothShop(this);
            VehicleSystem = new VehicleSystem(this);
            EventHandlers["core:getPlayerData"] += new Action<string>(PlayerMenu.GetPlayerData);
            EventHandlers["core:receivePlayerMoney"] += new Action<int>(money =>
            {
                PlayerMoney = money;
            });
            _scaleform = new Scaleform("mp_car_stats_01");
            
        }

        [EventHandler("core:receivePlayerMoney")]
        public void ReceivePlayerMoney(int money)
        {
            PlayerMoney = money;
        }

        public void OnClientStart(string text)
        {
            UpdatePlayer();
        }

        public void OnPlayerConnecting()
        {
            TriggerServerEvent("core:isPlayerRegistered");
        }

        public void OnDeath(int playerId, dynamic deathCoords)
        {
            if (playerId != PlayerId())
                return;
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
            var pedHash = gender == "Femme" ? PedHash.FreemodeFemale01 : PedHash.FreemodeMale01;
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

        [EventHandler("core:getClothes")]
        public void GetClothes(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            var properties = jsonObject.Properties();
            
            foreach (var property in properties)
            {
                if (property.Value is JObject valueObject)
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
        }

        public void DressPed(int componentId, int drawableId, int textureId)
        {
            SetPedComponentVariation(GetPlayerPed(-1), componentId, drawableId, textureId, 0);
        }

        [EventHandler("core:teleportLastPosition")]
        public void TeleportLastPosition(string json)
        {
            var coords = JsonConvert.DeserializeObject<Vector3>(json);
            Game.PlayerPed.Position = coords;
        }

        [EventHandler("core:sendNotif")]
        public void ServerSendNotif(string text)
        {
            Format.SendNotif(text);
        }

        [EventHandler("core:createCharacter")]
        public void CreateCharacter()
        {
            Game.PlayerPed.Position = dressPos;
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
                if (Format.IsValidDateFormat(textInput))
                {
                    birthItem.AltTitle = textInput;
                    birth = textInput;
                } else
                {
                    Format.CustomNotif("Le format n'est pas valide");
                }
            };

            submit.Activated += (sender, e) =>
            {
                menu.Visible = false;
                Pool.Remove(menu);
                Format.CustomNotif("Vos informations ont bien été enregistré");
                
                Player.Gender = gender;
                Player.Firstname = firstname;
                Player.Lastname = lastname;
                Player.Birth = birth;
                Player.Inventory = "[]";
                Player.Bills = "[]";
                DressMenu();
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

        [EventHandler("core:createSkin")]
        public void CreateSkin()
        {
            DressMenu();
        }

        private void ChangeHair()
        {
            var ped = Game.PlayerPed.Handle;
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
                SetPedComponentVariation(ped, 2, hairItem.SelectedIndex, colorItem.SelectedIndex, 0);
                SetPedHairColor(ped, colorItem.SelectedIndex, 1);
            };

            colorItem.ItemChanged += (sender, e) =>
            {
                SetPedComponentVariation(ped, 2, hairItem.SelectedIndex, colorItem.SelectedIndex, 0);
                SetPedHairColor(ped, colorItem.SelectedIndex, 1);
            };
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

            var submit = new NativeItem("Envoyer");
            menu.Add(submit);

            submit.Activated += (sender, e) =>
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

        [EventHandler("core:getCarInformation")]
        public void GetCarInformation()
        {
            Vehicle vehicle = GetLocalPlayer().Character.CurrentVehicle;

            if (vehicle != null)
            {
                string modelName = GetDisplayNameFromVehicleModel(vehicle.Model);
                string licensePlate = GetVehicleNumberPlateText(vehicle.Handle);
                int engineLevel = GetVehicleMod(vehicle.Handle, 11);
                int brakeLevel = GetVehicleMod(vehicle.Handle, 12);
                var carInfo = new
                {
                    Model = modelName,
                    LicensePlate = licensePlate,
                    EngineLevel = engineLevel,
                    BrakeLevel = brakeLevel,
                };

                string json = JsonConvert.SerializeObject(carInfo);
                TriggerServerEvent("core:receiveCarInformation", json);
            }
        }


        [EventHandler("core:sendVehicleList")]
        public void SendVehicleList(dynamic json)
        {
            foreach (var item in json)
            {
                Console.WriteLine(item);
            }
        }

        [EventHandler("core:changeLockState")]
        public void ChangeLockState(int id, string plate, int isLock)
        {
            if (isLock == 2 || isLock == 0)
            {
                SetVehicleDoorsLocked(id, 1);
                PlayVehicleDoorOpenSound(id, 1);
                Format.SendNotif("~g~Vous avez ouvert votre voiture");
            }
            else if (isLock == 1)
            {
                SetVehicleDoorsLocked(id, 2);
                PlayVehicleDoorOpenSound(id, 2);
                Format.SendNotif("~r~Vous avez fermé votre voiture");
            }
            else
            {
                Format.SendNotif("Valeur de verrouillage non valide");
            }
        }

        [Tick]
        public Task OnTick()
        {
            Pool.Process();
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            Format.SendTextUI($"{playerCoords}");
            PopulationManaged();

            PlayerMenu.F5Menu();
            PlayerMenu.F6Menu();
            Parking.OnTick();
            ConcessAuto.OnTick();
            Bank.OnTick();
            LTDShop.OnTick();
            ClothShop.OnTick();
            VehicleSystem.OnTick();
            AmmuNation.GunShop();

            if (Game.PlayerPed != null && Game.PlayerPed.IsAlive)
            {
                _scaleform.Render2DScreenSpace(new PointF(0.1f, 0.9f), new PointF(0.2f, 0.1f));
                DrawPlayerHealthBar();
            }
            
            return Task.FromResult(0);
        }
        public Task PopulationManaged()
        {
            SetVehicleDensityMultiplierThisFrame(0.5f);
            SetPedDensityMultiplierThisFrame(0.5f);
            SetRandomVehicleDensityMultiplierThisFrame(0.5f);
            SetParkedVehicleDensityMultiplierThisFrame(0.5f);
            SetScenarioPedDensityMultiplierThisFrame(0.5f, 0.5f);
            return Task.FromResult(0);
        }
        public Player GetLocalPlayer() => this.LocalPlayer;
        public void SpawnPnj(Model ped, float x, float y, float z, float heading = 0f)
        {
            ped.Request();
            var shop = World.CreatePed(ped, new Vector3(x, y, z), heading);
            FreezeEntityPosition(shop.Result.Handle, true);
            SetEntityInvincible(shop.Result.Handle, true);
            SetBlockingOfNonTemporaryEvents(shop.Result.Handle, true);
            TaskStartScenarioInPlace(shop.Result.Handle, "WORLD_HUMAN_COP_IDLES", 0, true);
        }
        public void AddEvent(string key, System.Delegate value) => this.EventHandlers.Add(key, value);

        public void UpdateVehicles()
        {
            TriggerServerEvent("core:getVehicleInfo");
        }

        public void UpdatePlayer()
        {
            TriggerServerEvent("core:requestPlayerData");
        }

        private void DrawPlayerHealthBar()
        {
            float health = Game.PlayerPed.Health;
            float maxHealth = Game.PlayerPed.MaxHealth;

            float barWidth = 0.16f;
            float healthBarWidth = (health / maxHealth) * barWidth;

            API.DrawRect(0.07f, 0.75f, 0.16f, 0.02f, 0, 0, 0, 150);
            API.DrawRect(0.07f, 0.75f, healthBarWidth, 0.02f, 0, 255, 0, 150);
        }

    }
}
