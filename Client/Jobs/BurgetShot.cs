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
    public class BurgerShot : Job
    {
        public Job Metier;
        public ClientMain Client;

        public Vector3 garagePosSortie = new Vector3(-1171, -900.7f, 13.8f);
        public Vector3 garagePosEntrer = new Vector3(-1174.6f, -900.6f, 13.1f);

        public Vector3 coffreEntreprise = new Vector3(-1196.2f, -892.8f, 13.9f);
        public Vector3 clothPos = new Vector3(-1204.2f, -892.3f, 13.4f);

        bool is_working = false;

        public BurgerShot(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 5,
                JobName = "BurgerShot",
                MenuTitle = "BurgerShot",
                PosCoffreEntreprise = coffreEntreprise,
                PosCloth = clothPos,
                PosGarageEntrer = garagePosEntrer,
                PosGarageSortie = garagePosSortie,
                AvailableVehicles = new List<string>
                {
                    "stalion2", "mule"
                },
                Doors = new List<DoorClass>
                {
                    new DoorClass { DoorHash = 1, ModelHash = 167687243, Coordinates = new Vector3(-1199.033f, -885.1699f, 14.25259f) },
                    new DoorClass { DoorHash = 2, ModelHash = 167687243, Coordinates = new Vector3(-1184.892f, -883.3377f, 14.25113f) },
                    new DoorClass { DoorHash = 3, ModelHash = 1800304361, Coordinates = new Vector3(21196.539f, -883.4852f, 14.25259f) },
                    new DoorClass { DoorHash = 4, ModelHash = (uint)GetHashKey("p_bs_map_door_01_s"), Coordinates = new Vector3(-1179.327f, -891.4769f, 14.05767f) }
                },

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

        public override void Ticked()
        {
            if (is_working)
            {
                
            }
        }

    }

}
