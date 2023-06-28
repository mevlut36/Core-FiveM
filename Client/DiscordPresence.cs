using System.Threading;
using CitizenFX.Core;
using LemonUI;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class DiscordPresence
    {
        PlayerMenu PlayerMenu;
        public readonly string DiscordInvite = "https://discord.gg/discord";
        private readonly Timer presenceTimer;

        public DiscordPresence(ClientMain caller)
        {
            PlayerMenu = caller.PlayerMenu;
            SetDiscordAppId("");
            SetDiscordRichPresenceAction(0, "Rejoins notre discord!", DiscordInvite);
            SetDiscordRichPresenceAsset("scantrad_fivem");
            presenceTimer = new Timer(UpdatePresence, null, 0, 10000);
        }

        public void UpdatePresence(object stateInfo)
        {
            SetDiscordRichPresenceAssetText($"ID:{PlayerMenu.PlayerInst.Firstname} {PlayerMenu.PlayerInst.Lastname}");
        }
    }
}
