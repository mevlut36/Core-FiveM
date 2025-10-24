using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using LemonUI.Menus;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using ShurikenLegal.Shared;

namespace ShurikenLegal.Client.Jobs
{
    public class EMS : Job
    {
        public ClientMain Client;
        public Vector3 garagePosSortie = new Vector3(-454.7f, -340.6f, 35);
        public Vector3 garagePosEntrer = new Vector3(-454.7f, -340.6f, 35);

        public Vector3 coffreEntreprise = new Vector3(-503.9f, -298.2f, 70);
        public Vector3 clothPos = new Vector3(-443.6f, -310.3f, 34.2f);
        private Vector3 coords_closest_player = new Vector3();
        private bool MarkerState = false;

        public EMS(ClientMain caller) : base(caller) { }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 3,
                JobName = "EMS",
                MenuTitle = "EMS",

                // Positions
                PosCoffreEntreprise = coffreEntreprise,
                PosGarageSortie = garagePosSortie,
                PosGarageEntrer = garagePosEntrer,
                PosCloth = clothPos,

                Outfits = new Dictionary<string, ClothingSet>
                {
                    ["Tenue de travail"] = new ClothingSet
                    {
                        Components = new Dictionary<int, ComponentVariation>
                        {
                            [8] = new ComponentVariation { ComponentId = 8, DrawableId = 242, TextureId = 0 },
                            [11] = new ComponentVariation { ComponentId = 11, DrawableId = 15, TextureId = 0 },
                            [4] = new ComponentVariation { ComponentId = 4, DrawableId = 0, TextureId = 0 },
                            [6] = new ComponentVariation { ComponentId = 6, DrawableId = 129, TextureId = 0 },
                            [3] = new ComponentVariation { ComponentId = 3, DrawableId = 24, TextureId = 0 }
                        }
                    },
                },

                AvailableVehicles = new List<string>
                {
                    "ambulance",
                },
            };
        }

        public override void ShowMenu()
        {
            var menu = new NativeMenu("EMS", "Menu intéraction");
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;

            var revive = new NativeItem("Soigner", "Soigner le joueur le plus proche");
            menu.Add(revive);

            revive.Activated += (sender, e) =>
            {
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());
                if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                {
                    Main.CallServerEvent("core:heal", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)), 200);
                }
            };

            revive.Selected += (sender, e) =>
            {
                MarkerState = true;
                UpdateClosestPlayer();
            };

            menu.Closed += (sender, e) =>
            {
                MarkerState = false;
            };
        }

        private void UpdateClosestPlayer()
        {
            var myPed = Game.PlayerPed;
            var myCoords = myPed.Position;
            var allPeds = World.GetAllPeds().Where(ped => ped != myPed);
            var closestPed = allPeds.OrderBy(ped => (ped.Position - myCoords).LengthSquared()).FirstOrDefault();
            coords_closest_player = closestPed?.Position ?? new Vector3();
        }

        private void MarkerPlayer(Vector3 coords)
        {
            float groundZ = World.GetGroundHeight(new Vector3(coords.X, coords.Y, 0));
            World.DrawMarker(MarkerType.VerticalCylinder, new Vector3(coords.X, coords.Y, groundZ + 1f),
                Vector3.Zero, Vector3.Zero, new Vector3(1f, 1f, 1f), System.Drawing.Color.FromArgb(100, 255, 0, 0));
        }

        [EventHandler("revive_client")]
        public void Revive(int id)
        {
            Debug.WriteLine($"Revive() {id}");
            var ped = GetPlayerPed(id);
            var playerPos = GetEntityCoords(ped, true);
            NetworkResurrectLocalPlayer(playerPos.X, playerPos.Y, playerPos.Z, 0, false, false);
            SetPlayerInvincible(ped, false);
            ClearPedBloodDamage(ped);
        }

        public override void Ticked()
        {
            base.Ticked();

            if (MarkerState)
            {
                MarkerPlayer(coords_closest_player);
            }
        }
    }
}