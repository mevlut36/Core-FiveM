using CitizenFX.Core;
using Core.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Core.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from Core.Server!");
            EventHandlers["core:playerSpawned"] += new Action<Player>(PlayerConnected);
            EventHandlers["playerDropped"] += new Action<Player>(IsPlayerDisconnecting);
        }

        public void PlayerConnected([FromSource] Player player)
        {
            using (var dbContext = new DataContext())
            {
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (player.Identifiers["discord"] == null)
                {
                    player.Drop("Votre compte discord n'est pas lié");
                }

                if (existingPlayer != null)
                {
                    var playerInstance = new PlayerInstance
                    {
                        Id = existingPlayer.Id,
                        State = existingPlayer.State,
                        Discord = existingPlayer.Discord,
                        Skin = existingPlayer.Skin,
                        Firstname = existingPlayer.FirstName,
                        Lastname = existingPlayer.LastName,
                        Rank = existingPlayer.Rank,
                        Job = existingPlayer.Job,
                        Organisation = existingPlayer.Organisation,
                        Bitcoin = existingPlayer.Bitcoin,
                        Birth = existingPlayer.Birth,
                        Cars = existingPlayer.Cars,
                        Clothes = JsonConvert.DeserializeObject<List<ClothingSet>>(existingPlayer.Clothes),
                        ClothesList = existingPlayer.ClothesList,
                        Money = existingPlayer.Money,
                        Bills = existingPlayer.Bills,
                        LastPosition = JsonConvert.DeserializeObject<Vector3>(existingPlayer.LastPosition),
                        Inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory)
                    };

                    SendPlayerDataToClientEvents(player, playerInstance);
                    TriggerClientEvent(player, "core:playerConnected", JsonConvert.SerializeObject(playerInstance));
                }
                else
                {
                    TriggerClientEvent(player, "core:createCharacter");
                    SetPlayerRoutingBucket(player.Handle, Int32.Parse(player.Handle));
                }
            }
        }

        [EventHandler("core:requestPlayerData")]
        public void GetPlayerData([FromSource] Player player, int playerId = -1)
        {
            using (var dbContext = new DataContext())
            {
                Player targetPlayer = playerId == -1 ? player : Players[playerId];
                var license = targetPlayer.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);

                if (existingPlayer != null)
                {
                    var playerState = JsonConvert.DeserializeObject<PlayerState>(existingPlayer.State);
                    if (playerState.IsCuffed)
                    {
                        TriggerClientEvent(player, "core:cuffPlayer");
                    }
                    var playerInstance = new PlayerInstance
                    {
                        License = existingPlayer.License,
                        Id = existingPlayer.Id,
                        State = existingPlayer.State,
                        Discord = existingPlayer.Discord,
                        Skin = existingPlayer.Skin,
                        Firstname = existingPlayer.FirstName,
                        Lastname = existingPlayer.LastName,
                        Rank = existingPlayer.Rank,
                        Job = existingPlayer.Job,
                        Organisation = existingPlayer.Organisation,
                        Bitcoin = existingPlayer.Bitcoin,
                        Birth = existingPlayer.Birth,
                        Clothes = JsonConvert.DeserializeObject<List<ClothingSet>>(existingPlayer.Clothes),
                        ClothesList = existingPlayer.ClothesList,
                        Money = existingPlayer.Money,
                        Bills = existingPlayer.Bills,
                        Inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory)
                    };
                    SendPlayerDataToClientEvents(player, playerInstance);
                }
            }
        }

        [EventHandler("core:updateInventory")]
        public void UpdateInventory([FromSource] Player player, int playerId = -1)
        {
            using (var dbContext = new DataContext())
            {
                Player targetPlayer = playerId == -1 ? player : Players[playerId];
                var license = targetPlayer.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);

                if (existingPlayer != null)
                {
                    TriggerClientEvent(targetPlayer, "core:updateInventory", existingPlayer.Inventory);
                    TriggerClientEvent(targetPlayer, "legal:updateInventory", existingPlayer.Inventory);
                }
            }
        }

        [EventHandler("core:updatePlayer")]
        public void UpdatePlayer([FromSource] Player player, dynamic obj, int playerId = -1)
        {
            using (var dbContext = new DataContext())
            {
                Player targetPlayer = playerId == -1 ? player : Players[playerId];
                var license = targetPlayer.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);

                if (existingPlayer != null)
                {
                    if (obj is ItemQuantity)
                    {
                        var data = JsonConvert.DeserializeObject<List<ItemQuantity>>(obj);
                        TriggerClientEvent(targetPlayer, "core:updatePlayer", existingPlayer.Inventory);
                    }
                    else if (obj is string && ((string)obj).Contains("Bills"))
                    {
                        var data = (string)obj;
                        TriggerClientEvent(targetPlayer, "core:updatePlayer", existingPlayer.Bills);
                    }
                }
            }
        }


        private void SendPlayerDataToClientEvents(Player player, PlayerInstance instance)
        {
            var json = JsonConvert.SerializeObject(instance);
            var events = new[]
            {
                "core:getPlayerData",
                "legal_client:getPlayerData",
                "illegal_client:getPlayerData"
            };

            foreach (var eventName in events)
            {
                TriggerClientEvent(player, eventName, json);
            }
        }

        [EventHandler("legal_server:requestCompanyData")]
        public void GetCompanyData([FromSource] Player player, int id)
        {
            using (var dbContext = new DataContext())
            {
                var networkId = player.Handle;
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                var existingCompany = dbContext.Company.FirstOrDefault(u => u.Id == id);

                if (existingPlayer != null)
                {
                    var chestInstance = new CompanyInstance
                    {
                        Id = existingCompany.Id,
                        Name = existingCompany.Name,
                        Chest = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingCompany.Chest),
                        Taxes = existingCompany.Taxes
                    };
                    var json = JsonConvert.SerializeObject(chestInstance);
                    TriggerClientEvent(player, "legal_client:getCompanyData", json);
                }
            }
        }

        [EventHandler("core:setClothes")]
        public void SetClothes([FromSource] Player player)
        {
            using (var dbContext = new DataContext())
            {
                var networkId = player.Handle;
                var playerPed = GetPlayerPed(networkId);
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    TriggerClientEvent(player, "core:getClothes", existingPlayer.Clothes, existingPlayer.Skin);
                }
            }
        }

        [EventHandler("core:buyTopClothes")]
        public void BuyTopClothes([FromSource] Player player, int cost, string json)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                var clothes = JsonConvert.DeserializeObject<ClothingSet>(json);
                if (existingPlayer != null)
                {
                    if (existingPlayer.Money >= cost)
                    {
                        var items = JsonConvert.DeserializeObject<List<ClothingSet>>(existingPlayer.ClothesList);
                        existingPlayer.Money -= cost;
                        items.Add(clothes);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez acheté de nouveaux vêtements.\n~r~-${cost}");
                        existingPlayer.ClothesList = JsonConvert.SerializeObject(items);
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", $"~r~Vous n'avez pas assez d'argent.");
                    }

                    dbContext.SaveChanges();
                    GetPlayerData(player);
                }
            }
        }

        public void IsPlayerDisconnecting([FromSource] Player player)
        {
            using (var dbContext = new DataContext())
            {
                var license = player.Identifiers["license"];
                var json = JsonConvert.SerializeObject(player.Character.Position);
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    existingPlayer.LastPosition = json;
                    dbContext.SaveChanges();
                    Debug.WriteLine($"{existingPlayer.FirstName} {existingPlayer.LastName} ({player.Name}) disconnected");
                }
            }
        }

        [EventHandler("legal_server:addItemInChest")]
        public void AddItemInChest([FromSource] Player player, int id, string item, int quantity, string type)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);

            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                var existingCompany = dbContext.Company.FirstOrDefault(u => u.Id == id);

                if (existingPlayer != null)
                {
                    var chests = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingCompany.Chest);
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);

                    var itemFilter = chests.FirstOrDefault(i => i.Item == item);
                    var itemInvFilter = inventory.FirstOrDefault(i => i.Item == item);
                    if (itemFilter != null)
                    {
                        itemFilter.Quantity += quantity;
                        itemInvFilter.Quantity -= quantity;
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez mis ~r~{quantity}~w~ de {item} dans le coffre.");
                    }
                    else
                    {
                        var newItem = new ItemQuantity
                        {
                            Item = item,
                            Quantity = quantity,
                            ItemType = type,
                        };

                        chests.Add(newItem);
                        itemInvFilter.Quantity -= quantity;
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez ajouté un nouvel article : {item}.\n~r~-${quantity}");
                    }

                    var updatedChests = JsonConvert.SerializeObject(chests);
                    existingCompany.Chest = updatedChests;
                    existingPlayer.Inventory = JsonConvert.SerializeObject(inventory);
                    dbContext.SaveChanges();

                    GetCompanyData(player, id);
                    UpdateInventory(player);
                }
            }
        }

        [EventHandler("legal_server:removeItemFromChest")]
        public void RemoveItemFromChest([FromSource] Player player, int id, string jsonItem)
        {
            var license = player.Identifiers["license"];
            var item = JsonConvert.DeserializeObject<ItemQuantity>(jsonItem);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                var existingCompany = dbContext.Company.FirstOrDefault(u => u.Id == id);

                if (existingPlayer != null && existingCompany != null)
                {
                    var chests = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingCompany.Chest) ?? new List<ItemQuantity>();
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);

                    var chestItem = chests.FirstOrDefault(i => i.Item == item.Item);
                    var itemInvFilter = inventory.FirstOrDefault(i => i.Item == item.Item);

                    if (chestItem != null)
                    {
                        if (itemInvFilter != null)
                        {
                            chestItem.Quantity -= item.Quantity;
                            itemInvFilter.Quantity += item.Quantity;
                        }
                        else
                        {
                            chestItem.Quantity -= item.Quantity;
                            inventory.Add(item);
                        }
                        if (chestItem.Quantity <= 0)
                        {
                            chests.Remove(chestItem);
                        }
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez pris ~r~{item.Quantity}~w~ de {item.Item}.");
                    }
                    else
                    {
                        chestItem.Quantity -= item.Quantity;
                        inventory.Add(item);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez pris un nouvel article : {item}.\n~r~-${item.Quantity}");
                    }

                    var updatedChests = JsonConvert.SerializeObject(chests);
                    var updatedInventory = JsonConvert.SerializeObject(inventory);
                    existingCompany.Chest = updatedChests;
                    existingPlayer.Inventory = updatedInventory;
                    dbContext.SaveChanges();

                    GetCompanyData(player, id);
                    UpdateInventory(player);
                }
            }
        }

        [EventHandler("core:robbery")]
        public async void Robbery([FromSource] Player player, string ltdName)
        {
            using (var dbContext = new DataContext())
            {
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    foreach (var p in Players)
                    {
                        var playerPolice = dbContext.Player.FirstOrDefault(a => a.License == p.Identifiers["license"]);
                        var jobPolice = JsonConvert.DeserializeObject<JobInfo>(playerPolice.Job);
                        if (jobPolice.JobID.Equals("1") || playerPolice.Rank == "staff")
                        {
                            TriggerClientEvent(p, "core:sendNotif", $"~r~ALERTE\r~s~Braquage en cours à {ltdName}");
                        }
                    }
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                    var itemFilter = inventory.FirstOrDefault(item => item.Item == "Dollars");
                    Random random = new Random();
                    int randomNumber = random.Next(3500, 5001);

                    var dollars = new ItemQuantity
                    {
                        ItemType = "item",
                        Item = "Dollars",
                        Quantity = randomNumber,
                    };

                    if (itemFilter != null)
                    {
                        itemFilter.Quantity += randomNumber;
                    }
                    else
                    {
                        inventory.Add(dollars);
                    }

                    await Delay(10000);
                    var updatedInventory = JsonConvert.SerializeObject(inventory);
                    existingPlayer.Inventory = updatedInventory;
                    dbContext.SaveChanges();

                    UpdateInventory(player);
                    TriggerClientEvent(player, "core:sendNotif", $"Vous avez récupérer ~g~${dollars.Quantity}");
                }
                else
                {
                    Debug.WriteLine("Error in core:robbery: Player doesn't exist");
                }
            }
        }

        [EventHandler("core:bankRobbery")]
        public void BankRobbery([FromSource] Player player, string bankName)
        {
            using (var dbContext = new DataContext())
            {
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    foreach (var p in Players)
                    {
                        var playerPolice = dbContext.Player.FirstOrDefault(a => a.License == p.Identifiers["license"]);
                        var jobPolice = JsonConvert.DeserializeObject<JobInfo>(playerPolice.Job);
                        if (jobPolice.JobID.Equals("1") || playerPolice.Rank == "staff")
                        {
                            TriggerClientEvent(p, "core:sendNotif", $"~r~ALERTE\r~s~Braquage en cours à {bankName}");
                        }
                    }
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                    var itemFilterDollars = inventory.FirstOrDefault(item => item.Item == "Dollars");
                    var itemFilterPerceuse = inventory.FirstOrDefault(item => item.Item == "Perceuse");
                    var itemFilterPc = inventory.FirstOrDefault(item => item.Item == "Ordinateur");
                    Random random = new Random();
                    int randomNumber = random.Next(150000, 200000);

                    var dollars = new ItemQuantity
                    {
                        ItemType = "item",
                        Item = "Dollars",
                        Quantity = randomNumber,
                    };

                    if (itemFilterPc.Quantity > 0 && itemFilterPerceuse.Quantity > 0)
                    {
                        itemFilterPerceuse.Quantity -= 1;
                        itemFilterPc.Quantity -= 1;

                        if (itemFilterDollars != null)
                        {
                            itemFilterDollars.Quantity += randomNumber;
                        }
                        else
                        {
                            inventory.Add(dollars);
                        }
                        var updatedInventory = JsonConvert.SerializeObject(inventory);
                        existingPlayer.Inventory = updatedInventory;
                        dbContext.SaveChanges();

                        UpdateInventory(player);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez récupérer ~g~${dollars.Quantity}");
                    }
                }
                else
                {
                    Debug.WriteLine("Error in core:robbery: Player doesn't exist");
                }
            }
        }

        [EventHandler("core:registerPlayerData")]
        public void RegisterPlayerData([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<PlayerInstance>(json);

            using (var dbContext = new DataContext())
            {
                var dollars = new ItemQuantity
                {
                    ItemType = "item",
                    Item = "Dollars",
                    Quantity = 10000,
                };
                var bread = new ItemQuantity
                {
                    ItemType = "item",
                    Item = "Pain",
                    Quantity = 20,
                };
                var water = new ItemQuantity
                {
                    ItemType = "item",
                    Item = "Eau",
                    Quantity = 20,
                };

                var inventoryJson = JsonConvert.SerializeObject(new List<ItemQuantity> { dollars, bread, water });

                var state = new PlayerState
                {
                    Health = 200,
                    Thirst = 200,
                    IsCuffed = false
                };

                var defaultJob = new JobInfo
                {
                    JobID = "0",
                    JobRank = 0,
                };

                var newPlayer = new PlayerTable
                {
                    License = player.Identifiers["license"],
                    Discord = player.Identifiers["discord"],
                    State = JsonConvert.SerializeObject(state),
                    Skin = data.Skin,
                    FirstName = data.Firstname,
                    LastName = data.Lastname,
                    Rank = "player",
                    Job = JsonConvert.SerializeObject(defaultJob),
                    Organisation = "aucun",
                    Clothes = data.ClothesList,
                    ClothesList = data.ClothesList,
                    Money = 50000,
                    Birth = data.Birth,
                    Inventory = inventoryJson,
                    Bills = data.Bills,
                    Cars = "[]",
                    LastPosition = new System.Numerics.Vector3(-283.2f, -939.4f, 31.2f).ToString()
                };
                dbContext.Player.Add(newPlayer);
                dbContext.SaveChanges();

                SetPlayerRoutingBucket(player.Handle, 0);
                GetPlayerData(player);
                TriggerClientEvent(player, "core:sendNotif", $"Vos informations ont bien été enregistré");
            }
        }

        [EventHandler("core:updateClothes")]
        public void UpdateClothes([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<PlayerInstance>(json);
            using (var dbContext = new DataContext())
            {
                var newClothes = dbContext.Player.FirstOrDefault(a => a.License == player.Identifiers["license"]);

                if (newClothes != null)
                {
                    newClothes.Clothes = JsonConvert.SerializeObject(data.Clothes);
                    dbContext.SaveChanges();
                    GetPlayerData(player);
                }
                TriggerClientEvent(player, "core:sendNotif", $"Vos informations ont bien été enregistré");
            }
        }

        [EventHandler("core:updateClothe")]
        public void UpdateClothe([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<ClothingSet>(json);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(a => a.License == player.Identifiers["license"]);

                if (existingPlayer != null)
                {
                    var clothesList = JsonConvert.DeserializeObject<List<ClothingSet>>(existingPlayer.Clothes);
                    foreach (var clothes in data.Components)
                    {
                        var itemToUpdate = clothesList.FirstOrDefault(c => c.Components == data.Components);
                        foreach(var item in itemToUpdate.Components) // Ligne 530
                        {
                            if (item != null)
                            {
                                item.Drawable = clothes.Drawable;
                                item.Texture = clothes.Texture;
                                item.Palette = clothes.Palette;
                                existingPlayer.Clothes = JsonConvert.SerializeObject(clothesList);
                                dbContext.SaveChanges();
                                GetPlayerData(player);
                            }
                        }
                    }
                    
                }
            }
        }

        [EventHandler("core:sendVehicleInfo")]
        public void SendVehicleInfo([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<VehicleInfo>(json);
            var license = player.Identifiers["license"];
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(a => a.License == license);

                if (existingPlayer != null)
                {
                    var cars = JsonConvert.DeserializeObject<List<VehicleInfo>>(existingPlayer.Cars);

                    cars.Add(data);

                    existingPlayer.Cars = JsonConvert.SerializeObject(cars);
                    dbContext.SaveChanges();
                }

                var newVehicle = JsonConvert.SerializeObject(data);

                TriggerClientEvent("core:updateVehicles", newVehicle);
                TriggerClientEvent(player, "core:sendNotif", "Vos informations ont bien été enregistrées");
            }
        }


        [EventHandler("core:getVehicleInfo")]
        public void GetVehicleInfo([FromSource] Player player)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    string json = existingPlayer.Cars;
                    TriggerClientEvent(player, "core:sendVehicleInfos", json, playerPed);
                }
            }
        }

        [EventHandler("core:requestVehicleByPlate")]
        public void GetVehicleByPlate([FromSource] Player player, string targetPlate)
        {
            using (var dbContext = new DataContext())
            {
                var carsData = dbContext.Player.Select(p => p.Cars).ToList();
                VehicleInfo foundVehicle = null;
                foreach (var carData in carsData)
                {
                    var vehicles = JsonConvert.DeserializeObject<List<VehicleInfo>>(carData);
                    var targetVehicle = vehicles.FirstOrDefault(v => v.Plate == targetPlate);
                    if (targetVehicle != null)
                    {
                        foundVehicle = targetVehicle;
                        break;
                    }
                }

                if (foundVehicle != null)
                {
                    TriggerClientEvent(player, "core:getVehicleByPlate", JsonConvert.SerializeObject(foundVehicle));
                }
                else
                {
                    var newVehicle = new VehicleInfo
                    {
                        Boot = new List<BootInfo>(),
                        Plate = targetPlate,
                    };
                    TriggerClientEvent(player, "core:getVehicleByPlate", JsonConvert.SerializeObject(newVehicle));
                }
            }
        }



        [EventHandler("core:showCardId")]
        public void ShowCardId([FromSource] Player player, int targetId)
        {
            var targetPlayer = Players[targetId];
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == player.Identifiers["license"]);
                var existingTarget = dbContext.Player.FirstOrDefault(u => u.License == targetPlayer.Identifiers["license"]);

                if (existingPlayer != null && existingTarget != null)
                {
                    TriggerClientEvent(targetPlayer, "core:sendNotif",
                        $"Prénom: {existingPlayer.FirstName}\n" +
                        $"Nom: {existingPlayer.LastName}\n" +
                        $"Né le {existingPlayer.Birth}\n" +
                        $"Nationalité: Américaine\n" +
                        $"Genre: {JsonConvert.DeserializeObject<SkinInfo>(existingPlayer.Skin).Gender}");
                }
            }
        }

        [EventHandler("core:bitcoinTransaction")]
        public void BitcoinTransaction([FromSource] Player player, int cost)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    if (existingPlayer.Bitcoin >= cost)
                    {
                        existingPlayer.Bitcoin = existingPlayer.Bitcoin - cost;
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez payé ~r~${cost}BTC~w~.");
                        dbContext.SaveChanges();
                        GetPlayerData(player);
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", $"~r~Vous n'avez pas assez de Bitcoins.");
                    }
                }
            }
        }

        [EventHandler("core:transaction")]
        public void Transaction([FromSource] Player player, int cost, string itemName, int amount)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                    var itemFilter = items.FirstOrDefault(item => item.Item == itemName);

                    if (existingPlayer.Money >= cost)
                    {
                        if (itemFilter != null)
                        {
                            existingPlayer.Money -= cost;
                            itemFilter.Quantity += amount;
                            TriggerClientEvent(player, "core:sendNotif", $"Vous avez payé ~r~${cost}~w~.");
                        }
                        else
                        {
                            var newItem = new ItemQuantity
                            {
                                ItemType = "item",
                                Item = itemName,
                                Quantity = amount
                            };

                            items.Add(newItem);
                            TriggerClientEvent(player, "core:sendNotif", $"Vous avez ajouté un nouvel article : {itemName}.\n~r~-${cost}");
                        }

                        existingPlayer.Inventory = JsonConvert.SerializeObject(items);
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", $"~r~Vous n'avez pas assez d'argent.");
                    }
                    dbContext.SaveChanges();
                    GetPlayerData(player);
                }
            }
        }

        [EventHandler("core:bankTransaction")]
        public void BankTransaction([FromSource] Player player, string action, int amount)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);

            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                if (existingPlayer != null)
                {
                    var items = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                    var dollarsItem = items.FirstOrDefault(item => item.Item == "Dollars");

                    if (action == "~g~<b>Retirer</b>")
                    {
                        if (amount <= existingPlayer.Money)
                        {
                            if (dollarsItem != null)
                            {
                                existingPlayer.Money -= amount;
                                dollarsItem.Quantity += amount;
                                TriggerClientEvent(player, "core:sendNotif", $"Vous avez {action} ~g~${amount}~w~.");
                            }
                            else
                            {
                                var newItem = new ItemQuantity
                                {
                                    ItemType = "item",
                                    Item = "Dollars",
                                    Quantity = amount
                                };

                                items.Add(newItem);
                                TriggerClientEvent(player, "core:sendNotif", $"Vous avez {action} ~g~${amount}~w~.");
                            }
                        }
                    }
                    else if (action == "~g~<b>Déposer</b>")
                    {
                        if (amount <= dollarsItem.Quantity)
                        {
                            existingPlayer.Money += amount;
                            dollarsItem.Quantity -= amount;
                            TriggerClientEvent(player, "core:sendNotif", $"Vous avez {action} ~g~${amount}~w~.");
                        }
                    }
                    existingPlayer.Inventory = JsonConvert.SerializeObject(items);
                    dbContext.SaveChanges();
                    GetPlayerData(player);
                }
            }
        }

        [EventHandler("core:buyClothes")]
        public void BuyClothes([FromSource] Player player, int cost, string name, int component, int drawable, int texture, int palette)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var items = JsonConvert.DeserializeObject<List<ClothesInfo>>(existingPlayer.ClothesList);

                    if (existingPlayer.Money >= cost)
                    {
                        var newItem = new ClothesInfo
                        {
                            Name = name,
                            Component = component,
                            Drawable = drawable,
                            Texture = texture,
                            Palette = palette
                        };

                        existingPlayer.Money -= cost;
                        items.Add(newItem);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez acheté un nouvel article : {name}.\n~r~-${cost}");
                        existingPlayer.ClothesList = JsonConvert.SerializeObject(items);
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", $"~r~Vous n'avez pas assez d'argent.");
                    }

                    dbContext.SaveChanges();
                    GetPlayerData(player);
                }
            }
        }



        [EventHandler("core:giveMoney")]
        public void GiveMoney([FromSource] Player player, int amount)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    existingPlayer.Money += amount;
                }
            }
            TriggerClientEvent(player, "core:sendNotif", $"Vous avez reçu ~g~${amount}~w~.");
        }

        [EventHandler("core:removeMoney")]
        public void RemoveMoney([FromSource] Player player, int amount)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    existingPlayer.Money -= amount;
                }
            }
            TriggerClientEvent(player, "core:sendNotif", $"~r~-${amount}~w~.");
        }

        [EventHandler("core:payBill")]
        public void PayBill([FromSource] Player player, string company, int amount)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                var existingCompany = dbContext.Company.FirstOrDefault(u => u.Name == company);
                if (existingPlayer != null)
                {
                    var bills = JsonConvert.DeserializeObject<List<Bills>>(existingPlayer.Bills);
                    var companyChest = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingCompany.Chest);
                    var dollars = new ItemQuantity
                    {
                        ItemType = "item",
                        Item = "Dollars",
                        Quantity = amount,
                    };
                    var selectedBill = bills.FirstOrDefault(b => b.Company == company && b.Amount == amount);
                    if (selectedBill != null)
                    {
                        bills.Remove(selectedBill);
                        companyChest.Add(dollars);
                        existingPlayer.Bills = JsonConvert.SerializeObject(bills);
                        existingCompany.Chest = JsonConvert.SerializeObject(companyChest);
                        GetCompanyData(player, existingCompany.Id);
                        dbContext.SaveChanges();
                    }
                }
            }
            TriggerClientEvent(player, "core:sendNotif", $"~gs~Vous avez bien payé la facture");
            GetPlayerData(player);
        }

        [EventHandler("core:addItem")]
        public void AddItem([FromSource] Player player, string item, int quantity, int playerId = -1)
        {
            Player targetPlayer = playerId == -1 ? player : Players[playerId];
            var license = targetPlayer.Identifiers["license"];
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);

                    var itemFilter = inventory.FirstOrDefault(i => i.Item == item);
                    if (itemFilter != null)
                    {
                        itemFilter.Quantity += quantity;
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez pris ~r~{quantity}~w~ de {item}.");
                    }
                    else
                    {
                        var newItem = new ItemQuantity
                        {
                            ItemType = "item",
                            Item = item,
                            Quantity = quantity
                        };

                        inventory.Add(newItem);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez ajouté un nouvel article : {item}.\n~r~-${quantity}");
                    }

                    var updatedInventory = JsonConvert.SerializeObject(inventory);
                    existingPlayer.Inventory = updatedInventory;
                    dbContext.SaveChanges();
                    UpdateInventory(player);
                }
            }
        }

        [EventHandler("illegal_server:addDrug")]
        public void AddDrug([FromSource] Player player, string item, int quantity)
        {
            var license = player.Identifiers["license"];
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                    var itemFilter = inventory.FirstOrDefault(i => i.Item == item);
                    if (itemFilter != null)
                    {
                        itemFilter.Quantity += quantity;
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez récolté un total de ~r~{quantity}~w~ {item}.");
                    }
                    else
                    {
                        var newItem = new ItemQuantity
                        {
                            ItemType = "item",
                            Item = item,
                            Quantity = quantity
                        };

                        inventory.Add(newItem);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez récolté un total de {quantity} {item}");
                    }

                    var updatedInventory = JsonConvert.SerializeObject(inventory);
                    existingPlayer.Inventory = updatedInventory;
                    dbContext.SaveChanges();
                    UpdateInventory(player);
                }
            }
        }

        [EventHandler("core:removeItem")]
        public void RemoveItem([FromSource] Player player, string item, int quantity = 1)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);

                    var itemFilter = inventory.FirstOrDefault(i => i.Item == item);
                    if (itemFilter != null)
                    {
                        itemFilter.Quantity -= quantity;
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez perdu ~r~{quantity}~w~ de {item}.");
                    }

                    var updatedInventory = JsonConvert.SerializeObject(inventory);
                    existingPlayer.Inventory = updatedInventory;
                    dbContext.SaveChanges();
                    UpdateInventory(player);
                }
            }
        }

        [EventHandler("core:addItemInBoot")]
        public void AddItemInBoot([FromSource] Player player, string plate, string item, int quantity, string type)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var vehicles = JsonConvert.DeserializeObject<List<VehicleInfo>>(existingPlayer.Cars);
                    var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                    if (vehicles != null && vehicles.Count > 0)
                    {
                        foreach (VehicleInfo vehicle in vehicles)
                        {
                            if (vehicle.Plate == plate)
                            {
                                var boot = vehicle.Boot;
                                var itemBootFilter = boot.FirstOrDefault(i => i.Item == item);
                                var itemFilter = inventory.FirstOrDefault(i => i.Item == item);
                                if (quantity <= itemFilter.Quantity)
                                {
                                    if (itemBootFilter != null)
                                    {
                                        itemBootFilter.Quantity += quantity;
                                        itemFilter.Quantity -= quantity;
                                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez déposé ~r~{quantity}~w~ de {item}.");
                                    }
                                    else
                                    {
                                        var newItem = new BootInfo
                                        {
                                            Item = item,
                                            Quantity = quantity,
                                            Type = type
                                        };

                                        boot.Add(newItem);
                                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez déposé un nouvel article : {item}.\n~r~+{quantity}");
                                    }
                                    existingPlayer.Cars = JsonConvert.SerializeObject(vehicles);
                                    existingPlayer.Inventory = JsonConvert.SerializeObject(inventory);
                                    dbContext.SaveChanges();

                                    UpdateInventory(player);
                                    GetVehicleInfo(player);
                                } else
                                {
                                    TriggerClientEvent("core:sendNotif", "~r~La quantité est trop importante");
                                }
                            }
                        }
                    }
                }
            }
        }

        [EventHandler("core:removeItemFromBoot")]
        public void RemoveItemFromBoot([FromSource] Player player, string plate, string item, int quantity)
        {
            var license = player.Identifiers["license"];

            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    List<VehicleInfo> vehicles = JsonConvert.DeserializeObject<List<VehicleInfo>>(existingPlayer.Cars);

                    if (vehicles != null && vehicles.Count > 0)
                    {
                        foreach (VehicleInfo vehicle in vehicles)
                        {
                            if (vehicle.Plate == plate)
                            {
                                var boot = vehicle.Boot;
                                var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                                var itemFilter = inventory.FirstOrDefault(i => i.Item == item);

                                if (boot != null && boot.Count > 0)
                                {
                                    for (int i = boot.Count - 1; i >= 0; i--)
                                    {
                                        BootInfo bootItem = boot[i];

                                        if (bootItem.Item == item && bootItem.Quantity >= quantity)
                                        {
                                            bootItem.Quantity -= quantity;
                                            if (bootItem.Quantity <= 0)
                                            {
                                                boot.RemoveAt(i);
                                            }

                                            if (itemFilter != null)
                                            {
                                                itemFilter.Quantity += quantity;
                                            } else
                                            {
                                                var newItem = new ItemQuantity
                                                {
                                                    ItemType = bootItem.Type,
                                                    Item = bootItem.Item,
                                                    Quantity = quantity,
                                                };
                                                inventory.Add(newItem);
                                            }

                                            existingPlayer.Cars = JsonConvert.SerializeObject(vehicles);
                                            existingPlayer.Inventory = JsonConvert.SerializeObject(inventory);
                                            dbContext.SaveChanges();

                                            TriggerClientEvent("core:sendNotif", $"Vous avez pris {quantity} de ~g~{bootItem.Item}");
                                            UpdateInventory(player);
                                            GetVehicleInfo(player);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [EventHandler("core:shareItem")]
        public void ShareItem([FromSource] Player player, int targetId, string item, int quantity)
        {
            var targetPlayer = Players[targetId];
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == player.Identifiers["license"]);
                var existingTarget = dbContext.Player.FirstOrDefault(u => u.License == targetPlayer.Identifiers["license"]);

                if (existingPlayer != null && existingTarget != null)
                {
                    var invPlayer = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                    var invTarget = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingTarget.Inventory);

                    var itemFilter = invPlayer.FirstOrDefault(i => i.Item == item);
                    var itemFilterT = invTarget.FirstOrDefault(i => i.Item == item);

                    if (itemFilter.Quantity >= quantity)
                    {
                        itemFilter.Quantity -= quantity;

                        if (itemFilterT != null)
                        {
                            itemFilterT.Quantity += quantity;
                            TriggerClientEvent(targetPlayer, "core:sendNotif", $"Il vous a donné {quantity} de {item}");
                        }
                        else
                        {
                            var newItem = new ItemQuantity
                            {
                                ItemType = "item",
                                Item = item,
                                Quantity = quantity
                            };

                            invTarget.Add(newItem);
                            TriggerClientEvent(targetPlayer, "core:sendNotif", $"Il vous a donné {quantity} de {item}");
                        }

                        existingPlayer.Inventory = JsonConvert.SerializeObject(invPlayer);
                        existingTarget.Inventory = JsonConvert.SerializeObject(invTarget);
                        dbContext.SaveChanges();
                        UpdateInventory(player);
                        UpdateInventory(targetPlayer);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez donné {quantity} de {item}");
                    }
                }
            }
        }

        [EventHandler("core:buyWeapon")]
        public void BuyWeapon([FromSource] Player player, string item, int cost)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    if (existingPlayer.Money >= cost)
                    {
                        var inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory);
                        existingPlayer.Money -= cost;
                        var newItem = new ItemQuantity
                        {
                            ItemType = "weapon",
                            Item = item,
                            Quantity = 1,
                        };

                        inventory.Add(newItem);
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez ajouté un nouvel article : {item}.");

                        var updatedInventory = JsonConvert.SerializeObject(inventory);
                        existingPlayer.Inventory = updatedInventory;
                        dbContext.SaveChanges();
                        UpdateInventory(player);
                    }
                    else
                    {
                    }
                }
            }
        }

        [EventHandler("core:changeStateVehicle")]
        public void ChangeStateVehicle([FromSource] Player player, int handle, string plate, int isLock)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                if (existingPlayer != null)
                {
                    var carsJson = existingPlayer.Cars;
                    var carsList = JsonConvert.DeserializeObject<List<VehicleInfo>>(carsJson);
                    bool plateExists = carsList.Any(car => car.Plate == plate);
                    if (plateExists)
                    {
                        TriggerClientEvent(player, "core:changeLockState", handle, plate, isLock);
                    }
                    else
                    {
                        TriggerClientEvent(player, "core:sendNotif", "~r~Ce n'est pas votre voiture");
                    }
                }
            }
        }

        [EventHandler("core:getPlayersList")]
        public void GetPlayersList()
        {
            var playersInstances = new List<PlayerInstance>();
            var playersHandle = new List<string>();

            using (var dbContext = new DataContext())
            {
                foreach (Player player in Players)
                {
                    var license = player.Identifiers["license"];
                    var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                    if (existingPlayer != null)
                    {
                        var playerInstance = new PlayerInstance
                        {
                            Id = existingPlayer.Id,
                            Discord = existingPlayer.Discord,
                            Firstname = existingPlayer.FirstName,
                            Lastname = existingPlayer.LastName,
                            Rank = existingPlayer.Rank,
                            Job = existingPlayer.Job,
                            Organisation = existingPlayer.Organisation,
                            Bitcoin = existingPlayer.Bitcoin,
                            Money = existingPlayer.Money,
                            Bills = existingPlayer.Bills,
                            Inventory = JsonConvert.DeserializeObject<List<ItemQuantity>>(existingPlayer.Inventory)
                        };
                        playersHandle.Add(player.Handle);
                        playersInstances.Add(playerInstance);
                    }
                }

                var jsonPlayerInstances = JsonConvert.SerializeObject(playersInstances);
                var jsonPlayersHandle = JsonConvert.SerializeObject(playersHandle);
                TriggerClientEvent("core:receivePlayers", jsonPlayerInstances, jsonPlayersHandle);
            }
        }

        [EventHandler("core:setRank")]
        public void SetRank([FromSource] Player player, int playerServerId, string rank)
        {
            Player targetPlayer = Players[playerServerId];
            using (var dbContext = new DataContext())
            {
                var license = targetPlayer.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    existingPlayer.Rank = rank;
                    dbContext.SaveChanges();
                    GetPlayerData(targetPlayer);
                }
            }
        }

        [EventHandler("core:warn")]
        public void Warn(int playerServerId, string message)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            TriggerClientEvent(targetPlayer, "core:sendNotif", $"~r~Vous avez reçu un avertissement:~s~ \n{message}");
        }

        [EventHandler("core:kick")]
        public void Kick([FromSource] Player source, int playerServerId, string message)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            targetPlayer.Drop(message);
        }

        [EventHandler("core:ban")]
        public void Ban([FromSource] Player source, int playerServerId, string message)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            BanRecord ban = new BanRecord(targetPlayer.Name, targetPlayer.Identifiers.ToList(), new DateTime(3000, 1, 1), message, source.Name, new Guid());

            BanManager.AddBan(ban);
            targetPlayer.Drop(message);
        }

        [EventHandler("core:heal")]
        public void HealPlayer([FromSource] Player source, int playerServerId, int value)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            SetPlayerInvincible(targetPlayer.Handle, false);
            TriggerClientEvent(targetPlayer, "core:setHealth", value);
        }

        [EventHandler("core:goto")]
        public void GoTo([FromSource] Player source, int playerServerId)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            var route = GetPlayerRoutingBucket(targetPlayer.Handle);
            SetPlayerRoutingBucket(source.Handle, route);
            source.Character.Position = targetPlayer.Character.Position;
        }

        [EventHandler("core:bringServer")]
        public void Bring(int playerServerId, int X, int Y, int Z)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            targetPlayer.Character.Position = new CitizenFX.Core.Vector3(X, Y, Z);
        }

        [EventHandler("core:jail")]
        public void Jail(int playerServerId, string reason, int time)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            targetPlayer.Character.Position = new CitizenFX.Core.Vector3(1643.1f, 2570.2f, 45.5f);
            TriggerClientEvent(targetPlayer, "core:sendNotif", $"~r~Vous êtes en jail\nRaison: ~s~{reason} ~r~durant ~s~{time}s");
        }

        [EventHandler("core:cuff")]
        public void Cuff(int playerServerId)
        {
            Player targetPlayer = Players[playerServerId];
            if (targetPlayer == null)
            {
                Debug.WriteLine("[Warning] Player not found.");
                return;
            }
            using (var dbContext = new DataContext())
            {
                var license = targetPlayer.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    var playerState = JsonConvert.DeserializeObject<PlayerState>(existingPlayer.State);
                    playerState.IsCuffed = !playerState.IsCuffed;
                    var updatedInventory = JsonConvert.SerializeObject(playerState);
                    existingPlayer.State = updatedInventory;
                    dbContext.SaveChanges();
                    GetPlayerData(targetPlayer);
                    TriggerClientEvent(targetPlayer, "core:cuffPlayer");
                }
            }
        }
    }
}