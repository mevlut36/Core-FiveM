using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CitizenFX.Core;
using System.Linq;
using System.Collections.Generic;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using System.Dynamic;
using System.Numerics;
using System.Text;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

namespace Core.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from Core.Server!");
            EventHandlers["playerDropped"] += new Action<Player, string>(IsPlayerDisconnecting);
            EventHandlers["core:kick"] += new Action<Player, int, string>(Kick);
        }

        [EventHandler("core:isPlayerRegistered")]
        public void IsPlayerRegistered([FromSource] Player player)
        {
            using (var dbContext = new DataContext())
            {
                var networkId = player.Handle;
                var playerPed = GetPlayerPed(networkId);
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    TriggerClientEvent(player, "core:getSkin", existingPlayer.Skin);
                    TriggerClientEvent(player, "core:sendNotif", $"~g~Bienvenue ~w~{existingPlayer.FirstName}");
                    TriggerClientEvent(player, "core:teleportLastPosition", existingPlayer.LastPosition);
                }
                else
                {
                    TriggerClientEvent(player, "core:createCharacter");
                    SetPlayerRoutingBucket(player.Handle, Int32.Parse(networkId));
                }
            }
        }

        [EventHandler("core:requestPlayerData")]
        public void GetPlayerData([FromSource] Player player)
        {
            using (var dbContext = new DataContext())
            {
                var networkId = player.Handle;
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    var playerInstance = new PlayerInstance
                    {
                        Id = existingPlayer.Id,
                        Skin = existingPlayer.Skin,
                        Firstname = existingPlayer.FirstName,
                        Lastname = existingPlayer.LastName,
                        Rank = existingPlayer.Rank,
                        Job = existingPlayer.Job,
                        Organisation = existingPlayer.Organisation,
                        Bitcoin = existingPlayer.Bitcoin,
                        Birth = existingPlayer.Birth,
                        Clothes = existingPlayer.Clothes,
                        ClothesList = existingPlayer.ClothesList,
                        Money = existingPlayer.Money,
                        Bills = existingPlayer.Bills,
                        Inventory = existingPlayer.Inventory
                    };

                    var json = JsonConvert.SerializeObject(playerInstance);
                    TriggerClientEvent(player, "core:getPlayerData", json);
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
                var newPlayer = new PlayerTable
                {
                    License = player.Identifiers["license"],
                    Skin = data.Skin,
                    FirstName = data.Firstname,
                    LastName = data.Lastname,
                    Rank = "player",
                    Job = "chomage",
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
                }
                TriggerClientEvent(player, "core:sendNotif", $"Vos informations ont bien été enregistré");
            }
        }

        public void IsPlayerDisconnecting([FromSource] Player player, string reason)
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
                if (existingPlayer != null)
                {
                    var bills = JsonConvert.DeserializeObject<List<Bills>>(existingPlayer.Bills);

                    var selectedBill = bills.FirstOrDefault(b => b.Company == company && b.Amount == amount);
                    if (selectedBill != null)
                    {
                        bills.Remove(selectedBill);
                        existingPlayer.Bills = JsonConvert.SerializeObject(bills);
                        dbContext.SaveChanges();
                    }
                }
            }
            TriggerClientEvent(player, "core:sendNotif", $"~gs~Vous avez bien payé la facture");
            GetPlayerData(player);
        }

        // TODO
        [EventHandler("core:addBill")]
        public void AddBill([FromSource] Player player)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    existingPlayer.Bills += "Add";
                }
            }
            TriggerClientEvent(player, "core:sendNotif", $"Vous avez reçu une facture.");
        }

        [EventHandler("core:addItem")]
        public void AddItem([FromSource] Player player, string item, int quantity)
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
                    GetPlayerData(player);
                    dbContext.SaveChanges();
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
                    GetPlayerData(player);
                    dbContext.SaveChanges();
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
                        GetPlayerData(player);
                        dbContext.SaveChanges();
                    } else
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
                            Skin = existingPlayer.Skin,
                            Firstname = existingPlayer.FirstName,
                            Lastname = existingPlayer.LastName,
                            Rank = existingPlayer.Rank,
                            Job = existingPlayer.Job,
                            Organisation = existingPlayer.Organisation,
                            Bitcoin = existingPlayer.Bitcoin,
                            Birth = existingPlayer.Birth,
                            Clothes = existingPlayer.Clothes,
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
            BanManager.BanRecord ban = new BanManager.BanRecord(targetPlayer.Name, targetPlayer.Identifiers.ToList(), new DateTime(3000, 1, 1), message, source.Name, new Guid());

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
    }
}