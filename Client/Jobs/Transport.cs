using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using LemonUI.Menus;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using ShurikenLegal.Shared;

namespace ShurikenLegal.Client.Jobs
{
    public class Transport : Job
    {
        public Vector3 garagePosSortie = new Vector3(51.8f, 109.2f, 79);
        public Vector3 garagePosEntrer = new Vector3(64, 115.5f, 79.5f);

        public Vector3 coffreEntreprise = new Vector3(49.8f, 115.3f, 79.5f);
        public Vector3 stockageCoords = new Vector3(49.8f, 120.3f, 79.5f);
        public Vector3 clothPos = new Vector3(54.8f, 116, 79.3f);

        private bool is_working = false;
        ClientMain Main;
        public Transport(ClientMain caller) : base(caller)
        {
            Main = caller;
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 4,
                JobName = "Transport",
                MenuTitle = "LS Transport",

                // Positions
                PosCoffreEntreprise = coffreEntreprise,
                PosGarageSortie = garagePosSortie,
                PosGarageEntrer = garagePosEntrer,
                PosCloth = clothPos,

                Outfits = new Dictionary<string, ClothingSet>
                {
                    ["Chauffeur"] = new ClothingSet
                    {
                        Components = new Dictionary<int, ComponentVariation>
                        {
                            [8] = new ComponentVariation { ComponentId = 8, DrawableId = 153, TextureId = 0 },
                            [11] = new ComponentVariation { ComponentId = 11, DrawableId = 242, TextureId = 1 },
                            [4] = new ComponentVariation { ComponentId = 4, DrawableId = 25, TextureId = 5 },
                            [6] = new ComponentVariation { ComponentId = 6, DrawableId = 10, TextureId = 0 },
                            [3] = new ComponentVariation { ComponentId = 3, DrawableId = 0, TextureId = 0 }
                        }
                    },
                },

                AvailableVehicles = new List<string>
                {
                    "pounder",
                    "mule",
                    "phantom"
                },
            };
        }

        private List<WorksList> WorksList = new List<WorksList>()
        {
            new WorksList(
                "BurgerShot",
                1000,
                "Marchandise à emmener : \n - 50kg Steak \n - 30kg Pain Burger \n - 80L Bouteilles d'eau Cristaline",
                new Vector3(-1181.9f, -873.6f, 13.9f)
            ),
            new WorksList(
                "Benny's",
                1500,
                "Marchandise à emmener : \n - 10 Roues \n - 15 Repair-kits",
                new Vector3(-212, -1327, 30)
            )
        };

        public override void ShowMenu()
        {
            var menu = new NativeMenu("LS Transport", "Menu intéraction");
            menu.TitleFont = CitizenFX.Core.UI.Font.ChaletLondon;
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;

            var facture = new NativeItem("Facture");
            menu.Add(facture);

            facture.Activated += async (sender, e) =>
            {
                var textInput = await GetUserInput("Quantité", "", 20);
                if (int.TryParse(textInput, out var parsedInput))
                {
                    Main.SendBill("Transport", parsedInput, Main.GetPlayer().Name);
                }
            };

            var tasks = new NativeMenu("Vos tâches", "Vos tâches");
            Pool.Add(tasks);
            tasks.UseMouse = false;
            tasks.TitleFont = CitizenFX.Core.UI.Font.ChaletLondon;
            menu.AddSubMenu(tasks);

            foreach (var work in WorksList)
            {
                var task = new NativeItem($"{work.Company}", $"{work.Detail}", $"~g~{work.Price}$");
                task.Colors.TitleHovered = Color.FromArgb(255, 106, 31);
                tasks.Add(task);

                task.Activated += (sender, e) =>
                {
                    is_working = true;
                    Main.SendNotif($"~g~Le GPS a bien été activé au <b>{work.Company}</b>");
                    SetNewWaypoint(work.Coords.X, work.Coords.Y);
                    tasks.Visible = false;
                };
            }

            var setPackages = new NativeMenu("Passer une commande", "Passer une commande");
            Pool.Add(setPackages);
            menu.AddSubMenu(setPackages);
            setPackages.UseMouse = false;

            var item = new NativeItem("Que souhaitez vous commander ?", "Choisissez ce que vous voulez commander et la quantité");
            setPackages.Add(item);

            item.Activated += async (sender, e) =>
            {
                var textInput = await GetUserInput("Burger, Cola, Repair-kit, Roue...", "", 20);
                item.AltTitle = textInput;
            };

            var quantity = new NativeItem("Quantité");
            setPackages.Add(quantity);

            quantity.Activated += async (sender, e) =>
            {
                var textInput = await GetUserInput("Veuillez saisir la quantité", "", 5);
                quantity.AltTitle = $"{textInput}";
            };

            var submit = new NativeItem("Envoyer");
            setPackages.Add(submit);

            submit.Activated += (sender, e) =>
            {
                if (item.AltTitle != null && quantity.AltTitle != null)
                {
                    SelectedItem(item.AltTitle, quantity.AltTitle);
                    setPackages.Visible = false;
                }
                else
                {
                    Main.SendNotif("~r~Une entrée est vide");
                }
            };
        }

        public void SelectedItem(string item, string quantity)
        {
            HashSet<string> generalItems = new HashSet<string>()
            {
                "Pain",
                "Eau",
                "Burger",
                "Roue",
                "Repair-kit",
                "Café",
                "Cappuccino",
                "Thé au na3na3",
                "Expresso",
                "Diabolo fraise"
            };

            HashSet<string> weapons = new HashSet<string>()
            {
                "Munition",
                "pistol",
                "combatpistol",
                "appistol",
                "stungun",
                "pistol50",
                "snspistol",
                "heavypistol",
                "vintagepistol",
                "marksmanpistol",
                "revolver",
                "doubleaction",
                "ceramicpistol",
                "navyrevolver",
                "pistolxm3",
                "microsmg",
                "smg",
                "assaultsmg",
                "combatpdw",
                "machinepistol",
                "minismg",
                "tecpistol",
                "pumpshotgun",
                "sawnoffshotgun",
                "assaultshotgun",
                "bullpupshotgun",
                "musket",
                "heavyshotgun",
                "dbshotgun",
                "autoshotgun",
                "combatshotgun",
                "assaultrifle",
                "compactrifle",
                "militaryrifle",
                "heavyrifle",
                "tacticalrifle",
                "gusenberg",
                "combatmg",
                "mg",
                "sniperrifle",
                "heavysniper",
                "marksmanrifle",
                "precisionrifle",
                "bzgas",
                "smokegrenade",
                "petrolcan"
            };

            string Type;

            if (generalItems.Contains(item))
            {
                Type = "item";
            }
            else if (weapons.Contains(item))
            {
                Type = "weapon";
            }
            else
            {
                Main.SendNotif("~r~Article non reconnu");
                return;
            }

            BaseScript.TriggerServerEvent("legal_server:ordering", item, quantity, Type);
        }

        public override void Ticked()
        {
            base.Ticked();

            if (is_working)
            {
                foreach (var work in WorksList)
                {
                    var player_coords = GetEntityCoords(PlayerPedId(), false);
                    var destination = work.Coords;
                    float dist = player_coords.DistanceToSquared(destination);

                    if (dist < 8)
                    {
                        Main.SendNotif("~g~Vous avez reçu 1000$");
                        is_working = false;
                        break;
                    }
                }
            }
        }
    }

    public class WorksList
    {
        public string Company { get; set; }
        public int Price { get; set; }
        public string Detail { get; set; }
        public Vector3 Coords { get; set; }

        public WorksList(string company, int price, string detail, Vector3 coords)
        {
            Company = company;
            Price = price;
            Detail = detail;
            Coords = coords;
        }
    }
}