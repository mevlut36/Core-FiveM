using System.Collections.Generic;
using CitizenFX.Core;

namespace ShurikenLegal.Client.Jobs
{
    public class Taquila : Job
    {
        public Job Metier;
        public ClientMain Client;

        public Vector3 garagePosSortie = new Vector3(-564.9f, 297.8f, 83);
        public Vector3 garagePosEntrer = new Vector3(-564.6f, 302.4f, 83.1f);

        public Vector3 coffreEntreprise = new Vector3(-561.9f, 286.2f, 82.1f);
        public Vector3 clothPos = new Vector3(-561.8f, 289.4f, 82.1f);

        bool is_working = false;

        public Taquila(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
        }
        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 8,
                JobName = "Taquila-la",
                MenuTitle = "Taquila-la",
                Outfits = new Dictionary<string, ClothingSet>
                {
                    ["Tenue de service"] = new ClothingSet
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
                    "stafford",
                    "superd",
                    "mule",
                    "cavalcade"
                },
            };
        }
    }

}
