using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using LemonUI.Menus;
using LemonUI;
using static CitizenFX.Core.Native.API;
using Game = CitizenFX.Core.Game;

namespace ShurikenLegal.Client.Jobs
{
    public class AmmuNation : Job
    {
        public Job Metier;
        public ClientMain Client;
        public Vector3 Vendeur = new Vector3(22, -1105, (float)28.7);
        public AmmuNation(ClientMain caller) : base(caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Client.ShopPnj(PedHash.Michael, Vendeur.X, Vendeur.Y, Vendeur.Z);
        }

        protected override JobConfig GetJobConfig()
        {
            return new JobConfig
            {

            };
        }

        public void GunShop()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var distance = GetDistanceBetweenCoords(Vendeur.X, Vendeur.Y, Vendeur.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);
            if (distance < 4)
            {
                Client.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir le catalogue");
                if (IsControlPressed(0, 38))
                {
                    var menu = new NativeMenu("Ammu-Nation", "Acheter vos armes");
                    Pool.Add(menu);
                    var weapon_dict = new Dictionary<string, Dictionary<WeaponHash, int>>();

                    weapon_dict.Add("Lampe de poche", new Dictionary<WeaponHash, int>());
                    weapon_dict["Lampe de poche"].Add(WeaponHash.Flashlight, 2000);

                    weapon_dict.Add("Batte de base-ball", new Dictionary<WeaponHash, int>());
                    weapon_dict["Batte de base-ball"].Add(WeaponHash.Bat, 2000);

                    weapon_dict.Add("Couteau", new Dictionary<WeaponHash, int>());
                    weapon_dict["Couteau"].Add(WeaponHash.Knife, 5000);

                    weapon_dict.Add("Machette", new Dictionary<WeaponHash, int>());
                    weapon_dict["Machette"].Add(WeaponHash.Machete, 7000);

                    weapon_dict.Add("Pétoire", new Dictionary<WeaponHash, int>());
                    weapon_dict["Pétoire"].Add(WeaponHash.SNSPistol, 80000);
                    
                    foreach (KeyValuePair<string, Dictionary<WeaponHash, int>> weapon in weapon_dict) {
                        foreach(var kvp2 in weapon.Value)
                        {
                            var model = new Model(kvp2.Key);
                            var item = new NativeItem(weapon.Key, "", $"~g~{kvp2.Value}$");
                            menu.Add(item);
                            item.Activated += (sender, e) =>
                            {
                                Game.PlayerPed.Weapons.Give(model, 0, false, false);
                            };
                            menu.Visible = true;
                            menu.UseMouse = false;
                        }
                    }
                }
            }
            
        }

        public override void Ticked()
        {
            GunShop();
        }
    }
}
