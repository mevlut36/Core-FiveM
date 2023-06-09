using CitizenFX.Core;
using CitizenFX.Core.Native;
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
        public ObjectPool Pool = new ObjectPool();
        public Format(ClientMain caller)
        {
            Pool = caller.Pool;
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
            API.BeginTextCommandThefeedPost("STRING");
            API.AddTextComponentSubstringPlayerName(text);
            API.EndTextCommandThefeedPostTicker(false, true);
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

        public async void PlayAnimation(string animDict, string animName, int duration)
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict)) await BaseScript.Delay(50);
            Game.PlayerPed.Task.ClearAllImmediately();
            Game.PlayerPed.Task.PlayAnimation(animDict, animName, -1, duration, 50);
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

        /*
         * Check if date of birth is in valid format
         * 
         * I don't want to hear any more about it
         */
        public bool IsValidDateFormat(string date)
        {
            if (date.Length != 10)
            {
                return false;
            }

            string[] parts = date.Split('/');

            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out int day) || !int.TryParse(parts[1], out int month) || !int.TryParse(parts[2], out int year))
            {
                return false;
            }

            return IsValidDate(day, month, year);
        }

        private bool IsValidDate(int day, int month, int year)
        {
            if (year < 1945 || year > 2015)
            {
                return false;
            }

            if (month < 1 || month > 12)
            {
                return false;
            }

            int[] maxDaysPerMonth = new int[]
            {
        31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
            };

            if (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0))
            {
                maxDaysPerMonth[1] = 29;
            }

            if (day < 1 || day > maxDaysPerMonth[month - 1])
            {
                return false;
            }

            return true;
        }

        public static DisplayAttribute GetDisplayAttribute(VehicleImport vehicle)
        {
            var type = typeof(VehicleImport);
            var memberInfo = type.GetMember(vehicle.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

            return (DisplayAttribute)attributes[0];
        }
    }
}
