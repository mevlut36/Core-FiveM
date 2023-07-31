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
        private List<CitizenFX.Core.Vector3> PNJ = new List<CitizenFX.Core.Vector3>
        {
            new CitizenFX.Core.Vector3(188.4f, -889.3f, 30.6f),
            new CitizenFX.Core.Vector3(24.3f, -1346.6f, 29.3f),
            new CitizenFX.Core.Vector3(-46.4f, -1758.1f, 29.4f),
            new CitizenFX.Core.Vector3(-3243.2f, 999.9f, 12.7f),
            new CitizenFX.Core.Vector3(-3040.9f, 583.9f, 7.9f),
            new CitizenFX.Core.Vector3(-2966.1f, 390.3f, 15),
            new CitizenFX.Core.Vector3(-1819.2f, 792.6f, 138.1f),
            new CitizenFX.Core.Vector3(-1485.6f, -378.1f, 40f),
            new CitizenFX.Core.Vector3(2555.6f, 380.8f, 108.6f),
            new CitizenFX.Core.Vector3(1392.3f, 3606.4f, 34.9f),
            new CitizenFX.Core.Vector3(1696.9f, 4923.1f, 42),
            new CitizenFX.Core.Vector3(1165.2f, -323.6f, 69.2f),
            new CitizenFX.Core.Vector3(372.2f, 326.5f, 103.5f),
            new CitizenFX.Core.Vector3(-1222.2f, -908.8f, 12.3f),
            new CitizenFX.Core.Vector3(1133.6f, -981.7f, 46.4f),
            new CitizenFX.Core.Vector3(549.2f, 2670.9f, 42.1f),
            new CitizenFX.Core.Vector3(1727.6f, 6415.3f, 35),

            new CitizenFX.Core.Vector3(200.4f, -870.9f, 30.6f),
            new CitizenFX.Core.Vector3(73.9f, -1393.1f, 29.2f),
            new CitizenFX.Core.Vector3(427.2f, -805.9f, 29.4f),
            new CitizenFX.Core.Vector3(127.1f, -224.5f, 54.4f),
            new CitizenFX.Core.Vector3(-164.9f, -302, 39.6f),
            new CitizenFX.Core.Vector3(-708.3f, -152.6f, 37.4f),
            new CitizenFX.Core.Vector3(-823.3f, -1072.2f, 11.2f),
            new CitizenFX.Core.Vector3(-1193.8f, -766.5f, 17.2f),
            new CitizenFX.Core.Vector3(-3169.1f, 1042.7f, 20.7f),
            new CitizenFX.Core.Vector3(612.7f, 2763.4f, 42),
            new CitizenFX.Core.Vector3(1695.3f, 4823.3f, 42),
            new CitizenFX.Core.Vector3(1196.4f, 2711.7f, 38.1f),
            new CitizenFX.Core.Vector3(5.9f, 6511.3f, 31.7f)
        };

        public ServerMain()
        {
            Debug.WriteLine("Hi from Core.Server!");
            EventHandlers["core:playerSpawned"] += new Action<Player>(PlayerConnected);
            EventHandlers["playerDropped"] += new Action<Player>(IsPlayerDisconnecting);
            EventHandlers["core:kick"] += new Action<Player, int, string>(Kick);
        }

        public void PlayerConnected([FromSource] Player player)
        {
            using (var dbContext = new DataContext())
            {
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
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
                        Clothes = JsonConvert.DeserializeObject<List<ClothesInfo>>(existingPlayer.Clothes),
                        ClothesList = existingPlayer.ClothesList,
                        Money = existingPlayer.Money,
                        Bills = existingPlayer.Bills,
                        LastPosition = JsonConvert.DeserializeObject<Vector3>(existingPlayer.LastPosition),
                        Inventory = existingPlayer.Inventory
                    };

                    var json = JsonConvert.SerializeObject(playerInstance);
                    TriggerClientEvent(player, "core:playerConnected", json);
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
                        Clothes = JsonConvert.DeserializeObject<List<ClothesInfo>>(existingPlayer.Clothes),
                        ClothesList = existingPlayer.ClothesList,
                        Money = existingPlayer.Money,
                        Bills = existingPlayer.Bills,
                        Inventory = existingPlayer.Inventory
                    };

                    var json = JsonConvert.SerializeObject(playerInstance);
                    TriggerClientEvent(player, "core:getPlayerData", json);
                    TriggerClientEvent(player, "legal_client:getPlayerData", json);
                    TriggerClientEvent(player, "illegal_client:getPlayerData", json);
                }
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

                    GetPlayerData(player);
                    TriggerClientEvent(player, "core:sendNotif", $"Vous avez récupérer ~g~${dollars.Quantity}");
                }
                else
                {
                    Debug.WriteLine("Error in core:robbery: Player doesn't exist");
                }
            }
        }

        [EventHandler("core:bankRobbery")]
        public async void BankRobbery([FromSource] Player player, string bankName)
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
                    int randomNumber = random.Next(20000, 30000);

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

                        await Delay(10000);
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

                        GetPlayerData(player);
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
                    Skin = JsonConvert.SerializeObject(data.Skin),
                    FirstName = data.Firstname,
                    LastName = data.Lastname,
                    Rank = "player",
                    Job = JsonConvert.SerializeObject(defaultJob),
                    Organisation = "aucun",
                    Clothes = data.ClothesList,
                    ClothesList = data.ClothesList,
                    Money = 20000,
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
            var data = JsonConvert.DeserializeObject<ClothesInfo>(json);
            using (var dbContext = new DataContext())
            {
                var newClothes = dbContext.Player.FirstOrDefault(a => a.License == player.Identifiers["license"]);

                if (newClothes != null)
                {
                    var clothesList = JsonConvert.DeserializeObject<List<ClothesInfo>>(newClothes.Clothes);
                    var itemToUpdate = clothesList.FirstOrDefault(c => c.Component == data.Component);

                    if (itemToUpdate != null)
                    {
                        itemToUpdate.Drawable = data.Drawable;
                        itemToUpdate.Texture = data.Texture;
                        itemToUpdate.Palette = data.Palette;
                        newClothes.Clothes = JsonConvert.SerializeObject(clothesList);
                        dbContext.SaveChanges();
                        GetPlayerData(player);
                    }
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
                existingPlayer.LastPosition = json;
                dbContext.SaveChanges();
            }
            Debug.WriteLine($"{player.Name} disconnected");
        }

        [EventHandler("core:sendVehicleInfo")]
        public void SendVehicleInfo([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<List<VehicleInfo>>(json);
            var license = player.Identifiers["license"];
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(a => a.License == license);

                if (existingPlayer != null)
                {
                    List<VehicleInfo> cars;
                    if (!string.IsNullOrEmpty(existingPlayer.Cars))
                    {
                        cars = JsonConvert.DeserializeObject<List<VehicleInfo>>(existingPlayer.Cars);
                    }
                    else
                    {
                        cars = new List<VehicleInfo>();
                    }

                    cars.AddRange(data);

                    existingPlayer.Cars = JsonConvert.SerializeObject(cars);
                    dbContext.SaveChanges();
                    GetPlayerData(player);
                }

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

        [EventHandler("core:getPlayerMoney")]
        public void GetPlayerMoney([FromSource] Player player)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);
                TriggerClientEvent(player, "core:receivePlayerMoney", existingPlayer.Money);
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
                    GetPlayerData(player);
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
                    GetPlayerData(player);
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
                    GetPlayerData(player);
                }
            }
        }

        [EventHandler("core:addItemInBoot")]
        public void AddItemInBoot([FromSource] Player player, string plate, string item, int quantity, string type)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);

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

                                var itemFilter = boot.FirstOrDefault(i => i.Item == item);
                                if (itemFilter != null)
                                {
                                    itemFilter.Quantity += quantity;
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
                            }
                        }
                    }

                    var updatedCars = JsonConvert.SerializeObject(vehicles);
                    existingPlayer.Cars = updatedCars;
                    dbContext.SaveChanges();
                }
            }
        }

        [EventHandler("core:removeItemFromBoot")]
        public void RemoveItemFromBoot([FromSource] Player player, string plate, string item, int quantity)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);

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

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var updatedCars = JsonConvert.SerializeObject(vehicles);
                    existingPlayer.Cars = updatedCars;
                    dbContext.SaveChanges();
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
                        GetPlayerData(player);
                        GetPlayerData(targetPlayer);
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
                        GetPlayerData(player);
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
                            Skin = existingPlayer.Skin,
                            Firstname = existingPlayer.FirstName,
                            Lastname = existingPlayer.LastName,
                            Rank = existingPlayer.Rank,
                            Job = existingPlayer.Job,
                            Organisation = existingPlayer.Organisation,
                            Bitcoin = existingPlayer.Bitcoin,
                            Birth = existingPlayer.Birth,
                            Clothes = JsonConvert.DeserializeObject<List<ClothesInfo>>(existingPlayer.Clothes),
                            ClothesList = existingPlayer.ClothesList,
                            Money = existingPlayer.Money,
                            Bills = existingPlayer.Bills,
                            Inventory = existingPlayer.Inventory
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
            if (player.Identifiers["license"] == "b88815828af00440aae4cc0551617f2de2b49fb4")
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
            else
            {
                TriggerClientEvent("core:sendNotif", "~r~Désolé vous n'êtes pas Mevlut");
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
            TriggerClientEvent("core:sendNotif", $"~r~Vous êtes en jail\nRaison: ~s~{reason} ~r~durant ~s~{time}s");
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