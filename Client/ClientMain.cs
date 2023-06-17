using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

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
        public ContainerSystem ContainerSystem;
        public Vector3 dressPos = new Vector3(151.1f, -751.4f, 258.1f);
        public string Result = "";
        private Scaleform _scaleform;

        SkinInfo Skin = new SkinInfo();

        PlayerInstance Player = new PlayerInstance
        {
            Skin = "",
            Firstname = "",
            Lastname = "",
            Birth = "",
            Clothes = "",
            ClothesList = ""
        };

        public List<VehicleInfo> vehicles = new List<VehicleInfo>();

        public int PlayerMoney = 0;
        public Vehicle MyVehicle;

        private bool IsDead = false;
        private DateTime DeathTime;
        public ClientMain()
        {
            Debug.WriteLine("Hi from Core.Client!");
            EventHandlers["playerSpawned"] += new Action(OnPlayerConnecting);
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientStart);
            EventHandlers["baseevents:onPlayerDied"] += new Action<dynamic>(dyn =>
            {
                SetEntityHealth(GetPlayerPed(-1), 100);
                DeathAnimation(600000);
                StartScreenEffect("DeathFailMPIn", 0, false);
                CreateCinematicShot(-1096069633, 2000, 0, GetPlayerPed(-1));
            });
            Format = new Format(this);
            Parking = new Parking(this);
            ConcessAuto = new ConcessAuto(this);
            PlayerMenu = new PlayerMenu(this);
            AmmuNation = new AmmuNation(this);
            Bank = new Bank(this);
            LTDShop = new LTDShop(this);
            ClothShop = new ClothShop(this);
            VehicleSystem = new VehicleSystem(this);
            ContainerSystem = new ContainerSystem(this);
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
            RegisterCommand("revive", new Action<int, int, string>((source, args, raw) =>
            {
                SetEntityHealth(GetPlayerPed(args), 200);
                Format.SendNotif($"Revived {args}");
            }), false);
        }

        public void OnPlayerConnecting()
        {
            TriggerServerEvent("core:isPlayerRegistered");
        }
        public async void DeathAnimation(int time)
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, "dead");
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "dead")) await BaseScript.Delay(50);
            Game.PlayerPed.Task.ClearAllImmediately();
            AnimationFlags flags = AnimationFlags.Loop;
            Game.PlayerPed.Task.PlayAnimation("dead", "dead_a", -1, time, flags);
            ClearPedBloodDamage(GetPlayerPed(-1));
        }

        [EventHandler("core:getSkin")]
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

        [EventHandler("core:getClothes")]
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
                    birthItem.AltTitle = textInput;
                    birth = textInput;
                }
                else
                {
                    Format.CustomNotif("Le format n'est pas valide");
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
                    Player.Firstname = firstname;
                    Player.Lastname = lastname;
                    Player.Birth = birth;
                    Player.Inventory = "[]";
                    Player.Bills = "[]";
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

            menu.Add(hairItem);
            menu.Add(hairColorItem);
            menu.Add(dadItem);
            menu.Add(momItem);
            menu.Add(eyeColorItem);
            menu.Add(beardItem);
            menu.Add(beardColorItem);
            menu.Add(eyebrowItem);

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

            var submit = new NativeItem("Enregistrer", "");
            menu.Add(submit);

            submit.Activated += (sender, e) =>
            {
                menu.Visible = false;
                Pool.Remove(menu);
                Player.Skin = JsonConvert.SerializeObject(Skin);
                DressMenu();
            };

            menu.Closing += (sender, e) =>
            {
                e.Cancel = true;
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
                Player.ClothesList = JsonConvert.SerializeObject(clothesInfoTempList);
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
        public async Task OnTick()
        {
            Pool.Process();
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            // Debug.WriteLine($"{playerCoords}");
            for (int i = 0; i <= 15; i++)
            {
                EnableDispatchService(i, false);
            }
            if (GetPlayerWantedLevel(PlayerId()) > 0)
            {
                SetPlayerWantedLevel(PlayerId(), 0, false);
                SetPlayerWantedLevelNow(PlayerId(), false);
            }

            await PopulationManaged();

            PlayerMenu.F5Menu();
            PlayerMenu.F6Menu();
            Parking.OnTick();
            ConcessAuto.OnTick();
            Bank.OnTick();
            LTDShop.OnTick();
            ClothShop.OnTick();
            VehicleSystem.OnTick();
            AmmuNation.GunShop();
            ContainerSystem.OnTick();
            if (Game.PlayerPed != null && Game.PlayerPed.IsAlive)
            {
                _scaleform.Render2DScreenSpace(new PointF(0.1f, 0.9f), new PointF(0.2f, 0.1f));
                DrawPlayerHealthBar();
            }

            if (IsPedFatallyInjured(GetPlayerPed(-1)))
            {
                IsDead = true;
                DeathTime = DateTime.UtcNow;
            }

            if (IsDead)
            {
                var totalMinutes = 10 - (int)Math.Floor((DateTime.UtcNow - DeathTime).TotalMinutes);
                if (totalMinutes >= 6)
                {
                    Format.SendTextUI($"~r~Vous êtes mort\nRéappartion possible dans {totalMinutes}min");
                }
                if (totalMinutes <= 5)
                {
                    Format.SendTextUI($"~r~Vous êtes mort\nRéappartion possible dans {totalMinutes}min");
                    Format.SendTextUI($"\n\n~r~! Réappartion avancée, appuyer sur ~g~E~r~, coût: ~g~$10000 ~r~!");
                    if (IsControlPressed(0, 38))
                    {
                        if (PlayerMenu.PlayerInst.Money >= 10000)
                        {
                            Format.SendNotif("~g~Vous êtes apparu à l'Hopital");
                            IsDead = false;
                            SetEntityHealth(GetPlayerPed(-1), 200);
                            PlayerMenu.PlayerInst.Money -= 10000;
                            TriggerServerEvent("core:requestPlayerData");
                            DeathAnimation(1);
                            StartScreenEffect("DeathFailFranklinIn", 0, false);
                            SetEntityCoords(GetPlayerPed(-1), -457.6f, -283.3f, 36.3f, true, false, true, false);
                        } else
                        {
                            Format.SendNotif("~r~Vous n'avez pas assez d'argent...");
                        }
                    }
                } else if (totalMinutes <= 0)
                {
                    IsDead = false;
                    StartScreenEffect("DeathFailFranklinIn", 0, false);
                    SetEntityHealth(GetPlayerPed(-1), 200);
                    SetEntityCoords(GetPlayerPed(-1), -457.6f, -283.3f, 36.3f, true, false, true, false);
                }
                if (GetEntityHealth(GetPlayerPed(-1)) >= 175)
                {
                    IsDead = false;
                    DeathAnimation(1);
                    StartScreenEffect("DeadlineNeon", 0, false);
                }
            }
        }

        public Task PopulationManaged()
        {
            SetVehicleDensityMultiplierThisFrame(0.05f);
            SetPedDensityMultiplierThisFrame(0.05f);
            SetRandomVehicleDensityMultiplierThisFrame(0.05f);
            SetParkedVehicleDensityMultiplierThisFrame(0.05f);
            SetScenarioPedDensityMultiplierThisFrame(0.0f, 0.05f);
            return Task.FromResult(0);
        }
        public Player GetLocalPlayer() => this.LocalPlayer;
        public void SpawnPnj(Model ped, Vector3 coords, float heading = 0f)
        {
            ped.Request();
            var shop = World.CreatePed(ped, coords, heading);
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
