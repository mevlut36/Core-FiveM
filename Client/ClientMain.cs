using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.NaturalMotion;
using LemonUI;
using Mono.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        private bool IsDead = false;
        private DateTime DeathTime;
        public ReportSystem reportSystem = new ReportSystem();
        public ClientMain()
        {
            Debug.WriteLine("Hi from Core.Client!");
            EventHandlers["playerSpawned"] += new Action(OnPlayerConnecting);
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientStart);
            EventHandlers["core:setHealth"] += new Action<int>(SetHealth);
            EventHandlers["baseevents:onPlayerDied"] += new Action<dynamic>(dyn =>
            {
                SetEntityHealth(GetPlayerPed(-1), 100);
                DeathAnimation(600000);
                StartScreenEffect("DeathFailMPIn", 0, false);
                CreateCinematicShot(-1096069633, 2000, 0, GetPlayerPed(-1));
                SetPlayerHealthRechargeMultiplier(GetPlayerPed(-1), 0.5f);
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
            SetCanAttackFriendly(GetPlayerPed(-1), true, false);
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

        public void SetHealth(int health)
        {
            SetEntityHealth(GetPlayerPed(-1), health);
        }
        public void OnClientStart(string text)
        {
            UpdatePlayer();
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

        [Tick]
        public async Task OnTick()
        {
            Pool.Process();
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            Format.SendTextUI($"{playerCoords}");
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
                DrawPlayerHealthBar();
            }

            if (IsPedFatallyInjured(GetPlayerPed(-1)))
            {
                IsDead = true;
                DeathTime = DateTime.UtcNow;
            }

            if (IsDead)
            {
                SetPlayerInvincible(GetPlayerPed(-1), true);
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
                            UpdatePlayer();
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
                    SetPlayerInvincible(GetPlayerPed(-1), false);
                    SetEntityCoords(GetPlayerPed(-1), -457.6f, -283.3f, 36.3f, true, false, true, false);
                }
                if (GetEntityHealth(GetPlayerPed(-1)) >= 175)
                {
                    IsDead = false;
                    SetPlayerInvincible(GetPlayerPed(-1), false);
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