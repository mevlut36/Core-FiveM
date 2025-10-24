using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShurikenLegal.Shared;
using LemonUI.Menus;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace ShurikenLegal.Client.Jobs
{
    public class Immobilier : Job
    {
        public Job Metier;
        public ClientMain Client;

        public Immobilier(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
        }
        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 5,
                JobName = "Immobilier",
                MenuTitle = "Immobilier",
            };
        }

        public override void ShowMenu()
        {
            var menu = new NativeMenu("Immobilier", "Agent immobilier");
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;

            var buildingList = new NativeMenu("Liste des immeubles", "Liste des immeubles");
            Pool.Add(buildingList);
            buildingList.UseMouse = false;
            menu.AddSubMenu(buildingList);
            foreach (var building in Client.Buildings)
            {
                var buildingItem = new NativeMenu($"Nombre d'appartements: {building.Appartments.Count}",
                    $"{building.Address}", $"Coordonnées: {building.Door}");
                Pool.Add(buildingItem);
                buildingItem.UseMouse = false;
                buildingList.AddSubMenu(buildingItem);
                foreach (var item in building.Appartments)
                {
                    var isLocked = item.IsLocked ? "~r~Fermé" : "~g~Ouvert";
                    var appartItem = new NativeMenu($"{item.Resident}", $"{item.Resident}");
                    Pool.Add(appartItem);
                    appartItem.UseMouse = false;
                    buildingItem.AddSubMenu(appartItem);
                    if (item.Resident == "Libre")
                    {
                        var assign = new NativeItem("Assigner l'appartement", "Au joueur le plus proche ou vous");
                        appartItem.Add(assign);
                        assign.Activated += (sender, e) =>
                        {
                            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                            var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                            var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());
                            if (playerCoords.DistanceToSquared(playerTarget.Position) < 10)
                            {
                                BaseScript.TriggerServerEvent("legal_server:assignAppart",
                                    GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)), building.Address, JsonConvert.SerializeObject(item));
                            }
                        };

                        var assignMe = new NativeItem("Assigner à moi");
                        appartItem.Add(assignMe);
                        assignMe.Activated += (sender, e) =>
                        {
                            var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                            var playerTarget = GetPlayerPed(-1);
                            BaseScript.TriggerServerEvent("legal_server:assignAppart",
                                    GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget)), building.Address, JsonConvert.SerializeObject(item));
                        };
                    }
                }
            }

            // Add a building
            var addBuilding = new NativeMenu("Ajouter un immeuble", "Ajouter un immeuble");
            Pool.Add(addBuilding);
            menu.AddSubMenu(addBuilding);
            addBuilding.UseMouse = false;

            var address = new NativeItem("Adresse");
            addBuilding.Add(address);
            address.Activated += async (sender, e) =>
            {
                var input = await GetUserInput("Adresse", "", 50);
                address.AltTitle = input;
            };

            var door = new NativeItem("Porte d'entrée", "", "");
            addBuilding.Add(door);
            var doorPosition = new Vector3();
            door.Activated += (sender, e) =>
            {
                doorPosition = Game.Player.Character.Position;
                door.AltTitle = $"{Game.Player.Character.Position}";
            };

            var submit = new NativeItem("Enregistrer");
            addBuilding.Add(submit);

            submit.Activated += (sender, e) =>
            {
                if (address.AltTitle != null || door.AltTitle != null)
                {
                    var building = new Building(address.AltTitle, doorPosition, new List<Appartment>());
                    BaseScript.TriggerServerEvent("legal_server:addBuilding", JsonConvert.SerializeObject(building));
                } else
                {
                    Client.SendNotif("~r~Une information est manquante");
                }
            };

            // Add an appartment
            var appart = new NativeMenu("Appartement", "Ajouter un appartement");
            Pool.Add(appart);
            menu.AddSubMenu(appart);
            appart.UseMouse = false;

            var whichBuilding = new NativeItem("Pour quel immeuble ?");
            appart.Add(whichBuilding);
            var addressBuilding = "";

            whichBuilding.Activated += (sender, e) =>
            {
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                var closestBuilding = GetClosestBuilding(playerCoords);

                if (closestBuilding != null)
                {
                    whichBuilding.AltTitle = closestBuilding.Address;
                    addressBuilding = closestBuilding.Address;
                    Debug.WriteLine("Le bâtiment le plus proche est" + closestBuilding.Address);
                }
                else
                {
                    Debug.WriteLine("Aucun bâtiment trouvé à proximité.");
                }
            };

            var myAppart = new Appartment(new Vector3(), new Vector3(), new Vector3());

            var appartStyle = new NativeListItem<string>("Type de l'appart", "", "Luxe niv. 1", "Luxe niv. 2", "Luxe niv. 3");
            appart.Add(appartStyle);

            appartStyle.Activated += (sender, e) =>
            {
                if (appartStyle.SelectedItem == "Luxe niv. 1")
                {
                    myAppart = Client.Appartment1;
                    myAppart.Decorations = new List<Decoration>()
                    {
                        new Decoration("", new Vector3(-28.4f, -595.98f, 79.2f))
                    };
                }
                else if (appartStyle.SelectedItem == "Luxe niv. 2")
                {
                    myAppart = Client.Appartment2;
                    myAppart.Decorations = new List<Decoration>()
                    {
                        new Decoration("", new Vector3(-478.27f, -690, 52.6f))
                    };
                }
                else if (appartStyle.SelectedItem == "Luxe niv. 3")
                {
                    myAppart = Client.Appartment3;
                    myAppart.Decorations = new List<Decoration>()
                    {
                        new Decoration("", new Vector3(-582.19f, -709.41f, 113)),
                        new Decoration("", new Vector3(-584.3f, -715, 112.12f)),
                        new Decoration("", new Vector3(-591.24f, -712.89f, 112.56f)),
                    };
                }
                Client.SendNotif("L'appartement a bien été choisi");
            };

            var visit = new NativeItem("Visiter l'appartement", "Téléporte également le joueur le plus proche");
            appart.Add(visit);
            visit.Activated += (sender, e) =>
            {
                Dictionary<string, Appartment> stylesToAppartments = new Dictionary<string, Appartment>
                {
                    { "Luxe niv. 1", Client.Appartment1 },
                    { "Luxe niv. 2", Client.Appartment2 },
                    { "Luxe niv. 3", Client.Appartment3 }
                };

                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());

                if (stylesToAppartments.TryGetValue(appartStyle.SelectedItem, out Appartment selectedAppartment))
                {
                    Game.Player.Character.Position = selectedAppartment.Interior;
                    var interior = selectedAppartment.Interior;
                    if (playerCoords.DistanceToSquared(playerTarget.Position) < 10)
                    {
                        BaseScript.TriggerServerEvent("core:bringServer", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)), interior.X, interior.Y, interior.Z);
                    }
                }
            };

            var submitAppart = new NativeItem("Enregistrer l'appartement", "Rapprocher vous du joueur");
            appart.Add(submitAppart);
            myAppart.Decorations = new List<Decoration>();
            submitAppart.Activated += (sender, args) =>
            {
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                BaseScript.TriggerServerEvent("legal_server:addAppart", addressBuilding, JsonConvert.SerializeObject(myAppart));
            };
        }

        public Building GetClosestBuilding(Vector3 playerCoords)
        {
            Building closestBuilding = null;
            float closestDistance = 10;

            foreach (var building in Client.Buildings)
            {
                float distance = playerCoords.DistanceToSquared(building.Door);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBuilding = building;
                }
            }

            return closestBuilding;
        }

        public override void Ticked()
        {
            
        }
    }
}
