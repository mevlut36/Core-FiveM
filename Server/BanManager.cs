using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Core.Server
{
    public class BanManager : BaseScript
    {
        private List<BanRecord> banList = new List<BanRecord>();

        public class BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
            public Guid uuid;

            public BanRecord(string playerName, List<string> identifiers, DateTime bannedUntil, string banReason, string bannedBy, Guid uuid)
            {
                this.playerName = playerName;
                this.identifiers = identifiers;
                this.bannedUntil = bannedUntil;
                this.banReason = banReason;
                string uuidSuffix = $"\nYour ban id: {uuid}";
                if (!this.banReason.Contains(uuidSuffix) && uuid != Guid.Empty)
                {
                    this.banReason += uuidSuffix;
                }
                this.bannedBy = bannedBy;
                this.uuid = uuid;
            }
        }

        public BanManager()
        {
            EventHandlers.Add("core:TempBanPlayer", new Action<int, double, string>(BanPlayer));
            EventHandlers.Add("core:PermBanPlayer", new Action<int, string>(BanPlayer));
            // EventHandlers.Add("playerConnecting", new Action<Player, string, CallbackDelegate>(CheckForBans));
            EventHandlers.Add("core:RequestPlayerUnban", new Action<BanRecord>(RemoveBan));
            EventHandlers.Add("core:RequestBanList", new Action<Player>(SendBanList));
        }

        private void BanPlayer(int targetPlayer, string banReason)
        {
            BanPlayer(targetPlayer, -1.0, banReason);
        }

        private void BanPlayer(int targetPlayerId, double duration, string reason)
        {
            Player targetPlayer = Players[targetPlayerId];
            var playerName = targetPlayer.Name;
            var identifiers = new List<string> { targetPlayer.Identifiers["licence"] };
            var bannedBy = "System";
            var uuid = Guid.NewGuid();

            var bannedUntil = duration > 0 ? DateTime.Now.AddMinutes(duration) : DateTime.MaxValue;

            banList.Add(new BanRecord(playerName, identifiers, bannedUntil, reason, bannedBy, uuid));
            targetPlayer.Drop($"Tu as été banni pour {reason} jusqu'a {bannedUntil}");
        }

        internal static void AddBan(BanRecord ban)
        {
            string existingRecord = GetResourceKvpString(ban.uuid.ToString());
            if (string.IsNullOrEmpty(existingRecord))
            {
                SetResourceKvp($"core_ban_{ban.uuid}", JsonConvert.SerializeObject(ban));
            }
        }

        private void CheckForBans(Player player, string playerName, CallbackDelegate kickCallback)
        {
            var identifier = player.Identifiers["licence"];
            var banRecord = banList.FirstOrDefault(record => record.identifiers.Contains(identifier));

            if (banRecord != null && DateTime.Now <= banRecord.bannedUntil)
            {
                kickCallback($"You are banned until {banRecord.bannedUntil}. Ban reason: {banRecord.banReason}");
            }
        }

        internal static void RemoveBan(BanRecord ban)
        {
            string existingRecord = GetResourceKvpString(ban.uuid.ToString());
            if (string.IsNullOrEmpty(existingRecord))
            {
                DeleteResourceKvp($"core_ban_{ban.uuid}");
            }
        }

        private void SendBanList(Player admin)
        {
            TriggerClientEvent(admin, "core:ReceiveBanList", banList);
        }
    }

}