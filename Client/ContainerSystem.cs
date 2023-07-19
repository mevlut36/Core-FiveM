using CitizenFX.Core;
using CitizenFX.Core.UI;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class ContainerSystem
    {
        Format Format;
        ObjectPool Pool = new ObjectPool();

        Vector3 Point = new Vector3(259.5f, -782.9f, 30.2f);
        Vector3 OutPoint = new Vector3(238.6f, -772.1f, 30.7f);
        bool State = false;
        public ContainerSystem(ClientMain caller)
        {
            Pool = caller.Pool;
            Format = caller.Format;
        }

        public void ContainerInterface()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            
            Format.SetMarker(Point, MarkerType.ChevronUpx1);
            var menu = new NativeMenu("Container", "Container")
            {
                UseMouse = false
            };
            Pool.Add(menu);

            var containerID = 1765283457;
            if (playerCoords.DistanceToSquared2D(Point) < 5)
            {
                Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                if (IsControlPressed(0, 38))
                {
                    menu.Visible = true;

                    var outContainer = new NativeItem("Sortir un container");
                    menu.Add(outContainer);
                    outContainer.Activated += (sender, e) =>
                    {
                        State = true;
                        menu.Visible = false;
                        RequestModel((uint)containerID);
                        CreateObjectNoOffset((uint)containerID, OutPoint.X, OutPoint.Y, OutPoint.Z, true, false, true);
                        PlaceObjectOnGroundProperly(containerID);
                        FreezeEntityPosition(containerID, true);
                        SetModelAsNoLongerNeeded((uint)containerID);
                    };
                }
            }
        }

        public void OnTick()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            ContainerInterface();
            var closestProp = World.GetClosest(GetEntityCoords(GetPlayerPed(-1), true), World.GetAllProps());
            if (State && closestProp.Model.Hash == 1765283457)
            {
                if (closestProp.Position.DistanceToSquared2D(playerCoords) <= 6)
                {
                    Format.SetMarker(Point, MarkerType.ChevronUpx1);
                    var menu = new NativeMenu("Container", "Container")
                    {
                        UseMouse = false
                    };
                    Pool.Add(menu);
                    var openContainer = new NativeMenu("Votre container", "Votre container")
                    {
                        UseMouse = false
                    };
                    Pool.Add(openContainer);
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir");

                    if (IsControlPressed(0, 38))
                    {
                        menu.Visible = true;

                        var outContainer = new NativeItem("Ranger le container");
                        menu.Add(outContainer);

                        for (int i = 0; i < 5; i++)
                        {
                            openContainer.Add(new NativeItem($"Item N°{i}"));
                        }
                        menu.AddSubMenu(openContainer);
                        outContainer.Activated += (sender, e) =>
                        {
                            closestProp.Delete();
                            State = false;
                            menu.Visible = false;
                        };
                    }
                }
            }
        }
    }
}
