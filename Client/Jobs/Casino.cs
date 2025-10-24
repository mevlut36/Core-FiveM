using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using LemonUI.Menus;
using Mono.CSharp.Linq;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using ShurikenLegal.Shared;
using static ShurikenLegal.Client.ClientMain;

namespace ShurikenLegal.Client.Jobs
{
    public class Casino : Job
    {
        public Job Metier;
        public ClientMain Client;

        public Vector3 garagePosSortie = new Vector3(975, 12.5f, 81);
        public Vector3 garagePosEntrer = new Vector3(975.1f, 6.4f, 81);

        public Vector3 coffreEntreprise = new Vector3(1110.3f, 207.4f, -49.4f);
        public Vector3 clothPos = new Vector3(1096.3f, 200.9f, -49.4f);

        public Casino(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 7,
                JobName = "Casino",
                MenuTitle = "Casino",
                PosCoffreEntreprise = coffreEntreprise,
                PosCloth = clothPos,
                PosGarageSortie = garagePosSortie,
                PosGarageEntrer = garagePosEntrer,

                AvailableVehicles = new List<string> { "stretch", "stafford", "superd", "mule", "cavalcade" },

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
