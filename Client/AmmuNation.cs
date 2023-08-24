using CitizenFX.Core;
using LemonUI;
using LemonUI.Elements;
using LemonUI.Menus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Core.Shared;

namespace Core.Client
{
    public class AmmuNation
    {
        public ClientMain Client;
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;
        PlayerMenu PlayerMenu;
        List <AmmuNationInfo> AmmuNationList = new List<AmmuNationInfo>();
        public Vector3 Vendeur = new Vector3(22, -1105, (float)28.7);
        public AmmuNation(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            PlayerMenu = caller.PlayerMenu;

            AmmuNationInfo ltd1 = new AmmuNationInfo("Pillbox Hill", new Vector3(21.2f, -1104.7f, 29.6f), new Vector3(21.2f, -1103.5f, 29.5f));
            AmmuNationList.Add(ltd1);
            AmmuNationInfo ltd2 = new AmmuNationInfo("La Mesa", new Vector3(842.5f, -1033.1f, 28.1f), new Vector3(842.5f, -1035.6f, 28));
            AmmuNationList.Add(ltd2);
            AmmuNationInfo ltd3 = new AmmuNationInfo("Hawick", new Vector3(252, -49.6f, 69.8f), new Vector3(254.1f, -50.3f, 69.8f));
            AmmuNationList.Add(ltd3);
            AmmuNationInfo ltd4 = new AmmuNationInfo("Morningwood", new Vector3(-1305.9f, -394.2f, 36.6f), new Vector3(-1303.7f, -394.2f, 36.6f));
            AmmuNationList.Add(ltd4);
            AmmuNationInfo ltd5 = new AmmuNationInfo("Little Seoul", new Vector3(-662.4f, -935.3f, 29.7f), new Vector3(-662.4f, -933.2f, 29.7f));
            AmmuNationList.Add(ltd5);
            AmmuNationInfo ltd6 = new AmmuNationInfo("Monts Tataviam", new Vector3(2568.1f, 294.6f, 108.7f), new Vector3(2567.9f, 292.1f, 108.7f));
            AmmuNationList.Add(ltd6);
            AmmuNationInfo ltd7 = new AmmuNationInfo("Paleto Bay", new Vector3(-330.4f, 6083.6f, 31.4f), new Vector3(-331.8f, 6085.2f, 31.4f));
            AmmuNationList.Add(ltd7);
            AmmuNationInfo ltd8 = new AmmuNationInfo("Sandy Shores", new Vector3(1693.6f, 3759.6f, 34.7f), new Vector3(1691.9f, 3759.6f, 34.7f));
            AmmuNationList.Add(ltd8);
            AmmuNationInfo ltd9 = new AmmuNationInfo("Chumash", new Vector3(-3171.6f, 1087.3f, 20.7f), new Vector3(-3173.7f, 1088.4f, 20.7f));
            AmmuNationList.Add(ltd9);
            foreach (var ammuNation in AmmuNationList)
            {
                Blip myBlip = World.CreateBlip(ammuNation.Checkout);
                myBlip.Sprite = BlipSprite.AmmuNation;
                myBlip.Color = BlipColor.Red;
                myBlip.Name = "AmmuNation";
                myBlip.IsShortRange = true;
            }
        }
        public void GunShop()
        {
            var playerCoords = GetEntityCoords(PlayerPedId(), false);
            foreach (var ammuNation in AmmuNationList)
            {
                var distance = GetDistanceBetweenCoords(ammuNation.Checkout.X, ammuNation.Checkout.Y, ammuNation.Checkout.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, false);
                if (distance < 4)
                {
                    Format.SendTextUI("~w~Cliquer sur ~r~E ~w~ pour ouvrir le catalogue");
                    if (IsControlPressed(0, 38))
                    {
                        var menu = new NativeMenu("Ammu-Nation", "Acheter vos armes")
                        {
                            UseMouse = false
                        };
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
                                    if (PlayerMenu.PlayerInst.Money >= kvp2.Value)
                                    {
                                        Format.ShowAdvancedNotification("ShurikenRP", "AmmuNation", $"~g~Vous avez bien acheté {weapon.Key}");
                                        BaseScript.TriggerServerEvent("core:buyWeapon", kvp2.Key.ToString(), kvp2.Value);
                                    }
                                    else
                                    {
                                        Format.ShowAdvancedNotification("ShurikenRP", "AmmuNation", "~r~Vous n'avez pas assez d'argent.");
                                    }
                                };
                                menu.Visible = true;
                                menu.UseMouse = false;
                            }
                        }
                        var ammo = new NativeItem("Munitions", "", "~g~$200");
                        menu.Add(ammo);
                        var items = PlayerMenu.PlayerInst.Inventory;
                        ammo.Activated += async (sender, e) =>
                        {
                            var textInput = await Format.GetUserInput("Quantité", "1", 4);
                            var parsedInput = Int32.Parse(textInput);
                            var result = 200 * parsedInput;
                            if (result <= PlayerMenu.PlayerInst.Money)
                            {
                                PlayerMenu.PlayerInst.Money -= result;
                                PlayerMenu.PlayerInst.Inventory = items;
                                BaseScript.TriggerServerEvent("core:transaction", result, "Munitions", parsedInput, "item");
                                menu.Visible = false;
                            }
                            else
                            {
                                Format.ShowAdvancedNotification("ShurikenRP", "AmmuNation", "~r~La somme est trop élevée");
                            }
                        };
                    }
                }
            }
        }

    }
}
