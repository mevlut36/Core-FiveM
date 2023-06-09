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

namespace Core.Server
{
    public class PlayerInstance
    {
        public string Gender { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int Bitcoin { get; set; }
        public string Birth { get; set; }
        public string Clothes { get; set; }
        public int Money { get; set; }
        public string Bills { get; set; }
        public string Inventory { get; set; }
    }
    public class VehicleInfo
    {
        public string Model { get; set; }
        public string Plate { get; set; }
        public int EngineLevel { get; set; }
        public int BrakeLevel { get; set; }
        public int ColorPrimary { get; set; }
        public int ColorSecondary { get; set; }
    }

    public class Bills
    {
        [JsonProperty("company")]
        public string Company { get; set; }
        [JsonProperty("amount")]
        public int Amount { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
    }
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from Core.Server!");
            EventHandlers["playerDropped"] += new Action<Player, string>(IsPlayerDisconnecting);
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
                    TriggerClientEvent(player, "core:getGender", existingPlayer.Gender);
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
                        Firstname = existingPlayer.FirstName,
                        Lastname = existingPlayer.LastName,
                        Bitcoin = existingPlayer.Bitcoin,
                        Birth = existingPlayer.Birth,
                        Clothes = existingPlayer.Clothes,
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
                    TriggerClientEvent(player, "core:getClothes", existingPlayer.Clothes, existingPlayer.Gender);
                }
            }
        }

        // rename for registerPlayerData
        [EventHandler("core:registerPlayerData")]
        public void RegisterPlayerData([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<PlayerInstance>(json);
            using (var dbContext = new DataContext())
            {
                var newPlayer = new PlayerTable
                {
                    License = player.Identifiers["license"],
                    Gender = data.Gender,
                    FirstName = data.Firstname,
                    LastName = data.Lastname,
                    Clothes = data.Clothes,
                    Money = data.Money,
                    Birth = data.Birth,
                };
                dbContext.Player.Add(newPlayer);
                dbContext.SaveChanges();
                SetPlayerRoutingBucket(player.Handle, 0);
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
                    newClothes.Clothes = data.Clothes;
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
        public void Transaction([FromSource] Player player, int cost)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    if(existingPlayer.Money >= cost)
                    {
                        existingPlayer.Money = existingPlayer.Money - cost;
                        TriggerClientEvent(player, "core:sendNotif", $"Vous avez payé ~r~${cost}~w~.");
                        dbContext.SaveChanges();
                    } else
                    {
                        TriggerClientEvent(player, "core:sendNotif", $"~r~Vous n'avez pas assez d'argent.");
                    }
                    
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

        [EventHandler("core:addWeapon")]
        public void AddWeapon([FromSource] Player player, string weapon)
        {
            var license = player.Identifiers["license"];
            var networkId = player.Handle;
            var playerPed = GetPlayerPed(networkId);
            using (var dbContext = new DataContext())
            {
                var existingPlayer = dbContext.Player.FirstOrDefault(u => u.License == license);

                if (existingPlayer != null)
                {
                    var inventory = JsonConvert.DeserializeObject<List<dynamic>>(existingPlayer.Inventory);

                    var newWeapon = new
                    {
                        weapon = weapon
                    };

                    inventory.Add(newWeapon);
                    var updatedInventory = JsonConvert.SerializeObject(inventory);

                    existingPlayer.Inventory = updatedInventory;
                    dbContext.SaveChanges();
                }
            }
        }



    }
}
