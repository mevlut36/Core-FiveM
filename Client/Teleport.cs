using CitizenFX.Core;
using LemonUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class Teleport
    {
        Format Format;
        
        List<TeleportInfo> Teleports = new List<TeleportInfo>();

        public Teleport(ClientMain caller)
        {
            Format = caller.Format;

            TeleportInfo Bahamas = new TeleportInfo(new Vector3(-1388.6f, -586.4f, 30.2f), new Vector3(-1387.2f, -588.4f, 30f));
            Teleports.Add(Bahamas);

            TeleportInfo GarageDrift = new TeleportInfo(new Vector3(259.7f, -783, 30.0f), new Vector3(-2214.1f, 1072.7f, -23.8f));
            Teleports.Add(GarageDrift);

            TeleportInfo LaboWeed = new TeleportInfo(new Vector3(-438.5f, -2183.7f, 9.6f), new Vector3(1066.1f, -3183.27f, -39.7f));
            Teleports.Add(LaboWeed);

            TeleportInfo LaboCoke = new TeleportInfo(new Vector3(930.2f, -1584.2f, 29.8f), new Vector3(1088.6f, -3188.1f, -39.5f));
            Teleports.Add(LaboCoke);

            TeleportInfo LaboMeth = new TeleportInfo(new Vector3(-1335.8f, -226.3f, 42.4f), new Vector3(970, -147, -46.8f));
            Teleports.Add(LaboMeth);
            
            TeleportInfo Casino = new TeleportInfo(new Vector3(935.5f, 46.6f, 80.4f), new Vector3(1089.9f, 206.3f, -49.5f));
            Teleports.Add(Casino);
            
            TeleportInfo DriftParking = new TeleportInfo(new Vector3(-2153.5f, 1105.6f, -24.4f), new Vector3(-2142, 1106.1f, -25.5f));
            Teleports.Add(DriftParking);
        }

        public void OnTick()
        {
            var player = GetPlayerPed(-1);

            foreach(TeleportInfo info in Teleports)
            {
                if (info != null)
                {
                    if (GetEntityCoords(player, true).DistanceToSquared(info.Enter) < 5)
                    {
                        Format.SendTextUI("Appuyer sur ~r~E~w~ pour rentrer");
                        World.DrawMarker(MarkerType.VerticleCircle, info.Enter, new Vector3(0, 0, 0), new Vector3(90, 90, 90), new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(100, 204, 0, 0));
                        
                        if (IsControlJustPressed(0, 38))
                        {
                            if (IsPedInAnyVehicle(PlayerPedId(), false))
                            {
                                SetPedCoordsKeepVehicle(PlayerPedId(), info.Exit.X, info.Exit.Y, info.Exit.Z);
                            }
                            else
                            {
                                Game.PlayerPed.Position = info.Exit;
                            }
                        }
                        
                    }
                    else if (GetEntityCoords(player, true).DistanceToSquared(info.Exit) < 5)
                    {
                        Format.SendTextUI("Appuyer sur ~r~E~w~ pour sortir");
                        World.DrawMarker(MarkerType.VerticleCircle, info.Exit, new Vector3(0, 0, 0), new Vector3(90, 90, 90), new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(100, 204, 0, 0));
                        
                        if (IsControlJustPressed(0, 38))
                        {
                            if (IsPedInAnyVehicle(PlayerPedId(), false))
                            {
                                SetPedCoordsKeepVehicle(PlayerPedId(), info.Enter.X, info.Enter.Y, info.Enter.Z);
                            } else
                            {
                                Game.PlayerPed.Position = info.Enter;
                            }
                        }
                    }
                }
            }
        }
    }

    class TeleportInfo
    {
        public Vector3 Enter;
        public Vector3 Exit;

        public TeleportInfo(Vector3 enter, Vector3 exit)
        {
            Enter = enter;
            Exit = exit;
        }
    }
}
