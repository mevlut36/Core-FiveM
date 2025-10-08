using System;
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
using Core.Shared;
using System.Threading;

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

            Client.AddEvent("core:receivePlayers", new Action<string, string>(OnReceivePlayers));
        }

        public PlayerInstance PlayerInst = new PlayerInstance
        {
            Inventory = new List<InventoryItem>(),
            Money = 0
        };

        public void F5Menu()
        {
            if (IsControlPressed(0, 166))
            {
                if (PlayerInst.Firstname == null)
                {
                    BaseScript.TriggerServerEvent("core:requestPlayerData");
                }
                var PlayerPos = GetEntityCoords(PlayerPedId(), true);
                var MainMenu = new NativeMenu("F5", "Menu personnel")
                {
                    Visible = true,
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var shopMenu = new NativeMenu("Boutique", "~o~Boutique")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var menuInfo = new NativeMenu("Informations", "Informations")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var inventoryMenu = new NativeMenu("Inventaire", "Inventaire")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var clothMenu = new NativeMenu("Vêtements", "Vêtements")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var billsMenu = new NativeMenu("Factures", "Factures")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var weaponsMenu = new NativeMenu("Armes", "Armes")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var playerList = new NativeMenu("Liste des joueurs", "Liste des joueurs")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var reportList = new NativeMenu("Liste des reports", "Liste des reports")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var vip = new NativeMenu("VIP", "VIP")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var carImport = new NativeMenu("Véhicules import", "Véhicules import")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                var lootbox = new NativeMenu("Caisse du mois", "Caisse du mois")
                {
                    MouseBehavior = MenuMouseBehavior.Disabled
                };
                if (PlayerInst.Rank == "staff")
                {
                    BaseScript.TriggerServerEvent("core:getPlayersList");
                    var adminMenu = new NativeMenu("Administration", "~o~Administration")
                    {
                        MouseBehavior = MenuMouseBehavior.Disabled
                    };
                    Pool.Add(adminMenu);
                    MainMenu.AddSubMenu(adminMenu);
                    Pool.Add(playerList);
                    for (int i = 0; i < Client.PlayersInstList.Count; i++)
                    {
                        var playerInst = Client.PlayersInstList[i];
                        var handle = Client.PlayersHandle[i];

                        var targetPlayer = GetPlayerPed(GetPlayerFromServerId(int.Parse(handle)));

                        var job = JsonConvert.DeserializeObject<JobInfo>(playerInst.Job);
                        var playerMenu = new NativeMenu($"", $"{handle} | {playerInst.Firstname} {playerInst.Lastname}",
                        $"ID: {handle}\nDB ID: {playerInst.Id}\nDiscord: {playerInst.Discord}\nJob: {job.JobID} | {job.JobRank}\nOrga: {playerInst.Organisation}\nRank: {playerInst.Rank}\nBitcoin: {playerInst.Bitcoin}\nMoney: {playerInst.Money}")
                        {
                            MouseBehavior = MenuMouseBehavior.Disabled
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
                            var reason = await Format.GetUserInput("Raison", "", 20);
                            BaseScript.TriggerServerEvent("core:jail", handle, reason, parsedInput);
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
                    BaseScript.TriggerServerEvent("core:getPlayersList");
                    var showNameState = false;
                    showName.Activated += (sender, e) =>
                    {
                        showNameState = !showNameState;
                        TogglePlayerNames(showNameState);
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
                        SetEntityInvincible(closestPed.Handle, false);
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
                            Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "~r~Votre véhicule n'existe pas");
                            return;
                        }
                        var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
                        Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                        Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "~g~Votre véhicule est apparu");
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
                            Format.ShowAdvancedNotification("ShurikenRP", "Admin Sys.", "~g~Vous avez bien réparé le véhicule");
                        }
                        else
                        {
                            Format.ShowAdvancedNotification("ShurikenRP", "Admin Sys.", "~r~Vous devez être dans un véhicule"); ;
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
                        ClearAreaOfEverything(PlayerPos.X, PlayerPos.Y, PlayerPos.Z, int.Parse(input), false, false, false, false);
                    };
                }

                if (Client.IsDead == false)
                {
                    shopMenu.Add(new NativeItem("[V.I.P] Aucun"));
                    shopMenu.Add(new NativeItem($"Bitcoins: {PlayerInst.Bitcoin}"));
                    var vip_1 = new NativeItem("~f~~h~VIP~h~", "", "~g~500~w~ Bitcoins /mois");
                    var vip_2 = new NativeItem("~y~~h~VIP+~h~", "", "~g~700~w~ Bitcoins /mois");
                    var vip_3 = new NativeItem("~q~~h~MVP~h~", "", "~g~1000~w~ Bitcoins /mois");
                    vip.Add(vip_1);
                    vip.Add(vip_2);
                    vip.Add(vip_3);
                    shopMenu.AddSubMenu(vip);

                    
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
                                PlayerInst.Bitcoin -= displayAttribute.Price;
                                var model = new Model(GetHashKey(displayAttribute.VehicleName));

                                if (!model.IsInCdImage || !model.IsValid)
                                {
                                    Debug.WriteLine($"Le modèle du véhicule {model} n'est pas valide ou n'est pas présent dans les fichiers du jeu.");
                                    Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Erreur: veuillez contacter un admin");
                                    return;
                                }

                                model.Request();

                                while (!model.IsLoaded)
                                {
                                    await BaseScript.Delay(0);
                                }

                                var car = await World.CreateVehicle(model.Hash, GetEntityCoords(GetPlayerPed(-1), true), 90);

                                if (car != null && car.Exists())
                                {
                                    car.IsPersistent = true;
                                    SetPedIntoVehicle(GetPlayerPed(-1), car.Handle, -2);
                                    SendVehicleInfo(car);
                                    BaseScript.TriggerServerEvent("core:getVehicleInfo");
                                }
                                else
                                {
                                    Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Erreur: Impossible de créer le véhicule");
                                }
                            }
                            else
                            {
                                Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Vous n'avez pas assez de ~r~bitcoins~s~...");
                            }
                        };
                    }
                    shopMenu.AddSubMenu(carImport);
                    shopMenu.AddSubMenu(lootbox);

                    Pool.Add(vip);
                    Pool.Add(carImport);
                    Pool.Add(lootbox);

                    
                    menuInfo.Add(new NativeItem($"~g~${PlayerInst.Money}"));
                    menuInfo.Add(new NativeItem($"{PlayerInst.Firstname} {PlayerInst.Lastname}"));
                    menuInfo.Add(new NativeItem($"Né le {PlayerInst.Birth}"));

                    var cardId = new NativeItem("Montrer sa carte d'identité");
                    menuInfo.Add(cardId);
                    cardId.Activated += (sender, e) =>
                    {
                        var player = GetPlayerPed(-1);
                        var playerCoords = GetEntityCoords(player, true);
                        var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                        var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());
                        if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                        {
                            BaseScript.TriggerServerEvent("core:showCardId", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)));
                        }
                    };
                    var cardPPA = new NativeItem("Montrer son PPA");
                    menuInfo.Add(cardPPA);
                    cardPPA.Activated += (sender, e) =>
                    {

                    };
                    
                    var items = PlayerInst.Inventory;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            if (item != null && item.Type == "item" && item.Quantity != 0)
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

                    var clothesInfo = JsonConvert.DeserializeObject<List<ClothingSet>>(PlayerInst.ClothesList);
                    if (clothesInfo != null)
                    {
                        foreach (var clothesSet in clothesInfo)
                        {
                            if (clothesSet.Name != null)
                            {
                                var clotheItem = new NativeListItem<string>($"{clothesSet.Name}", "", "Porter", "Enlever");
                                clothMenu.Add(clotheItem);
                                clotheItem.Activated += (sender, e) =>
                                {
                                    if (clotheItem.SelectedItem == "Porter")
                                    {
                                        foreach (var component in clothesSet.Components)
                                        {
                                            if (component.Palette == 1)
                                            {
                                                SetPedComponentVariation(GetPlayerPed(-1), component.ComponentId, component.Drawable, component.Texture, component.Palette);
                                            } else
                                            {
                                                SetPedPropIndex(GetPlayerPed(-1), component.ComponentId, component.Drawable, component.Texture, false);
                                            }
                                        }

                                        var clothesJson = JsonConvert.SerializeObject(clothesSet);
                                        // BaseScript.TriggerServerEvent("core:updateClothe", clothesJson);
                                    }
                                    else
                                    {
                                        foreach (var component in clothesSet.Components)
                                        {
                                            if (component.Palette == 1)
                                            {
                                                SetPedComponentVariation(GetPlayerPed(-1), component.ComponentId, 15, 0, 2);
                                            } else
                                            {
                                                SetPedPropIndex(GetPlayerPed(-1), component.ComponentId, 8, 0, false);
                                            } 
                                        }
                                    }
                                };
                            }
                        }
                    }


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
                                        Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Tu n'as pas assez d'~r~argent...");
                                    }
                                }
                            };
                        }
                    }

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


                            if (weapon.Item != null && weapon.Type == "weapon")
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
                } else
                {
                    Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "~r~Vous êtes mort...");
                }
                SetSubmenu(MainMenu, shopMenu, menuInfo, inventoryMenu, clothMenu, billsMenu, weaponsMenu);
                SetPool(MainMenu, shopMenu, menuInfo, inventoryMenu, clothMenu, billsMenu, weaponsMenu);
            }
        }

        public void SendVehicleInfo(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Erreur: Le véhicule est null");
                return;
            }

            if (!vehicle.Exists())
            {
                Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Vous n'êtes pas dans une voiture");
                return;
            }

            var vehicleMods = vehicle.Mods;
            if (vehicleMods == null)
            {
                Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Erreur: Les modifications du véhicule sont null");
                return;
            }
            if (vehicle.Exists())
            {
                VehicleInfo info = new VehicleInfo
                {
                    Model = API.GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash),
                    Plate = vehicle.Mods.LicensePlate,
                    Boot = new List<BootInfo>(),
                    EngineLevel = vehicle.Mods[VehicleModType.Engine].Index,
                    BrakeLevel = vehicle.Mods[VehicleModType.Brakes].Index,
                    ColorPrimary = (int)vehicle.Mods.PrimaryColor,
                    ColorSecondary = (int)vehicle.Mods.SecondaryColor,
                    Spoiler = vehicle.Mods[VehicleModType.Spoilers].Index,
                    Bumber_F = vehicle.Mods[VehicleModType.FrontBumper].Index,
                    Bumber_R = vehicle.Mods[VehicleModType.RearBumper].Index,
                    Skirt = vehicle.Mods[VehicleModType.SideSkirt].Index,
                    Exhaust = vehicle.Mods[VehicleModType.Exhaust].Index,
                    Chassis = vehicle.Mods[VehicleModType.Frame].Index,
                    Grill = vehicle.Mods[VehicleModType.Grille].Index,
                    Bonnet = vehicle.Mods[VehicleModType.Hood].Index,
                    Wing_L = vehicle.Mods[VehicleModType.Fender].Index,
                    Wing_R = vehicle.Mods[VehicleModType.RightFender].Index,
                    Roof = vehicle.Mods[VehicleModType.Roof].Index,
                    Engine = vehicle.Mods[VehicleModType.Engine].Index,
                    Brakes = vehicle.Mods[VehicleModType.Brakes].Index,
                    Gearbox = vehicle.Mods[VehicleModType.Transmission].Index,
                    Horn = vehicle.Mods[VehicleModType.Horns].Index,
                    Suspension = vehicle.Mods[VehicleModType.Suspension].Index,
                    Armour = vehicle.Mods[VehicleModType.Armor].Index,
                    Subwoofer = vehicle.Mods[VehicleModType.Speakers].Index,
                    Hydraulics = vehicle.Mods[VehicleModType.Hydraulics].Index,
                    Wheels = vehicle.Mods[VehicleModType.FrontWheel].Index,
                    WheelsRearOrHydraulics = vehicle.Mods[VehicleModType.RearWheel].Index,
                    PLTHolder = vehicle.Mods[VehicleModType.PlateHolder].Index,
                    PLTVanity = vehicle.Mods[VehicleModType.VanityPlates].Index,
                    Interior1 = vehicle.Mods[VehicleModType.TrimDesign].Index,
                    Interior2 = vehicle.Mods[VehicleModType.Ornaments].Index,
                    Interior3 = vehicle.Mods[VehicleModType.Dashboard].Index,
                    Interior4 = vehicle.Mods[VehicleModType.DialDesign].Index,
                    Interior5 = vehicle.Mods[VehicleModType.DoorSpeakers].Index,
                    Seats = vehicle.Mods[VehicleModType.Seats].Index,
                    Steering = vehicle.Mods[VehicleModType.SteeringWheels].Index,
                    Knob = vehicle.Mods[VehicleModType.ColumnShifterLevers].Index,
                    Plaque = vehicle.Mods[VehicleModType.Plaques].Index,
                    Ice = vehicle.Mods[VehicleModType.Speakers].Index,
                    Trunk = vehicle.Mods[VehicleModType.Trunk].Index,
                    Hydro = vehicle.Mods[VehicleModType.Hydraulics].Index,
                    EngineBay1 = vehicle.Mods[VehicleModType.EngineBlock].Index,
                    EngineBay2 = vehicle.Mods[VehicleModType.Struts].Index,
                    EngineBay3 = vehicle.Mods[VehicleModType.ArchCover].Index,
                    Chassis2 = vehicle.Mods[VehicleModType.Aerials].Index,
                    Chassis3 = vehicle.Mods[VehicleModType.Trim].Index,
                    Chassis4 = vehicle.Mods[VehicleModType.Trim].Index,
                    Chassis5 = vehicle.Mods[VehicleModType.Tank].Index,
                    Door_L = vehicle.Mods[VehicleModType.Windows].Index,
                    Door_R = vehicle.Mods[VehicleModType.Windows].Index,
                    LiveryMod = vehicle.Mods[VehicleModType.Livery].Index
                };

                BaseScript.TriggerServerEvent("core:sendVehicleInfo", JsonConvert.SerializeObject(info));
                BaseScript.Delay(1000).ContinueWith(_ =>
                {
                    BaseScript.TriggerServerEvent("core:getVehicleInfo");
                });
            }
            else
            {
                Format.ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Vous n'êtes pas dans une voiture");
            }
        }

        public void F6Menu()
        {

        }

        public void TogglePlayerNames(bool showNames)
        {
            var gamerTags = new List<int>();
            for (int i = 0; i < Client.PlayersInstList.Count; i++)
            {
                var playerInst = Client.PlayersInstList[i];
                var handle = Client.PlayersHandle[i];
                var targetPlayer = GetPlayerPed(GetPlayerFromServerId(int.Parse(handle)));

                if (i >= gamerTags.Count)
                {
                    int gamerTagID = CreateMpGamerTag(targetPlayer, $"[{handle}] {playerInst.Firstname} {playerInst.Lastname}", true, false, "test", 1);
                    gamerTags.Add(gamerTagID);
                }

                SetMpGamerTagVisibility(gamerTags[i], 0, showNames);
            }
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
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                        SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1))+10);
                        await Format.AddPropToPlayer("prop_sandwich_01", 28422, 0, 0, 0, 0, 0, 0, 3000);
                        Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                        break;
                    case "Sandwich":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                        SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1)) + 30);
                        await Format.AddPropToPlayer("prop_sandwich_01", 28422, 0, 0, 0, 0, 0, 0, 3000);
                        Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                        break;
                    case "Burger":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                        SetEntityHealth(GetPlayerPed(-1), GetEntityHealth(GetPlayerPed(-1)) + 30);
                        await Format.AddPropToPlayer("prop_cs_burger_01", 28422, 0, 0, 0, 0, 0, 0, 3000);
                        Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                        break;
                    case "Eau":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                        await Format.AddPropToPlayer("prop_water_bottle", 28422, 0, 0, 0, 0, 0, 0, 3000);
                        Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                        break;
                    case "Coca Cola":
                        Format.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8, (AnimationFlags)50);
                        await Format.AddPropToPlayer("prop_ecola_can", 28422, 0, 0, 0, 0, 0, 0, 3000);
                        await BaseScript.Delay(3000);
                        Format.StopAnimation("mini@sprunk", "plyr_buy_drink_pt2");
                        break;
                    case "Outil de crochetage":
                        Format.PlayAnimation("anim@heists@humane_labs@emp@hack_door", "hack_intro", 8, (AnimationFlags)50);
                        await BaseScript.Delay(3000);
                        Format.StopAnimation("anim@heists@humane_labs@emp@hack_door", "hack_intro");
                        break;
                    case "Menotte":
                        var player = GetPlayerPed(-1);
                        var playerCoords = GetEntityCoords(player, true);
                        var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                        var playerTarget = World.GetClosest(playerCoords, without_me.ToArray());
                        if (GetDistanceBetweenCoords(playerTarget.Position.X, playerTarget.Position.Y, playerTarget.Position.Z, playerCoords.X, playerCoords.Y, playerCoords.Z, true) < 10)
                        {
                            AttachEntityToEntity(player, playerTarget.Handle, 11816, -0.1f, 0.45f, 0, 0, 0, 20, false, false, false, false, 20, false);
                            BaseScript.TriggerServerEvent("core:cuff", GetPlayerServerId(NetworkGetPlayerIndexFromPed(playerTarget.Handle)));
                        }
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
                var without_me = World.GetAllPeds().Except(new List<Ped>() { Game.PlayerPed });
                var closestPed = World.GetClosest(playerCoords, without_me.ToArray());
                if (GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, closestPed.Position.X, closestPed.Position.Y, closestPed.Position.Z, true) < 10)
                {
                    if (parsedInput != 0)
                    {
                        var closestPedID = GetPlayerServerId(NetworkGetPlayerIndexFromPed(closestPed.Handle));
                        BaseScript.TriggerServerEvent("core:shareItem", closestPedID, item, parsedInput);
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
            PlayerInst.Discord = player.Discord;
            PlayerInst.Firstname = player.Firstname;
            PlayerInst.Lastname = player.Lastname;
            PlayerInst.State = player.State;
            PlayerInst.Skin = player.Skin;
            PlayerInst.Rank = player.Rank;
            PlayerInst.Bitcoin = player.Bitcoin;
            PlayerInst.Birth = player.Birth;
            PlayerInst.Clothes = player.Clothes;
            PlayerInst.ClothesList = player.ClothesList;
            PlayerInst.Money = player.Money;
            PlayerInst.Bills = player.Bills;
            PlayerInst.Inventory = player.Inventory;
            PlayerInst.LastPosition = player.LastPosition;
        }

    }
}
