using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI.Menus;
using Mono.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;
using ShurikenLegal.Shared;
using Game = CitizenFX.Core.Game;

namespace ShurikenLegal.Client.Jobs
{
    public class Police : Job
    {
        public Job Metier;
        public bool radarState;
        public string[] Facture = new string[] { };
        public Vector3 garagePosSortie = new Vector3(458, -987, 26);
        public Vector3 garagePosEntrer = new Vector3(452, -976, 26);

        public Vector3 coffreEntreprise = new Vector3(480, -996, 31);
        public Vector3 clothPos = new Vector3(462, (float)-996.5, 30);

        public Vector3 heliportPosSortie = new Vector3(462, -982, 43);
        public Vector3 heliportPosEntrer = new Vector3(449, -981, 45);

        public Vector3 surprisePos = new Vector3(450, -986, 26);

        public ClientMain Client;

        private int carPlate = 0;

        public Police(ClientMain caller) : base(caller)
        {
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 1,
                JobName = "Police",
                MenuTitle = "Policier",

                ParkingSpots = new List<Vector3>
                {
                    new Vector3(437, -994, 26),
                    new Vector3(446, -991.66f, 26),
                    new Vector3(446, -986, 26)
                },

                AvailableVehicles = new List<string>
                {
                    "police", "policet", "sheriff2", "pol718",
                    "polamggtr", "polrevent", "policefelon"
                },

                Outfits = new Dictionary<string, ClothingSet>
                {
                    ["Capitaine"] = new ClothingSet
                    {
                        Components = new Dictionary<int, ComponentVariation>
                        {
                            [8] = new ComponentVariation { ComponentId = 8, DrawableId = 153, TextureId = 0 },
                            [11] = new ComponentVariation { ComponentId = 11, DrawableId = 55, TextureId = 0 },
                            [4] = new ComponentVariation { ComponentId = 4, DrawableId = 35, TextureId = 0 },
                            [6] = new ComponentVariation { ComponentId = 6, DrawableId = 24, TextureId = 0 }
                        }
                    },
                    ["SWAT"] = new ClothingSet
                    {
                        Components = new Dictionary<int, ComponentVariation>
                        {
                            [8] = new ComponentVariation { ComponentId = 8, DrawableId = 123, TextureId = 0 },
                            [11] = new ComponentVariation { ComponentId = 11, DrawableId = 320, TextureId = 0 },
                            [4] = new ComponentVariation { ComponentId = 4, DrawableId = 58, TextureId = 19 }
                        }
                    }
                },

                HasDoors = true,
                MinRankForRecruitment = 2,

                Doors = new List<DoorClass>
                {
                    // Portes principales
                    new DoorClass { DoorHash = 53265119, ModelHash = 4241622182, Coordinates = new Vector3(481.0084f, -1004.118f, 26.48005f) },
                    new DoorClass { DoorHash = 53264698, ModelHash = 4241622182, Coordinates = new Vector3(476.6157f, -1008.875f, 26.48005f) },
                    new DoorClass { DoorHash = 692702533, ModelHash = 3602318172, Coordinates = new Vector3(469.7743f, -1014.406f, 26.48382f) },
                    new DoorClass { DoorHash = 692702292, ModelHash = 3602318172, Coordinates = new Vector3(467.3686f, -1014.406f, 26.48382f) },
                    new DoorClass { DoorHash = 288880542, ModelHash = 4006163316, Coordinates = new Vector3(469.9274f, -1000.544f, 26.40548f) },
                    new DoorClass { DoorHash = 288880302, ModelHash = 4006163316, Coordinates = new Vector3(467.5222f, -1000.544f, 26.40548f) },
                    new DoorClass { DoorHash = 1547381826, ModelHash = 2747659708, Coordinates = new Vector3(440.7392f, -998.7462f, 30.8153f) },
                    new DoorClass { DoorHash = 1547382137, ModelHash = 2747659708, Coordinates = new Vector3(443.0618f, -998.7462f, 30.8153f) },
                    new DoorClass { DoorHash = 1547382682, ModelHash = 2747659708, Coordinates = new Vector3(434.7444f, -983.0781f, 30.8153f) },
                    new DoorClass { DoorHash = 1547423874, ModelHash = 2747659708, Coordinates = new Vector3(434.7444f, -980.7556f, 30.8153f) },
                    new DoorClass { DoorHash = 96605368, ModelHash = 4198287975, Coordinates = new Vector3(440.5201f, -986.2335f, 30.82319f) },
                    new DoorClass { DoorHash = 1406766092, ModelHash = 2888281650, Coordinates = new Vector3(440.5201f, -977.6011f, 30.82319f) },
                    new DoorClass { DoorHash = 1547423638, ModelHash = 2747659708, Coordinates = new Vector3(455.8862f, -972.2543f, 30.81531f) },
                    new DoorClass { DoorHash = 1547423614, ModelHash = 2747659708, Coordinates = new Vector3(458.2087f, -972.2543f, 30.81531f) },
                    new DoorClass { DoorHash = 692702667, ModelHash = 3602318172, Coordinates = new Vector3(475.8323f, -990.4839f, 26.40548f) },
                    new DoorClass { DoorHash = 1406756682, ModelHash = 2888281650, Coordinates = new Vector3(482.6694f, -983.9868f, 26.40548f) },
                    new DoorClass { DoorHash = 1406756562, ModelHash = 2888281650, Coordinates = new Vector3(482.6701f, -987.5792f, 26.40548f) },
                    new DoorClass { DoorHash = 1406755979, ModelHash = 2888281650, Coordinates = new Vector3(482.6699f, -992.2991f, 26.40548f) },
                    new DoorClass { DoorHash = 1406755813, ModelHash = 2888281650, Coordinates = new Vector3(482.6703f, -995.7285f, 26.40548f) },
                    new DoorClass { DoorHash = 692705399, ModelHash = 3602318172, Coordinates = new Vector3(464.3086f, -984.5284f, 43.77124f) },
                    new DoorClass { DoorHash = 692702651, ModelHash = 3602318172, Coordinates = new Vector3(479.7507f, -999.629f, 30.78917f) },
                    
                    // Cellules
                    new DoorClass { DoorHash = 1, ModelHash = 4241622182, Coordinates = new Vector3(477.9f, -1012.1f, 26.4f) },
                    new DoorClass { DoorHash = 2, ModelHash = 4241622182, Coordinates = new Vector3(480.9f, -1012.1f, 26.4f) },
                    new DoorClass { DoorHash = 3, ModelHash = 4241622182, Coordinates = new Vector3(483.9f, -1012.1f, 26.4f) },
                    new DoorClass { DoorHash = 4, ModelHash = 4241622182, Coordinates = new Vector3(486.9f, -1012.1f, 26.4f) },
                    new DoorClass { DoorHash = 5, ModelHash = 4241622182, Coordinates = new Vector3(485.1f, -1007.6f, 26.27f) }
                }
            };
        }

        protected override void AddCustomMenuItems(NativeMenu menu, JobInfo job)
        {
            var menotter = new NativeItem("Menotter / Démenotter");
            menotter.Activated += (sender, e) =>
            {
                var player = GetPlayerPed(-1);
                var playerCoords = GetEntityCoords(player, true);
                var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());
                if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                {
                    AttachEntityToEntity(player, playerTarget.Handle, 11816, -0.1f, 0.45f, 0, 0, 0, 20, false, false, false, false, 20, false);
                    BaseScript.TriggerServerEvent("core:cuff", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)));
                }
            };
            menu.Add(menotter);

            var fouiller = new NativeItem("Fouiller une personne", "Examiner l'inventaire du joueur le plus proche");
            fouiller.Activated += (sender, e) =>
            {
                CheckNearestPlayerInventory();
            };
            menu.Add(fouiller);
        }

        private void CheckNearestPlayerInventory()
        {
            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
            var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
            var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());

            if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z,
                playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 3.0f)
            {
                int targetServerId = GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle));

                BaseScript.TriggerServerEvent("police:requestPlayerInventory", targetServerId);

                PlaySearchAnimation();
            }
            else
            {
                Main.SendNotif("~r~Aucun joueur à proximité.");
            }
        }

        private async void PlaySearchAnimation()
        {
            string animDict = "amb@prop_human_bum_bin@base";
            string animName = "base";

            RequestAnimDict(animDict);

            while (!HasAnimDictLoaded(animDict))
            {
                await BaseScript.Delay(100);
            }

            TaskPlayAnim(GetPlayerPed(-1), animDict, animName, 8.0f, -8.0f, -1, 1, 0, false, false, false);

            await BaseScript.Delay(4000);

            ClearPedTasks(GetPlayerPed(-1));
        }

        [EventHandler("police:receivePlayerInventory")]
        public void ReceivePlayerInventory(string inventoryJson, int targetId)
        {
            var inventory = JsonConvert.DeserializeObject<List<InventoryItem>>(inventoryJson);
            if (inventory == null || inventory.Count == 0)
            {
                Main.SendNotif("~y~Le joueur n'a rien sur lui.");
                return;
            }

            var searchMenu = new NativeMenu("Fouille", "Inventaire du joueur");
            searchMenu.UseMouse = false;
            Pool.Add(searchMenu);

            foreach (var item in inventory)
            {
                if (string.IsNullOrEmpty(item.Item) || item.Quantity <= 0) continue;

                var menuItem = new NativeItem($"{item.Item} ({item.Quantity})", $"Type: {item.Type}");
                searchMenu.Add(menuItem);

                menuItem.Activated += async (sender, e) =>
                {
                    var actionMenu = new NativeMenu($"Actions - {item.Item}", "Que voulez-vous faire ?");
                    actionMenu.UseMouse = false;
                    Pool.Add(actionMenu);

                    var confiscateItem = new NativeItem("Confisquer cet objet");
                    actionMenu.Add(confiscateItem);

                    confiscateItem.Activated += async (s, ev) =>
                    {
                        var textInput = await GetUserInput("Quantité à confisquer", "1", 3);

                        if (int.TryParse(textInput, out int quantity) && quantity > 0 && quantity <= item.Quantity)
                        {
                            BaseScript.TriggerServerEvent("police:confiscateItem", targetId, item.Item, quantity, item.Type);

                            Main.SendNotif($"~g~Vous avez confisqué {quantity} {item.Item}.");

                            actionMenu.Visible = false;
                            searchMenu.Visible = false;

                            await BaseScript.Delay(500);
                            BaseScript.TriggerServerEvent("police:requestPlayerInventory", targetId);
                        }
                        else if (quantity > item.Quantity)
                        {
                            Main.SendNotif($"~r~Le joueur ne possède que {item.Quantity} {item.Item}.");
                        }
                        else
                        {
                            Main.SendNotif("~r~Quantité invalide.");
                        }
                    };

                    var returnItem = new NativeItem("Rendre cet objet");
                    actionMenu.Add(returnItem);

                    returnItem.Activated += (s, ev) =>
                    {
                        actionMenu.Visible = false;
                    };

                    searchMenu.Visible = false;
                    actionMenu.Visible = true;

                    actionMenu.Closed += (s, ev) =>
                    {
                        searchMenu.Visible = true;
                    };
                };
            }

            var closeButton = new NativeItem("Terminer la fouille");
            searchMenu.Add(closeButton);

            closeButton.Activated += (sender, e) =>
            {
                searchMenu.Visible = false;
                Main.SendNotif("~g~Fouille terminée.");
            };

            searchMenu.Visible = true;
        }

        public override void ShowMenu()
        {
            var job = JsonConvert.DeserializeObject<JobInfo>(Main.PlayerInst.Job);

            var notInServiceMenu = new NativeMenu("Policier", "Menu policier");
            var prendreService = new NativeCheckboxItem("Prendre son service");
            notInServiceMenu.Add(prendreService);

            var inServiceMenu = new NativeMenu("Métier", "Menu métier");
            var finService = new NativeCheckboxItem("Arrêter son service");
            inServiceMenu.Add(finService);

            // Menu interactions joueurs
            var playerInteractMenu = new NativeMenu("Policier", "Intéraction joueurs");
            Pool.Add(playerInteractMenu);

            var facture = new NativeItem("Mettre une facture");
            facture.Activated += async (sender, e) =>
            {
                var textInput = await GetUserInput("Montant", "", 20);
                if (int.TryParse(textInput, out int amount))
                {
                    Main.SendBill("Police", amount, Main.GetPlayer().Name);
                }
            };

            var annonce = new NativeItem("Faire une annonce");
            annonce.Activated += async (sender, e) =>
            {
                var textInput = await GetUserInput("Texte", "", 200);
                if (!string.IsNullOrEmpty(textInput))
                {
                    BaseScript.TriggerServerEvent("core:announce", textInput);
                }
            };

            var menotter = new NativeItem("Menotter / Démenotter");
            menotter.Activated += (sender, e) =>
            {
                var player = GetPlayerPed(-1);
                var playerCoords = GetEntityCoords(player, true);
                var withoutMe = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                var playerTarget = World.GetClosest(playerCoords, withoutMe.ToArray());
                if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y,
                    playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                {
                    AttachEntityToEntity(player, playerTarget.Handle, 11816, -0.1f, 0.45f, 0, 0, 0, 20,
                        false, false, false, false, 20, false);
                    BaseScript.TriggerServerEvent("core:cuff",
                        GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)));
                }
            };

            var fouiller = new NativeItem("Fouiller une personne");
            var miranda = new NativeItem("Lire les droits de Miranda");
            miranda.Activated += (sender, e) =>
            {
                Main.SendNotif("Blablabla est-ce que vous avez bien compris vos droits ?");
            };

            playerInteractMenu.Add(facture);
            playerInteractMenu.Add(annonce);
            playerInteractMenu.Add(menotter);
            playerInteractMenu.Add(fouiller);
            playerInteractMenu.Add(miranda);
            inServiceMenu.AddSubMenu(playerInteractMenu);

            // Menu véhicules
            var carMenu = new NativeMenu("Policier", "Intéraction véhicules");
            Pool.Add(carMenu);

            var radar = new NativeCheckboxItem("Allumer le radar de vitesse",
                "Récupère la vitesse d'un véhicule visé");
            radar.CheckboxChanged += (sender, e) =>
            {
                radarState = !radarState;
            };

            var fourriere = new NativeItem("Mettre en fourrière");
            var ammande = new NativeItem("Mettre une ammande au propriétaire");

            carMenu.Add(radar);
            carMenu.Add(fourriere);
            carMenu.Add(ammande);
            inServiceMenu.AddSubMenu(carMenu);

            // Configuration des menus
            inServiceMenu.UseMouse = false;
            notInServiceMenu.UseMouse = false;
            playerInteractMenu.UseMouse = false;
            carMenu.UseMouse = false;

            // Gestion de la visibilité
            if (en_service)
            {
                notInServiceMenu.Visible = false;
                inServiceMenu.Visible = true;
            }
            else
            {
                notInServiceMenu.Visible = true;
                inServiceMenu.Visible = false;
            }

            finService.CheckboxChanged += (sender, e) =>
            {
                en_service = false;
                inServiceMenu.Visible = false;
                notInServiceMenu.Visible = true;
            };

            prendreService.CheckboxChanged += (sender, e) =>
            {
                notInServiceMenu.Visible = false;
                inServiceMenu.Visible = true;
                en_service = true;
            };

            Pool.Add(notInServiceMenu);
            Pool.Add(inServiceMenu);
        }

        public override void Ticked()
        {
            base.Ticked();
            GetCarInfoWithGun();
        }

        private void GetCarInfoWithGun()
        {
            if (!radarState) return;

            var targettedPed = Game.Player.GetTargetedEntity() as Ped;
            if (targettedPed != null)
            {
                var targettedCar = GetVehiclePedIsIn(targettedPed.Handle, false);
                if (targettedCar != 0)
                {
                    var targettedSpeed = GetEntitySpeed(targettedCar) * 3.6;
                    Main.SendTextUI($"Plaque: {GetVehicleNumberPlateText(targettedCar)} \n" +
                                    $"~w~Vitesse: ~r~{targettedSpeed:0.} KM/H");
                }
            }
        }

    }
}
