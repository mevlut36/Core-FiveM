using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using CitizenFX.Core;

namespace ShurikenLegal.Client.data
{
    public class VehicleWheelItem
    {
        [JsonProperty("WheelType")]
        public int WheelType { get; set; }

        [JsonProperty("Wheel")]
        public string Wheel { get; set; }

        [JsonProperty("vID")]
        public int vID { get; set; }
    }

    public static class VehicleWheelData
    {
        private static List<VehicleWheelItem> _wheels;
        private static bool _initialized = false;

        public static List<VehicleWheelItem> GetWheels()
        {
            if (!_initialized)
            {
                LoadWheels();
            }
            return _wheels ?? new List<VehicleWheelItem>();
        }

        public static List<VehicleWheelItem> GetWheelsByType(int wheelType)
        {
            if (!_initialized)
            {
                LoadWheels();
            }
            return _wheels?.Where(w => w.WheelType == wheelType).ToList() ?? new List<VehicleWheelItem>();
        }

        private static void LoadWheels()
        {
            try
            {
                string json = CitizenFX.Core.Native.API.LoadResourceFile("ShurikenLegal", "VehicleWheels.json");

                if (!string.IsNullOrEmpty(json))
                {
                    _wheels = JsonConvert.DeserializeObject<List<VehicleWheelItem>>(json);
                    _initialized = true;
                    Debug.WriteLine($"[VehicleWheelData] {_wheels.Count} roues chargées");
                }
                else
                {
                    Debug.WriteLine("[VehicleWheelData] VehicleWheels.json est vide ou introuvable");
                    _wheels = new List<VehicleWheelItem>();
                    _initialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VehicleWheelData] Erreur: {ex.Message}");
                _wheels = new List<VehicleWheelItem>();
                _initialized = true;
            }
        }
    }
}