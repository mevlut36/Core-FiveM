using System.Threading;
using CitizenFX.Core;
using LemonUI;
using static CitizenFX.Core.Native.API;
using Core.Shared;

namespace Core.Client
{
    public class DiscordPresence
    {
        PlayerMenu PlayerMenu;
        public readonly string DiscordAppId = "980213934432874547";
        public readonly string DiscordInvite = "https://discord.gg/HG6TqUHAdc";
        public readonly string FiveM = "fivem://connect/cfx.re/join/3xpo7b";
        private readonly Timer presenceTimer;

        public DiscordPresence(ClientMain caller)
        {
            PlayerMenu = caller.PlayerMenu;
            SetDiscordAppId("1016391752397115392");
            SetDiscordRichPresenceAction(0, "Rejoins notre discord !", DiscordInvite);
            SetDiscordRichPresenceAction(1, "Se connecter à FiveM", FiveM);
            SetDiscordRichPresenceAsset("logo");
            presenceTimer = new Timer(UpdatePresence, null, 0, 10000);
        }

        public void UpdatePresence(object stateInfo)
        {
            SetDiscordRichPresenceAssetText($"{PlayerMenu.PlayerInst.Firstname} {PlayerMenu.PlayerInst.Lastname}");
        }
    }
}
