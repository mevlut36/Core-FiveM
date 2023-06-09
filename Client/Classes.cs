using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Client
{
    public class ItemQuantity
    {
        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class Weapon
    {
        [JsonProperty("weapon")]
        public string WeaponName { get; set;}
    }

    public class Bills
    {
        [JsonProperty("company")]
        public string Company { get; set; }
        [JsonProperty("amount")]
        public int Amount { get; set;}
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
    }

    public class PlayerInstance
    {
        public string Gender { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int Bitcoin { get; set; }
        public string Birth { get; set; }
        public string Clothes { get; set; }
        public int Money { get; set; }
        public string Bills { get; set; }
        public string Inventory { get; set; }
    }
    public class VehicleInfo
    {
        public string Model { get; set; }
        public string Plate { get; set; }
        public int EngineLevel { get; set; }
        public int BrakeLevel { get; set; }
        public int ColorPrimary { get; set; }
        public int ColorSecondary { get; set; }
    }
}
