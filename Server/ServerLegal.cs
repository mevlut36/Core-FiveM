using CitizenFX.Core;
using Core.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Core.Server
{
    public class ServerLegal : ServerMain
    {
        public ServerLegal()
        {
            Debug.WriteLine("Hi from Core.ServerLegal!");
            EventHandlers["legal_server:addItemInChest"] += new Action<Player, int, string, int, string>(AddItemInChest);
            EventHandlers["legal_server:removeItemFromChest"] += new Action<Player, int, string>(RemoveItemFromChest);
            EventHandlers["playerDropped"] += new Action<Player>(IsPlayerDisconnecting);
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
                    GetPlayerData(player);
                }
            }
        }

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
                    GetPlayerData(player);
                }
            }
        }
    }
}
