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
    public class Bahama : Job
    {
        public Job Metier;
        public ClientMain Client;

        public Vector3 garagePosSortie = new Vector3(-1372.7f, -597.6f, 29.4f);
        public Vector3 garagePosEntrer = new Vector3(-1374.4f, -590.4f, 29.7f);

        public Vector3 coffreEntreprise = new Vector3(-1391.3f, -605.5f, 29.6f);
        public Vector3 clothPos = new Vector3(-1387.9f, -609.1f, 29.7f);

        bool is_working = false;

        public Bahama(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {
                JobId = 6,
                JobName = "bahama",
                MenuTitle = "Bahama Mamas",
                PosCoffreEntreprise = coffreEntreprise,
                PosGestionEntreprise = new Vector3(-1392.1f, -606.0f, 29.6f),
                PosCloth = clothPos,
                PosGarageSortie = garagePosSortie,
                PosGarageEntrer = garagePosEntrer,
                AvailableVehicles = new List<string>
                {
                    "taxi",
                    "taxi2"
                },

            };
        }

    }

}
