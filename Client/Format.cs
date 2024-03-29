﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Core.Shared;
using System.Drawing;

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

        public static Notification ShowAdvancedNotification(string title, string subtitle, string text, string icon = "CHAR_STRETCH", Color flashColor = new Color(), bool blink = false, NotificationType type = NotificationType.Default, bool showInBrief = true, bool sound = true)
        {
            AddTextEntry("ScaleformUIAdvancedNotification", text);
            BeginTextCommandThefeedPost("ScaleformUIAdvancedNotification");
            AddTextComponentSubstringPlayerName(text);
            SetNotificationBackgroundColor(140);
            if (!flashColor.IsEmpty && !blink)
                SetNotificationFlashColor(flashColor.R, flashColor.G, flashColor.B, flashColor.A);
            if (sound) Audio.PlaySoundFrontend("DELETE", "HUD_DEATHMATCH_SOUNDSET");
            return new Notification(EndTextCommandThefeedPostMessagetext(icon, icon, true, (int)type, title, subtitle));
            //return new Notification(EndTextCommandThefeedPostTicker(blink, showInBrief));
        }

        public sealed class Notification
        {
            #region Fields
            int _handle;
            #endregion

            internal Notification(int handle)
            {
                _handle = handle;
            }
            public void Hide()
            {
                ThefeedRemoveItem(_handle);
            }
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

        public async void PlayAnimation(string animDict, string animName, float speed, AnimationFlags flags)
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict)) await BaseScript.Delay(50);
            Game.PlayerPed.Task.PlayAnimation(animDict, animName, speed, -1, flags);
        }

        public void StopAnimation(string animDict, string animName)
        {
            if (Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Game.PlayerPed.Handle, animDict, animName, 3))
            {
                Game.PlayerPed.Task.ClearAnimation(animDict, animName);
            }
        }

        public async Task AddPropToPlayer(string prop1, int bone, float off1, float off2, float off3, float rot1, float rot2, float rot3, int duration)
        {
            int player = PlayerPedId();
            Vector3 playerCoords = GetEntityCoords(player, true);

            RequestModel((uint)GetHashKey(prop1));

            int prop = CreateObject(GetHashKey(prop1), playerCoords.X, playerCoords.Y, playerCoords.Z + 0.2f, true, true, true);
            AttachEntityToEntity(prop, player, GetPedBoneIndex(player, bone), off1, off2, off3, rot1, rot2, rot3, true, true, false, true, 1, true);

            await BaseScript.Delay(duration);

            DeleteEntity(ref prop);

            SetModelAsNoLongerNeeded((uint)GetHashKey(prop1));
        }

        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            var resourceName = GetCurrentResourceName().ToUpper();
            string title = $"{windowTitle ?? "Enter"}:\t(MAX {maxInputLength} Characters)";

            AddTextEntry($"{resourceName}_WINDOW_TITLE", title);
            DisplayOnscreenKeyboard(1, $"{resourceName}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);

            if (string.IsNullOrEmpty(defaultText))
            {
                defaultText = "";
            }

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
                        await BaseScript.Delay(50); // Wait 50 ms before next iteration
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
                ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Le format n'est pas valide");
                return false;
            }

            string[] parts = date.Split('/');

            if (parts.Length != 3)
            {
                ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "Il manque le mois jour ou année");
                return false;
            }

            if (!int.TryParse(parts[0], out int day) || !int.TryParse(parts[1], out int month) || !int.TryParse(parts[2], out int year))
            {
                ShowAdvancedNotification("ShurikenRP", "ShurikenCore", "C'est bon chef");
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
