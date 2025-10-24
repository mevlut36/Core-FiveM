using System.Collections.Generic;
using CitizenFX.Core;

namespace ShurikenLegal.Client.Jobs
{
    public class CoffeeShop : Job
    {
        public Job Metier;
        public ClientMain Client;

        public Vector3 garagePosSortie = new Vector3(315.35f, -1100.8f, 28.6f);
        public Vector3 garagePosEntrer = new Vector3(315, -1105, 29);

        public Vector3 coffreEntreprise = new Vector3(321.78f, -1094.5f, 28.5f);
        public Vector3 clothPos = new Vector3(322.2f, -1101.7f, 28.6f);

        bool is_working = false;

        public CoffeeShop(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 6,
                JobName = "CoffeeShop",
                MenuTitle = "CoffeeShop",

                AvailableVehicles = new List<string> { "windsor", "mule", "cavalcade" },

                Outfits = new Dictionary<string, ClothingSet>
                {
                    ["Tenue de travail"] = new ClothingSet
                    {
                        Components = new Dictionary<int, ComponentVariation>
                        {
                            [8] = new ComponentVariation { ComponentId = 8, DrawableId = 153, TextureId = 0 },
                            [11] = new ComponentVariation { ComponentId = 11, DrawableId = 242, TextureId = 0 },
                            [4] = new ComponentVariation { ComponentId = 4, DrawableId = 25, TextureId = 0 },
                            [6] = new ComponentVariation { ComponentId = 6, DrawableId = 10, TextureId = 0 },
                            [3] = new ComponentVariation { ComponentId = 3, DrawableId = 0, TextureId = 0 }
                        }
                    },
                },
            };
        }
    }
}
