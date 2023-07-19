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
        public string Result = "";
        public List<PlayerInstance> PlayersInstList = new List<PlayerInstance>();
        public List<string> PlayersHandle = new List<string>();
        public List<VehicleInfo> vehicles = new List<VehicleInfo>();

        public int PlayerMoney = 0;
        public Vehicle MyVehicle;

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
            EventHandlers["baseevents:onPlayerDied"] += new Action<dynamic>(async dyn =>
            {
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                SetEntityInvincible(GetPlayerPed(-1), true);
                NetworkResurrectLocalPlayer(playerCoords.X, playerCoords.Y, playerCoords.Z, 90, true, false);
                ClearPedTasksImmediately(GetPlayerPed(-1));
                await OnPlayerDeath();
            });

            Format = new Format(this);
            PlayerSystem = new PlayerSystem(this);
            Parking = new Parking(this);
            ConcessAuto = new ConcessAuto(this);
            PlayerMenu = new PlayerMenu(this);
            AmmuNation = new AmmuNation(this);
            Bank = new Bank(this);
            LTDShop = new LTDShop(this);
            ClothShop = new ClothShop(this);
            VehicleSystem = new VehicleSystem(this);
            ContainerSystem = new ContainerSystem(this);
            DiscordPresence = new DiscordPresence(this);

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
            Tick += ScenarioSuppressionLoop;

            TriggerServerEvent("core:spawnPnj", "csb_chin_goon", new Vector3(123.5f, -1040.4f, 29));
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

        List<Vector3> pedList = new List<Vector3>();
        List<int> pedId = new List<int>();

        [EventHandler("core:spawnPeds")]
        public async void SpawnPed(string json)
        {
            pedList = JsonConvert.DeserializeObject<List<Vector3>>(json);

            foreach (var ped in pedList)
            {
                var PedId = await World.CreatePed(PedHash.Malibu01AMM, new Vector3(ped.X, ped.Y, ped.Z));
                Debug.WriteLine($"Created ped with ID {PedId}");
                SetEntityInvincible(PedId.Handle, true);
                SetBlockingOfNonTemporaryEvents(PedId.Handle, true);
                pedId.Add(PedId.Handle);
            }
        }

        public bool IsRobbing = false;
        public void CheckIfPlayerIsAimingAtPed()
        {
            int playerId = PlayerId();

            foreach (var ped in pedId)
            {
                if (IsPlayerFreeAimingAtEntity(playerId, ped) && IsRobbing == false)
                {
                    IsRobbing = true;
                    Debug.WriteLine($"Robbing ID {ped}");
                    Format.SendNotif("~r~Braquage en cours...");
                    TriggerServerEvent("core:robbery");
                    PlaySoundFrontend(-1, "ROBBERY_MONEY_TOTAL", "HUD_FRONTEND_CUSTOM_SOUNDSET", true);
                }
            }

        }


        public void SetHealth(int health)
        {
            SetEntityHealth(GetPlayerPed(-1), health);
            IsDead = false;
            SetEntityHealth(GetPlayerPed(-1), 200);
            ClearPedTasksImmediately(GetPlayerPed(-1));
        }
        public void OnClientStart(string text)
        {
            // UpdatePlayer();
        }
        public void OnPlayerConnecting()
        {
            TriggerServerEvent("core:isPlayerRegistered");
            NetworkSetFriendlyFireOption(true);
            SetCanAttackFriendly(PlayerPedId(), true, true);
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
                }
            }

            if (IsDead)
            {
                SetEntityHealth(GetPlayerPed(-1), 200);
                SetEntityCoords(GetPlayerPed(-1), -457.6f, -283.3f, 36.3f, true, false, true, false);
                IsDead = false;
                SetEntityInvincible(GetPlayerPed(-1), false);
            }
        }


        public void NarcoMenu()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerMenu.PlayerInst.Inventory);
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
                                PlayerMenu.PlayerInst.Inventory = JsonConvert.SerializeObject(items);
                                TriggerServerEvent("core:transaction", result, item.Name, parsedInput, "item");
                                UpdatePlayer();
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
            PlayerMenu.F6Menu();
            Parking.OnTick();
            ConcessAuto.OnTick();
            Bank.OnTick();
            LTDShop.OnTick();
            ClothShop.OnTick();
            VehicleSystem.OnTick();
            AmmuNation.GunShop();
            ContainerSystem.OnTick();
            NarcoMenu();

            if (Game.PlayerPed != null && Game.PlayerPed.IsAlive)
            {
                DrawPlayerHealthBar();
            }
            foreach (var ped in pedList)
            {
                if (GetEntityCoords(GetPlayerPed(-1), true).DistanceToSquared2D(ped) < 10)
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
        public void SetJail(string playerId, string reason, int time)
        {
            TriggerServerEvent("core:jail", playerId, reason, time);
        }
    }
}