using CitizenFX.Core;
using Core.Client;
using LemonUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace Core.Client
{
    public class Format
    {
        public ClientMain Client;
        public Format(ClientMain caller)
        {
            Client = caller;
        }
        /*
         * Format JSON Request from server-side
         */
        public static List<string> SplitJsonObjects(string jsonString)
        {
            jsonString = jsonString.Replace("}{", "}|{");
            jsonString = jsonString.Replace("}\n{", "}\r\n{");
            string[] jsonObjectsArray = jsonString.Split('|');
            List<string> jsonObjectsList = new List<string>(jsonObjectsArray);
            return jsonObjectsList;
        }

        /*
         * Notification style
         * Parameter: Text entry
         */
        public void SendNotif(string text)
        {
            BeginTextCommandThefeedPost("STRING");
            AddTextComponentString(text);
            EndTextCommandThefeedPostTicker(true, true);
        }

        public static void CustomNotif(string message, bool blink = true, bool saveToBrief = true)
        {
            SetNotificationTextEntry("CELL_EMAIL_BCON");
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                AddTextComponentSubstringPlayerName(s);
            }
            DrawNotification(blink, saveToBrief);
        }

        /*
         * TextUI Style
         * Parameter: Text entry
         */
        public void SendTextUI(string text)
        {
            SetTextFont(6);
            SetTextScale(0.5f, 0.5f);
            SetTextProportional(false);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextCentre(false);
            SetTextJustification(0);
            SetTextEntry("STRING");
            AddTextComponentString($"{text}");
            int x = 0, y = 0;
            GetScreenActiveResolution(ref x, ref y);
            DrawText(0.50f, 0.80f);
        }

        /*
         * Create a quoicouMarker
         * Parameter : position type Vector3    
         */
        public void SetMarker(Vector3 position, MarkerType markerType)
        {
            World.DrawMarker(markerType, position, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(255, 130, 0), true);
        }

        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // THANKS vMENU
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await BaseScript.Delay(0);

            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3:
                    case 2:
                        return null;
                    case 1:
                        return GetOnscreenKeyboardResult();
                    default:
                        await BaseScript.Delay(0);
                        break;
                }
            }
        }
    }
}
