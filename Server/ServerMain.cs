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
        public string Birth { get; set; }
        public string Clothes { get; set; }
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

        [EventHandler("core:sendPlayerData")]
        public void SendPlayerData([FromSource] Player player, string json)
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
                TriggerClientEvent(player, "core:sendNotif", $"Vos informations ont bien été enregistré", newClothes.License);
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

                TriggerClientEvent(player, "core:sendNotif", "Vos informations ont bien été enregistrées", existingPlayer.License);
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

    }
}
