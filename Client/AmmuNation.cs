using CitizenFX.Core;
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
    public class AmmuNation
    {
        public ClientMain Client;
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;
        public Vector3 Vendeur = new Vector3(22, -1105, (float)28.7);
        public AmmuNation(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
        }
        public void GunShop()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            var distance = GetDistanceBetweenCoords(Vendeur.X, Vendeur.Y, Vendeur.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);
            if (distance < 4)
            {
                Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir le catalogue");
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

                    foreach (KeyValuePair<string, Dictionary<WeaponHash, int>> weapon in weapon_dict)
                    {
                        foreach (var kvp2 in weapon.Value)
                        {
                            var model = new Model(kvp2.Key);
                            var item = new NativeItem(weapon.Key, "", $"~g~{kvp2.Value}$");
                            menu.Add(item);
                            item.Activated += (sender, e) =>
                            {
                                BaseScript.TriggerServerEvent("core:getPlayerMoney");
                                if (Client.PlayerMoney >= kvp2.Value)
                                {
                                    Format.SendNotif($"~g~Vous avez bien acheté {weapon.Key}");
                                    BaseScript.TriggerServerEvent("core:transaction", kvp2.Value);
                                    BaseScript.TriggerServerEvent("core:addWeapon", kvp2.Key.ToString());
                                }
                                else
                                {
                                    Format.SendNotif("~r~Vous n'avez pas assez d'argent.");
                                }
                            };
                            menu.Visible = true;
                            menu.UseMouse = false;
                        }
                    }
                }
            }
        }

    }
}
