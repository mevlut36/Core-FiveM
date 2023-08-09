using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.NaturalMotion;
using LemonUI;
using LemonUI.Menus;
using Mono.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Core.Shared;

namespace Core.Client
{
    public class ClientMain : BaseScript
    {
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public PlayerSystem PlayerSystem;
        public Parking Parking;
        public ConcessAuto ConcessAuto;
        public PlayerMenu PlayerMenu;
        public AmmuNation AmmuNation;
        public Bank Bank;
        public LTDShop LTDShop;
        public ClothShop ClothShop;
        public VehicleSystem VehicleSystem;
        public ContainerSystem ContainerSystem;
        public DiscordPresence DiscordPresence;
        public Teleport Teleport;

        public List<PlayerInstance> PlayersInstList = new List<PlayerInstance>();
        public List<string> PlayersHandle = new List<string>();
        public List<VehicleInfo> vehicles = new List<VehicleInfo>();

        public int PlayerMoney = 0;
        public VehicleInfo MyVehicle;

        public bool IsDead = false;
        private DateTime DeathTime;
        public ReportSystem reportSystem = new ReportSystem();
        List<LTDItems> NarcoItems = new List<LTDItems>();
        public ClientMain()
        {
            Debug.WriteLine("Hi from Core.Client!");
            EventHandlers["playerSpawned"] += new Action(OnPlayerConnecting);
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientStart);
            EventHandlers["core:setHealth"] += new Action<int>(SetHealth);
            EventHandlers["core:cuffPlayer"] += new Action<bool>(SetCuff);
            EventHandlers["baseevents:onPlayerKilled"] += new Action<dynamic>(async dyn =>
            {
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                NetworkResurrectLocalPlayer(playerCoords.X, playerCoords.Y, playerCoords.Z, 90, true, false);
                ClearPedTasksImmediately(GetPlayerPed(-1));
                await OnPlayerDeath();
            });
            EventHandlers["baseevents:onPlayerDied"] += new Action<dynamic>(async dyn =>
            {
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                NetworkResurrectLocalPlayer(playerCoords.X, playerCoords.Y, playerCoords.Z, 90, true, false);
                ClearPedTasksImmediately(GetPlayerPed(-1));
                await OnPlayerDeath();
            });

            Format = new Format(this);
            PlayerMenu = new PlayerMenu(this);
            PlayerSystem = new PlayerSystem(this);
            Parking = new Parking(this);
            ConcessAuto = new ConcessAuto(this);
            AmmuNation = new AmmuNation(this);
            Bank = new Bank(this);
            LTDShop = new LTDShop(this);
            ClothShop = new ClothShop(this);
            VehicleSystem = new VehicleSystem(this);
            ContainerSystem = new ContainerSystem(this);
            DiscordPresence = new DiscordPresence(this);
            Teleport = new Teleport(this);
            EventHandlers["core:getPlayerData"] += new Action<string>(PlayerMenu.GetPlayerData);
            EventHandlers["core:reveivePlayersRobbery"] += new Action<string>(Bank.GetPlayers);
            RegisterCommand("report", new Action<int, List<object>, string>((source, args, raw) =>
            {
                string arguments = string.Join(" ", args);
                Format.SendNotif("~g~Votre report a bien été envoyer, veuillez patienter");
                var newReport = new ReportClass
                {
                    Player = PlayerMenu.PlayerInst,
                    Text = arguments,
                };
                reportSystem.AddReport(newReport);
            }), false);

            RegisterCommand("setrank", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (PlayerMenu.PlayerInst.Rank == "staff")
                {
                    var playerId = Convert.ToInt32(args[0]);
                    var gangId = Convert.ToString(args[1]);
                    if (args.Count < 2 || gangId != "staff" || gangId != "player")
                    {
                        Format.SendNotif("~r~Usage: /setrank [player_id] [staff|player]");
                    }
                    else
                    {
                        TriggerServerEvent("core:setRank", playerId, gangId);
                    }
                }
            }), false);

            Tick += ScenarioSuppressionLoop;

            var item1 = new LTDItems("Ordinateur", "Idéal pour faire du pentest...", 25000);
            NarcoItems.Add(item1);

            var item2 = new LTDItems("Perceuse", "Idéale pour percer une porte...", 25000);
            NarcoItems.Add(item2);

            var item3 = new LTDItems("Phone", "Excellent téléphone", 1000);
            NarcoItems.Add(item3);
        }

        private static readonly List<string> SCENARIO_TYPES = new List<string>
        {
            "WORLD_VEHICLE_MILITARY_PLANES_SMALL", // Zancudo Small Planes
            "WORLD_VEHICLE_MILITARY_PLANES_BIG", // Zancudo Big Planes
        };

        private static readonly List<int> SCENARIO_GROUPS = new List<int>
        {
            2017590552, // LSIA planes
            2141866469, // Sandy Shores planes
            1409640232, // Grapeseed planes
        };

        private static readonly List<string> SUPPRESSED_MODELS = new List<string>
        {
            "SHAMAL", // They spawn on LSIA and try to take off
            "LUXOR", // They spawn on LSIA and try to take off
            "LUXOR2", // They spawn on LSIA and try to take off
            "JET", // They spawn on LSIA and try to take off and land, remove this if you still want em in the skies
            "LAZER", // They spawn on Zancudo and try to take off
            "TITAN", // They spawn on Zancudo and try to take off
            "BARRACKS", // Regularly driving around the Zancudo airport surface
            "BARRACKS2", // Regularly driving around the Zancudo airport surface
            "CRUSADER", // Regularly driving around the Zancudo airport surface
            "RHINO", // Regularly driving around the Zancudo airport surface
            "AIRTUG", // Regularly spawns on the LSIA airport surface
            "RIPLEY", // Regularly spawns on the LSIA airport surface
        };

        public List<int> PedId = new List<int>();

        public bool IsRobbing = false;
        public DateTime? LastRobbery = null;

        public void CheckIfPlayerIsAimingAtPed()
        {
            int playerId = PlayerId();

            foreach (var ped in PedId)
            {
                if (IsPlayerFreeAimingAtEntity(playerId, ped))
                {
                    if (LastRobbery.HasValue && (DateTime.Now - LastRobbery.Value).TotalHours < 1)
                    {
                        Format.SendNotif("~r~Vous ne pouvez pas braquer encore. Attendez un peu.");
                        return;
                    }

                    IsRobbing = true;
                    LastRobbery = DateTime.Now;
                    Debug.WriteLine($"Robbing ID {ped}");
                    Format.SendNotif("~r~Braquage en cours...");
                    var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                    uint streetNameHash = 0;
                    uint crossingRoadHash = 0;
                    GetStreetNameAtCoord(playerCoords.X, playerCoords.Y, playerCoords.Z, ref streetNameHash, ref crossingRoadHash);
                    string streetName = GetStreetNameFromHashKey(streetNameHash);
                    TriggerServerEvent("core:robbery", streetName);
                    PlaySoundFrontend(-1, "ROBBERY_MONEY_TOTAL", "HUD_FRONTEND_CUSTOM_SOUNDSET", true);
                }
            }
        }

        public void SetHealth(int health)
        {
            SetEntityHealth(GetPlayerPed(-1), health);
            SetEntityInvincible(GetPlayerPed(-1), false);
            IsDead = false;
            SetEntityHealth(GetPlayerPed(-1), 200);
            ClearPedTasksImmediately(GetPlayerPed(-1));
        }

        public async void SetCuff(bool state)
        {
            if (state)
            {
                RequestAnimDict("mp_arrest_paired");
                while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "mp_arrest_paired")) await Delay(50);
                TaskPlayAnim(GetPlayerPed(-1), "mp_arrest_paired", "crook_p2_back_left", 8.0f, -8.0f, 5500, 33, 0, false, false, false);
            } else
            {
                Format.PlayAnimation("mp_prison_break", "handcuffed", 8, (AnimationFlags)48);
            }
        }

        public void OnClientStart(string text)
        {
            // UpdatePlayer();
        }

        [EventHandler("core:playerConnected")]
        public async void PlayerConnected(string json)
        {
            try
            {
                var player = JsonConvert.DeserializeObject<PlayerInstance>(json);
                PlayerMenu.PlayerInst = player;
                var skin = JsonConvert.DeserializeObject<SkinInfo>(PlayerMenu.PlayerInst.Skin);
                var ped = GetPlayerPed(-1);
                var pedHash = skin.Gender == "Femme" ? PedHash.FreemodeFemale01 : PedHash.FreemodeMale01;
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
                }
                
                Format.SendNotif($"~g~Bienvenue ~w~{player.Firstname}");
                foreach (var clothes in player.Clothes)
                {
                    if (!string.IsNullOrEmpty(clothes.Name))
                    {
                        SetPedComponentVariation(GetPlayerPed(-1), clothes.Component, clothes.Drawable, clothes.Texture, 0);
                    }
                }
                SetPedComponentVariation(LocalPlayer.Character.Handle, 2, skin.Hair, skin.HairColor, 0);
                SetPedHairColor(LocalPlayer.Character.Handle, skin.HairColor, 1);
                SetPedHeadBlendData(LocalPlayer.Character.Handle, skin.Mom, skin.Dad, skin.Dad, skin.Mom, skin.Dad, skin.Dad, skin.DadMomPercent * 0.1f, skin.DadMomPercent * 0.1f, 1.0f, false);
                SetPedEyeColor(LocalPlayer.Character.Handle, skin.EyeColor);
                SetPedHeadOverlay(LocalPlayer.Character.Handle, 1, skin.Beard, skin.BeardOpacity * 0.1f);
                SetPedHeadOverlayColor(LocalPlayer.Character.Handle, 1, 1, skin.BeardColor, skin.BeardColor);
                SetPedHeadOverlay(LocalPlayer.Character.Handle, 2, skin.Eyebrow, 10 * 0.1f);
                SetPedHeadOverlayColor(LocalPlayer.Character.Handle, 1, 1, 1, 1);
                SetPedHeadOverlayColor(LocalPlayer.Character.Handle, 2, 1, skin.EyebrowOpacity, skin.EyebrowOpacity);
                Game.PlayerPed.Position = PlayerMenu.PlayerInst.LastPosition;
                PlayerMenu.PlayerInst.Cars = PlayerMenu.PlayerInst.Cars;
                var vehicles = JsonConvert.DeserializeObject<List<VehicleInfo>>(player.Cars);
                if (vehicles != null && vehicles.Count > 0)
                {
                    Parking.CarList = vehicles;
                }
            }
            catch (JsonSerializationException ex)
            {
                Debug.WriteLine($"Error deserializing JSON: {ex.Message}");
                // Debug.WriteLine($"JSON: {json}");
            }
        }

        public void OnPlayerConnecting()
        {
            TriggerServerEvent("core:playerSpawned");
            NetworkSetFriendlyFireOption(true);
            SetCanAttackFriendly(PlayerPedId(), true, true);
            LTDShop.CreatePeds();
            ClothShop.CreatePeds();
            SpawnDealer();
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

        [EventHandler("core:sendNotif")]
        public void ServerSendNotif(string text)
        {
            Format.SendNotif(text);
        }

        [EventHandler("core:getVehicleByPlate")]
        public void GetVehicleByPlate(string json)
        {
            MyVehicle = JsonConvert.DeserializeObject<VehicleInfo>(json);
        }

        private async Task ScenarioSuppressionLoop()
        {
            while (true)
            {
                foreach (var sctyp in SCENARIO_TYPES)
                {
                    SetScenarioTypeEnabled(sctyp, false);
                }

                foreach (var scgrp in SCENARIO_GROUPS)
                {
                    SetScenarioGroupEnabled(scgrp.ToString(), false);
                }

                foreach (var model in SUPPRESSED_MODELS)
                {
                    SetVehicleModelIsSuppressed((uint)GetHashKey(model), false);
                }

                await Delay(10000);
            }
        }

        public async Task OnPlayerDeath()
        {
            if (IsDead) return;
            IsDead = true;
            DeathTime = DateTime.UtcNow;

            StartScreenEffect("DeathFailMPIn", 0, false);
            CreateCinematicShot(-1096069633, 2000, 0, GetPlayerPed(-1));
            DeathAnimation(600000);
            SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, true);
            while (IsDead && DateTime.UtcNow - DeathTime < TimeSpan.FromMinutes(10))
            {
                if (!IsEntityPlayingAnim(GetPlayerPed(-1), "dead", "dead_a", 3))
                {
                    DeathAnimation(600000);
                }
                await Delay(1);
                SetEntityInvincible(GetPlayerPed(-1), true);
                var remainingTime = TimeSpan.FromMinutes(10) - (DateTime.UtcNow - DeathTime);
                var totalMinutes = Math.Round(remainingTime.TotalMinutes);

                Format.SendTextUI($"~r~Vous êtes mort\nRéappartion possible dans {totalMinutes}min");

                if (GetEntityHealth(GetPlayerPed(1)) < 200)
                {
                    if (DateTime.UtcNow - DeathTime >= TimeSpan.FromMinutes(5))
                    {
                        Format.SendTextUI($"\n\n~r~! Réappartion avancée, appuyer sur ~g~E~r~, coût: ~g~$10000 ~r~!");

                        if (PlayerMenu.PlayerInst.Money >= 10000)
                        {
                            if (IsControlPressed(0, 38))
                            {
                                PlayerMenu.PlayerInst.Money -= 10000;
                                SetEntityHealth(GetPlayerPed(-1), 200);
                                SetEntityCoords(GetPlayerPed(-1), -457.6f, -283.3f, 36.3f, true, false, true, false);
                                SetEntityInvincible(GetPlayerPed(-1), false);
                                IsDead = false;
                            }
                        }
                        else
                        {
                            Format.SendNotif("Vous n'avez pas assez d'argent");
                        }
                    }
                } else
                {
                    IsDead = false;
                    SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, false);
                    SetEntityInvincible(GetPlayerPed(-1), false);
                }
            }

            if (IsDead)
            {
                SetEntityHealth(GetPlayerPed(-1), 200);
                SetEntityCoords(GetPlayerPed(-1), -457.6f, -283.3f, 36.3f, true, false, true, false);
                IsDead = false;
                SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, false);
                SetEntityInvincible(GetPlayerPed(-1), false);
            }
        }
        
        public async void SpawnDealer()
        {
            var pedHash = PedHash.Malibu01AMM;
            RequestModel((uint)pedHash);
            while (!HasModelLoaded((uint)pedHash))
            {
                await Delay(100);
            }
            var ped = World.CreatePed(pedHash, new Vector3(123.5f, -1040.4f, 28.2f));
            FreezeEntityPosition(ped.Result.Handle, true);
            SetEntityInvincible(ped.Result.Handle, true);
            SetBlockingOfNonTemporaryEvents(ped.Result.Handle, true);
        }
        public void NarcoMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var items = PlayerMenu.PlayerInst.Inventory;
            var pnj = new Vector3(123.5f, -1040.4f, 29);

            var distance = GetDistanceBetweenCoords(pnj.X, pnj.Y, pnj.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);

            if (distance < 5)
            {
                Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                if (IsControlJustPressed(0, 38))
                {
                    var menu = new NativeMenu("Pablo", $"Chut")
                    {
                        UseMouse = false
                    };
                    Pool.Add(menu);
                    menu.Visible = true;

                    var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");

                    foreach (var item in NarcoItems)
                    {
                        var itemDefault = items.FirstOrDefault(i => i.Item == item.Name);
                        var _ = new NativeItem($"{item.Name}", $"{item.Description}", $"~g~${item.Price}");
                        menu.Add(_);
                        _.Activated += async (sender, e) =>
                        {
                            var textInput = await Format.GetUserInput("Quantité", "1", 4);
                            var parsedInput = Int32.Parse(textInput);
                            var result = item.Price * parsedInput;
                            if (result <= PlayerMenu.PlayerInst.Money)
                            {
                                PlayerMenu.PlayerInst.Money -= result;
                                PlayerMenu.PlayerInst.Inventory = items;
                                TriggerServerEvent("core:transaction", result, item.Name, parsedInput, "item");
                                menu.Visible = false;
                            }
                            else
                            {
                                Format.SendNotif("~r~La somme est trop élevée");
                            }

                        };
                    }

                }
            }
        }

        [Tick]
        public async Task OnTick()
        {
            var playerPos = Game.PlayerPed.Position;
            // Format.SendTextUI($"{playerPos}");
            Pool.Process();
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
            Parking.OnTick();
            ConcessAuto.OnTick();
            Bank.OnTick();
            LTDShop.OnTick();
            ClothShop.OnTick();
            VehicleSystem.OnTick();
            AmmuNation.GunShop();
            Teleport.OnTick();
            // ContainerSystem.OnTick();
            NarcoMenu();

            if (Game.PlayerPed != null && Game.PlayerPed.IsAlive)
            {
                DrawPlayerHealthBar();
            }
            foreach (var ped in PedId)
            {
                if (GetEntityCoords(GetPlayerPed(-1), true).DistanceToSquared2D(GetEntityCoords(ped, true)) < 10)
                {
                    CheckIfPlayerIsAimingAtPed();
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
        public void AddEvent(string key, System.Delegate value) => this.EventHandlers.Add(key, value);
        private void DrawPlayerHealthBar()
        {
            float health = Game.PlayerPed.Health;
            float maxHealth = Game.PlayerPed.MaxHealth;

            float barWidth = 0.16f;
            float healthBarWidth = (health / maxHealth) * barWidth;

            API.DrawRect(0.07f, 0.75f, 0.16f, 0.02f, 0, 0, 0, 150);
            API.DrawRect(0.07f, 0.75f, healthBarWidth, 0.02f, 0, 255, 0, 150);
        }
        public void SetJail(string playerId, string reason, int time)
        {
            TriggerServerEvent("core:jail", playerId, reason, time);
        }


    }
}