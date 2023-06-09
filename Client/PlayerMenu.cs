﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
using LemonUI.Menus;
using Mono.CSharp;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class PlayerMenu
    {
        Format Format;
        ClientMain Client;
        ObjectPool Pool;
        Parking Parking;
        NoClip noClipInstance = new NoClip();
        
        public PlayerMenu(ClientMain caller)
        {
            Client = caller;
            Pool = caller.Pool;
            Format = caller.Format;
            Parking = caller.Parking;
            BaseScript.TriggerServerEvent("core:getPlayersList");
            Client.AddEvent("core:receivePlayers", new Action<string, string>(OnReceivePlayers));
        }

        public PlayerInstance PlayerInst = new PlayerInstance
        {
            Id = 0,
            Firstname = "",
            Lastname = "",
            Rank = "",
            Bitcoin = 0,
            Birth = "",
            Clothes = "",
            ClothesList = "",
            Money = 0,
            Bills = "",
            Inventory = ""
        };

        public void F5Menu()
        {
            if (IsControlPressed(0, 166))
            {
                var PlayerPos = GetEntityCoords(PlayerPedId(), true);
                var MainMenu = new NativeMenu("F5", "Menu personnel")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    Visible = true,
                    UseMouse = false
                };
                if (PlayerInst.Rank == "staff")
                {
                    BaseScript.TriggerServerEvent("core:getPlayersList");
                    var adminMenu = new NativeMenu("Administration", "~o~Administration")
                    {
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                        UseMouse = false
                    };
                    Pool.Add(adminMenu);
                    MainMenu.AddSubMenu(adminMenu);
                    var playerList = new NativeMenu("Liste des joueurs", "Liste des joueurs")
                    {
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                        UseMouse = false
                    };
                    Pool.Add(playerList);
                    for (int i = 0; i < Client.PlayersInstList.Count; i++)
                    {
                        var playerInst = Client.PlayersInstList[i];
                        var handle = Client.PlayersHandle[i];

                        var targetPlayer = GetPlayerPed(GetPlayerFromServerId(int.Parse(handle)));

                        var job = JsonConvert.DeserializeObject<JobInfo>(playerInst.Job);
                        var playerMenu = new NativeMenu($"", $"{handle} | {playerInst.Firstname} {playerInst.Lastname}",
                        $"ID: {handle}\nDB ID: {playerInst.Id}\nJob: {job.JobID} | {job.JobRank}\nOrga: {playerInst.Organisation}\nRank: {playerInst.Rank}\nBitcoin: {playerInst.Bitcoin}\nMoney: {playerInst.Money}")
                        {
                            TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                            UseMouse = false
                        };
                        var reviveP = new NativeItem("Revive", "Réanimer le joueur");
                        reviveP.Activated += (sender, e) =>
                        {
                            BaseScript.TriggerServerEvent("core:heal", handle, 200);
                        };
                        var jail = new NativeItem("Jail", "Jail une personne pendant un certain temps");
                        jail.Activated += async (sender, e) =>
                        {
                            var input = await Format.GetUserInput("Durée", "", 12);
                            var parsedInput = Int32.Parse(input);
                            Client.SetJail(handle, "Troll", parsedInput);
                        };
                        var spec = new NativeItem("Téléporter à lui", "Guette le joueur");
                        spec.Activated += (sender, e) =>
                        {
                            var targetPCoords = GetEntityCoords(targetPlayer, true);
                            BaseScript.TriggerServerEvent("core:goto", handle);
                        };
                        var tpHim = new NativeItem("Téléporter sur soi", "TP sur soi");
                        tpHim.Activated += (sender, e) =>
                        {
                            var pCoords = GetEntityCoords(GetPlayerPed(-1), true);
                            BaseScript.TriggerServerEvent("core:bringServer", handle, pCoords.X, pCoords.Y, pCoords.Z);
                        };
                        var warn = new NativeItem("Warn / Avertir", "Avertir le joueur");
                        warn.Activated += async (sender, e) =>
                        {
                            var input = await Format.GetUserInput("Message", "", 12);
                            BaseScript.TriggerServerEvent("core:warn", handle, input);
                        };
                        var kick = new NativeItem("Kick / Expulser", "Expulser le joueur");
                        kick.Activated += async (sender, e) =>
                        {
                            var input = await Format.GetUserInput("Message", "", 12);
                            BaseScript.TriggerServerEvent("core:kick", handle, input);
                        };
                        var ban = new NativeItem("Bannir", "Bannir le joueur");
                        ban.Activated += async (sender, e) =>
                        {
                            var inputMessage = await Format.GetUserInput("Message", "", 12);
                            var inputTime = await Format.GetUserInput("Temps (en min.)", "", 12);
                            var banTime = int.Parse(inputTime);
                            BaseScript.TriggerServerEvent("core:TempBanPlayer", handle, banTime, inputMessage);
                        };

                        playerMenu.Add(reviveP);
                        playerMenu.Add(jail);
                        playerMenu.Add(spec);
                        playerMenu.Add(tpHim);
                        playerMenu.Add(warn);
                        playerMenu.Add(kick);
                        playerMenu.Add(ban);

                        Pool.Add(playerMenu);
                        playerList.AddSubMenu(playerMenu);
                    }
                    adminMenu.AddSubMenu(playerList);
                    var reportList = new NativeMenu("Liste des reports", "Liste des reports")
                    {
                        TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                        UseMouse = false
                    };
                    Pool.Add(reportList);
                    adminMenu.AddSubMenu(reportList);

                    var unresolvedReports = Client.reportSystem.GetUnresolvedReports();
                    foreach (var report in unresolvedReports)
                    {
                        Debug.WriteLine($"{report.Text}");
                        reportList.Add(new NativeItem($"{report.Text}"));
                    }

                    var showName = new NativeCheckboxItem("Afficher les noms", false);
                    adminMenu.Add(showName);

                    var showNameState = false;
                    showName.Activated += (sender, e) =>
                    {
                        for (int i = 0; i < Client.PlayersInstList.Count; i++)
                        {
                            var playerInst = Client.PlayersInstList[i];
                            var handle = Client.PlayersHandle[i];
                            showNameState = !showNameState;
                            var targetPlayer = GetPlayerPed(GetPlayerFromServerId(int.Parse(handle)));
                            int gamerTagID = CreateMpGamerTag(targetPlayer, $"[{handle}] {playerInst.Firstname} {playerInst.Lastname}", true, false, "test", 1);
                            if (showNameState)
                            {
                                SetMpGamerTagVisibility(gamerTagID, 0, true);
                            }
                            else
                            {
                                SetMpGamerTagVisibility(gamerTagID, 0, false);
                            }
                        }
                    };

                    var noClip = new NativeCheckboxItem("NoClip", false);
                    adminMenu.Add(noClip);

                    noClip.Activated += (sender, e) =>
                    {
                        noClipInstance.SetNoclipActive(!noClipInstance.IsNoclipActive());
                    };

                    var revive = new NativeItem("Revive");
                    adminMenu.Add(revive);
                    revive.Activated += (sender, e) =>
                    {
                        var vector2Pos = new Vector2(PlayerPos.X, PlayerPos.Y);
                        var closestPed = World.GetClosest(vector2Pos, World.GetAllPeds());
                        SetEntityHealth(closestPed.Handle, 200);
                        Client.IsDead = false;
                        SetEntityHealth(GetPlayerPed(-1), 200);
                        ClearPedTasksImmediately(GetPlayerPed(-1));
                    };

                    var car = new NativeItem("Spawn car", "Faire apparaître un véhicule");
                    adminMenu.Add(car);
                    car.Activated += async (sender, e) =>
                    {
                        var input = await Format.GetUserInput("Nom de la voiture", "", 20);
                        var model = "twingo";
                        if (input != null)
                        {
                            model = input;
                        }
                        var hash = (uint)GetHashKey(model);
                        if (!IsModelInCdimage(hash) || !IsModelAVehicle(hash))
                        {
                            Format.SendNotif("~r~Votre véhicule n'existe pas");
                            return;
                        }
                        var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
                        Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                        Format.SendNotif("~g~Votre véhicule est apparu");
                    };

                    var repair = new NativeItem("Réparer un véhicule");
                    adminMenu.Add(repair);
                    repair.Activated += (sender, e) =>
                    {
                        if (IsPedInAnyVehicle(GetPlayerPed(-1), false))
                        {
                            var vehicle = GetVehiclePedIsUsing(GetPlayerPed(-1));
                            SetVehicleEngineHealth(vehicle, 100);
                            SetVehicleEngineOn(vehicle, true, true, true);
                            SetVehicleFixed(vehicle);
                            Format.SendNotif("~g~Vous avez bien réparé le véhicule");
                        }
                        else
                        {
                            Format.SendNotif("~r~Vous devez être dans un véhicule");
                        }
                    };

                    var delCar = new NativeItem("Supprimer un véhicule", "Met toi dans la voiture ou à coté pour la supprimer");
                    adminMenu.Add(delCar);
                    delCar.Activated += (sender, e) =>
                    {
                        var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                        Vehicle closestVehicle = World.GetClosest<Vehicle>(playerCoords, World.GetAllVehicles());
                        if (playerCoords.DistanceToSquared(closestVehicle.Position) < 30)
                        {
                            closestVehicle.Delete();
                        }
                    };

                    var clearVehicleArea = new NativeItem("Supprimer les véhicules d'une zone", "Supprimer les véhicules d'une zone");
                    adminMenu.Add(clearVehicleArea);
                    clearVehicleArea.Activated += async (sender, e) =>
                    {
                        var input = await Format.GetUserInput("Combien de mètre", "", 5);
                        ClearAreaOfVehicles(PlayerPos.X, PlayerPos.Y, PlayerPos.Z, int.Parse(input), false, false, false, false, false);
                    };

                    var bomb = new NativeItem("Crash Bandicoot");
                    adminMenu.Add(bomb);
                    bomb.Activated += async (sender, e) =>
                    {
                        var size = await Format.GetUserInput("Taille", "", 5);
                        World.AddExplosion(new Vector3(216.8f, -810, 30.7f), ExplosionType.PetrolPump, int.Parse(size), 2);
                        World.AddExplosion(new Vector3(206.8f, -810, 30.7f), ExplosionType.PetrolPump, int.Parse(size), 2);
                    };
                }

                var shopMenu = new NativeMenu("Boutique", "~o~Boutique")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                shopMenu.Add(new NativeItem("[V.I.P] Aucun"));
                shopMenu.Add(new NativeItem($"Bitcoins: {PlayerInst.Bitcoin}"));

                var vip = new NativeMenu("VIP", "VIP")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var vip_1 = new NativeItem("~f~~h~VIP~h~", "", "~g~500~w~ Bitcoins /mois");
                var vip_2 = new NativeItem("~y~~h~VIP+~h~", "", "~g~700~w~ Bitcoins /mois");
                var vip_3 = new NativeItem("~q~~h~MVP~h~", "", "~g~1000~w~ Bitcoins /mois");
                vip.Add(vip_1);
                vip.Add(vip_2);
                vip.Add(vip_3);
                shopMenu.AddSubMenu(vip);

                var carImport = new NativeMenu("Véhicules import", "Véhicules import")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                VehicleImport[] values = (VehicleImport[])System.Enum.GetValues(typeof(VehicleImport));

                foreach (VehicleImport vehicle in values)
                {
                    DisplayAttribute displayAttribute = Format.GetDisplayAttribute(vehicle);
                    var vehicleItem = new NativeItem($"{displayAttribute.Name}", "", $"~y~{displayAttribute.Price} BTC");
                    carImport.Add(vehicleItem);
                    vehicleItem.Activated += async (sender, args) =>
                    {
                        if (PlayerInst.Bitcoin >= displayAttribute.Price)
                        {
                            BaseScript.TriggerServerEvent("core:bitcoinTransaction", displayAttribute.Price);
                            var vehicleImport = await World.CreateVehicle(new Model(GetHashKey(displayAttribute.VehicleName)), GetEntityCoords(GetPlayerPed(-1), true));
                            Parking.SendVehicleInfo(vehicleImport);
                            BaseScript.TriggerServerEvent("core:getVehicleInfo");
                            Client.UpdatePlayer();
                        } else
                        {
                            Format.SendNotif("Vous n'avez pas assez de ~r~bitcoins~s~...");
                        }
                    };
                }
                shopMenu.AddSubMenu(carImport);

                var lootbox = new NativeMenu("Caisse du mois", "Caisse du mois")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                shopMenu.AddSubMenu(lootbox);

                Pool.Add(vip);
                Pool.Add(carImport);
                Pool.Add(lootbox);

                var menuInfo = new NativeMenu("Informations", "Informations")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                menuInfo.Add(new NativeItem($"~g~${PlayerInst.Money}"));
                menuInfo.Add(new NativeItem($"{PlayerInst.Firstname} {PlayerInst.Lastname}"));
                menuInfo.Add(new NativeItem($"Né le {PlayerInst.Birth}"));

                var inventoryMenu = new NativeMenu("Inventaire", "Inventaire")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(PlayerInst.Inventory);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item != null && item.ItemType == "item" && item.Quantity != 0)
                        {
                            var invItem = new NativeListItem<string>($"{item.Item} ({item.Quantity})", "", "Utiliser", "Donner");
                            inventoryMenu.Add(invItem);
                            invItem.Activated += async (sender, e) =>
                            {
                                await ItemActionAsync(invItem.SelectedItem, item.Item);
                            };
                        }
                    }
                }

                var clothMenu = new NativeMenu("Vêtements", "Vêtements")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var clothesInfo = JsonConvert.DeserializeObject<List<ClothesInfo>>(PlayerInst.ClothesList);
                if (clothesInfo != null)
                {
                    foreach (var clothes in clothesInfo)
                    {
                        if (clothes.Name != null)
                        {
                            var clotheItem = new NativeListItem<string>($"{clothes.Name}", "", "Porter", "Enlever");
                            clothMenu.Add(clotheItem);
                            clotheItem.Activated += (sender, e) =>
                            {
                                if (clotheItem.SelectedItem == "Porter")
                                {
                                    SetPedComponentVariation(GetPlayerPed(-1), clothes.Component, clothes.Drawable, clothes.Texture, clothes.Palette);
                                    var clothesJson = JsonConvert.SerializeObject(new ClothesInfo
                                    {
                                        Component = clothes.Component,
                                        Drawable = clothes.Drawable,
                                        Texture = clothes.Texture,
                                        Palette = clothes.Palette
                                    });

                                    BaseScript.TriggerServerEvent("core:updateClothe", clothesJson);
                                }
                                else
                                {
                                    SetPedComponentVariation(GetPlayerPed(-1), clothes.Component, 15, 0, 2);
                                }
                            };
                        }
                    }
                }

                var billsMenu = new NativeMenu("Factures", "Factures")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };
                var bills = JsonConvert.DeserializeObject<List<Bills>>(PlayerInst.Bills);
                if (bills != null)
                {
                    foreach (var billsItem in bills)
                    {
                        var billItem = new NativeItem($"{billsItem.Company}", $"Par {billsItem.Author} le {billsItem.Date}", $"~g~${billsItem.Amount}");
                        billsMenu.Add(billItem);
                        billItem.Activated += (sender, e) =>
                        {
                            var selectedBill = bills.FirstOrDefault(b => b.Company == billsItem.Company && b.Amount == billsItem.Amount);
                            if (selectedBill != null)
                            {
                                if (PlayerInst.Money >= billsItem.Amount)
                                {
                                    bills.Remove(selectedBill);
                                    billsMenu.Remove(billItem);
                                    BaseScript.TriggerServerEvent("core:payBill", billsItem.Company, billsItem.Amount);
                                }
                                else
                                {
                                    Format.SendNotif("Tu n'as pas assez d'~r~argent...");
                                }
                            }
                        };
                    }
                }

                var carMenu = new NativeMenu("Gestion du véhicule", "Gestion du véhicule")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };

                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == true)
                {
                    var engineHealth = (GetVehicleEngineHealth(GetVehiclePedIsIn(PlayerPedId(), false))) * 100.0 / 5000.0 * 5;
                    var vehicleStatus = new NativeItem($"Etat du moteur: {String.Format("{0:0.##}", engineHealth)}%");
                    carMenu.Add(vehicleStatus);
                    var driftMode = new NativeCheckboxItem("Activer le mode Drift");
                    carMenu.Add(driftMode);
                    var driftState = false;
                    driftMode.Activated += (sender, e) =>
                    {
                        driftState = !driftState;
                        if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == true)
                        {
                            if (driftState)
                            {
                                SetVehicleReduceGrip(GetVehiclePedIsIn(PlayerPedId(), false), true);
                            }
                            else
                            {
                                SetVehicleReduceGrip(GetVehiclePedIsIn(PlayerPedId(), false), false);
                            }
                        }
                        else
                        {
                            Format.SendNotif("~r~Vous n'êtes pas dans un véhicule");
                        }
                    };

                    var doorsItem = new NativeListItem<string>("Ouvrir / Fermer une porte", "", "Avant gauche", "Avant droit", "Arrière gauche", "Arrière droit", "Capot", "Coffre", "Toutes les portes");
                    var doors = new List<string>() { "Front Left", "Front Right", "Rear Left", "Rear Right", "Hood", "Trunk" };
                    carMenu.Add(doorsItem);

                    doorsItem.Activated += (sender, e) =>
                    {
                        if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == true)
                        {
                            if (GetPedInVehicleSeat(GetVehiclePedIsIn(PlayerPedId(), false), -1) == GetPlayerPed(-1))
                            {
                                var index = doorsItem.SelectedIndex;
                                if (index <= 5)
                                {
                                    bool open = GetVehicleDoorAngleRatio(GetVehiclePedIsIn(GetPlayerPed(-1), false), index) > 0.1f ? true : false;

                                    if (open)
                                    {
                                        SetVehicleDoorShut(GetVehiclePedIsIn(GetPlayerPed(-1), false), index, false);
                                    }
                                    else
                                    {
                                        SetVehicleDoorOpen(GetVehiclePedIsIn(GetPlayerPed(-1), false), index, false, false);
                                    }
                                }
                                else if (doorsItem.SelectedItem == "Toutes les portes")
                                {
                                    var open = false;
                                    for (var door = 0; door < 5; door++)
                                    {

                                        open = !open;

                                        if (open)
                                        {
                                            SetVehicleDoorsShut(GetVehiclePedIsIn(GetPlayerPed(-1), false), false);

                                        }
                                        else
                                        {
                                            SetVehicleDoorOpen(GetVehiclePedIsIn(GetPlayerPed(-1), false), door, false, false);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Format.SendNotif("~r~Vous n'êtes pas dans un véhicule");
                        }
                    };
                }

                var weaponsMenu = new NativeMenu("Armes", "Armes")
                {
                    TitleFont = CitizenFX.Core.UI.Font.ChaletLondon,
                    UseMouse = false
                };

                if (items != null)
                {
                    foreach (var weapon in items)
                    {
                        var ammo = 0;
                        foreach (var item in items)
                        {
                            if (item.Item == "Munitions")
                            {
                                ammo = item.Quantity;
                                break;
                            }
                        }


                        if (weapon.Item != null && weapon.ItemType == "weapon")
                        {
                            var weaponItem = new NativeListItem<string>($"{weapon.Item}", "", "Équiper", "Déséquiper");
                            weaponsMenu.Add(weaponItem);
                            weaponItem.Activated += (sender, e) =>
                            {
                                if (weaponItem.SelectedItem == "Équiper")
                                {
                                    GiveWeaponToPed(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weapon.Item}"), 0, false, false);
                                    SetPedAmmo(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weapon.Item}"), ammo);
                                }
                                else
                                {
                                    RemoveWeaponFromPed(GetPlayerPed(-1), (uint)GetHashKey($"weapon_{weapon.Item}"));
                                }
                            };
                        }
                    }
                }

                SetSubmenu(MainMenu, shopMenu, menuInfo, inventoryMenu, clothMenu, billsMenu, carMenu, weaponsMenu);
                SetPool(MainMenu, shopMenu, menuInfo, inventoryMenu, clothMenu, billsMenu, carMenu, weaponsMenu);
            }
        }

        public void F6Menu()
        {

        }

        private void OnReceivePlayers(string jsonInst, string jsonHandles)
        {
            var playersInst = JsonConvert.DeserializeObject<List<PlayerInstance>>(jsonInst);
            var playersHandle = JsonConvert.DeserializeObject<List<string>>(jsonHandles);
            Client.PlayersInstList.Clear();
            Client.PlayersHandle.Clear();
            foreach (var playerInst in playersInst)
            {
                Client.PlayersInstList.Add(playerInst);
            }
            foreach (var player in playersHandle)
            {
                Client.PlayersHandle.Add(player);
            }
        }


        public async Task ItemActionAsync(string action, string item)
        {
            if (action == "Utiliser")
            {
                switch(item)
                {
                    case "Pain":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 3000);
                        SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1))+10);
                        break;
                    case "Sandwich":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 3000);
                        SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1)) + 30);
                        break;
                    case "Eau":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 3000);
                        break;
                    case "Coca Cola":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 3000);
                        break;
                    case "Outil de crochetage":
                        Format.PlayAnimation("anim@heists@humane_labs@emp@hack_door", "hack_intro", 10000);
                        break;
                    case "Menotte":
                        BaseScript.TriggerServerEvent("");
                        break;
                    case "Dollars":
                        break;
                    case "Phone":
                        break;
                }
            } else if (action == "Donner")
            {
                var textInput = await Format.GetUserInput("Quantité", "1", 4);
                var parsedInput = int.Parse(textInput);
                var playerCoords = GetEntityCoords(GetPlayerPed(-1), true);
                var closestPed = World.GetClosest(playerCoords, World.GetAllPeds());
                if (GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, closestPed.Position.X, closestPed.Position.Y, closestPed.Position.Z, true) < 10)
                {
                    if (parsedInput != 0)
                    {
                        BaseScript.TriggerServerEvent("core:removeItem", item, parsedInput);
                        BaseScript.TriggerServerEvent("core:requestPlayerData");
                        BaseScript.TriggerServerEvent("core:addItem", item, parsedInput, closestPed.Handle);
                        BaseScript.TriggerServerEvent("core:requestPlayerData", closestPed.Handle);
                    }
                }
            }
        }

        public void SetPool(params NativeMenu[] menus)
        {
            foreach (var element in menus)
            {
                Pool.Add(element);
            }
        }

        public void SetSubmenu(NativeMenu mainMenu, params NativeMenu[] subMenus)
        {
            foreach (var element in subMenus)
            {
                mainMenu.AddSubMenu(element);
            }
        }

        public void GetPlayerData(string json)
        {
            var player = JsonConvert.DeserializeObject<PlayerInstance>(json);
            PlayerInst.Id = player.Id;
            PlayerInst.Firstname = player.Firstname;
            PlayerInst.Lastname = player.Lastname;
            PlayerInst.Rank = player.Rank;
            PlayerInst.Bitcoin = player.Bitcoin;
            PlayerInst.Birth = player.Birth;
            PlayerInst.Clothes = player.Clothes;
            PlayerInst.ClothesList = player.ClothesList;
            PlayerInst.Money = player.Money;
            PlayerInst.Bills = player.Bills;
            PlayerInst.Inventory = player.Inventory;
        }

    }
}
