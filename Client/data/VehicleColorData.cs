using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CitizenFX.Core;

namespace ShurikenLegal.Client.data
{
    public class VehicleColorItem
    {
        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Hex")]
        public string Hex { get; set; }

        [JsonProperty("RGB")]
        public string RGB { get; set; }
    }

    public static class VehicleColorData
    {
        private static List<VehicleColorItem> _colors;
        private static bool _initialized = false;

        public static List<VehicleColorItem> GetColors()
        {
            if (!_initialized)
            {
                LoadColors();
            }
            return _colors ?? new List<VehicleColorItem>();
        }

        private static void LoadColors()
        {
            try
            {
                string json = CitizenFX.Core.Native.API.LoadResourceFile("ShurikenLegal", "VehicleColors.json");

                if (!string.IsNullOrEmpty(json))
                {
                    _colors = JsonConvert.DeserializeObject<List<VehicleColorItem>>(json);
                    _initialized = true;
                    Debug.WriteLine($"[VehicleColorData] {_colors.Count} couleurs chargées");
                }
                else
                {
                    Debug.WriteLine("[VehicleColorData] VehicleColors.json est vide ou introuvable");
                    _colors = new List<VehicleColorItem>();
                    _initialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehicleColorData] Erreur: {ex.Message}");
                _colors = new List<VehicleColorItem>();
                _initialized = true;
            }
        }
    }
}