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

namespace Core.Server
{
    public class PlayerInstance
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Birth { get; set; }
        public string Clothes { get; set; }
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
                var license = player.Identifiers["license"];
                var existingPlayer = dbContext.Player.FirstOrDefault(p => p.License == license);
                if (existingPlayer != null)
                {
                    TriggerClientEvent("core:sendNotif", $"~g~Bienvenue ~w~{existingPlayer.FirstName}");
                    TriggerClientEvent("core:getClothes", existingPlayer.Clothes);
                    TriggerClientEvent("core:teleportLastPosition", existingPlayer.LastPosition);
                }
                else
                {
                    TriggerClientEvent("core:createCharacter");
                    SetPlayerRoutingBucket(player.Handle, Int32.Parse(networkId));
                }
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

        [EventHandler("core:sendPlayerData")]
        public void SendPlayerData([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<PlayerInstance>(json);
            using (var dbContext = new DataContext())
            {
                var newPlayer = new PlayerTable
                {
                    Id = player.Character.NetworkId,
                    License = player.Identifiers["license"],

                    FirstName = data.Firstname,
                    LastName = data.Lastname,
                    Clothes = data.Clothes,
                    Birth = data.Birth,
                };
                dbContext.Player.Add(newPlayer);
                dbContext.SaveChanges();
                TriggerClientEvent("core:sendNotif", $"Vos informations ont bien �t� enregistr�");
            }
        }

        [EventHandler("core:updateClothes")]
        public void UpdateClothes([FromSource] Player player, string json)
        {
            var data = JsonConvert.DeserializeObject<PlayerInstance>(json);
            using (var dbContext = new DataContext())
            {
                var newClothes = dbContext.Player.SingleOrDefault(a => a.License == player.Identifiers["license"]);

                if (newClothes != null)
                {
                    newClothes.Clothes = data.Clothes;
                    dbContext.SaveChanges();
                }
                TriggerClientEvent("core:sendNotif", $"Vos informations ont bien �t� enregistr�");
            }
        }

    }
}