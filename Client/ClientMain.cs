using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
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
        public string Result = "";
        public List<PlayerInstance> PlayersList = new List<PlayerInstance>();
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
        public void SetJail(int playerId, string reason, int time)
        {
            SetEntityCoords(playerId, 1643.1f, 2570.2f, 45.5f, true, false, false, false);
            Format.SendNotif("~r~Vous êtes en jail");
            Format.SendNotif($"~r~Raison: {reason}");
            Format.SendTextUI($"{time}");
        }
    }
}