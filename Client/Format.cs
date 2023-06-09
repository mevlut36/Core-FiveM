﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Core.Client
{
    public class Format
    {
        public Format(ClientMain caller)
        {
            
        }

        public List<string> parentFace = new List<string>
        {
            "00M Benjamin",
            "01M Daniel",
            "02M Joshua",
            "03M Noah",
            "04M Andrew",
            "05M Joan",
            "06M Alex",
            "07M Isaac",
            "08M Evan",
            "09M Ethan",
            "10M Vincent",
            "11M Angel",
            "12M Diego",
            "13M Adrian",
            "14M Gabriel",
            "15M Michael",
            "16M Santiago",
            "17M Kevin",
            "18M Louis",
            "19M Samuel",
            "20M Anthony",
            "42M John",
            "43M Niko",
            "44M Claude",
            "21F Hannah",
            "22F Audrey",
            "23F Jasmine",
            "24F Giselle",
            "25F Amelia",
            "26F Isabella",
            "27F Zoe",
            "28F Ava",
            "29F Camilla",
            "30F Violet",
            "31F Sophia",
            "32F Eveline",
            "33F Nicole",
            "34F Ashley",
            "35F Grace",
            "36F Brianna",
            "37F Natalie",
            "38F Olivia",
            "39F Elizabeth",
            "40F Charlotte",
            "41F Emma",
            "45F Misty"
        };

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
                SendNotif("Le format n'est pas valide");
                return false;
            }

            string[] parts = date.Split('/');

            if (parts.Length != 3)
            {
                SendNotif("Il manque le mois jour ou année");
                return false;
            }

            if (!int.TryParse(parts[0], out int day) || !int.TryParse(parts[1], out int month) || !int.TryParse(parts[2], out int year))
            {
                SendNotif("C'est bon chef");
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
