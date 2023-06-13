using CitizenFX.Core;
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

        [JsonProperty("type")]
        public string Type { get; set; }
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
        public List<BootInfo> Boot { get; set; }
        public string Plate { get; set; }
        public int EngineLevel { get; set; }
        public int BrakeLevel { get; set; }
        public int ColorPrimary { get; set; }
        public int ColorSecondary { get; set; }
    }
    public class BootInfo
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
    }

    public class LTDItems
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public LTDItems(string name, string description, int price)
        {
            Name = name;
            Description = description;
            Price = price;
        }
    }

    public class AmmuNationInfo
    {
        public string AmmuNationName { get; set; }
        public Vector3 Checkout { get; set; }
        public Vector3 PNJCoords { get; set; }
        public AmmuNationInfo(string ammuNationName, Vector3 checkout, Vector3 pnjCoords)
        {
            AmmuNationName = ammuNationName;
            Checkout = checkout;
            PNJCoords = pnjCoords;
        }
    }

    public class BankInfo
    {
        public string BankName { get; set; }
        public Vector3 Checkout { get; set; }
        public Vector3 Robbery { get; set; }

        public BankInfo(string bankName, Vector3 checkout, Vector3 robbery)
        {
            BankName = bankName;
            Checkout = checkout;
            Robbery = robbery;
        }
    }

    public class LTDShopInfo
    {
        public string LTDName { get; set; }
        public Vector3 Checkout { get; set; }
        public Vector3 PNJCoords { get; set; }
        public Vector3 ATM { get; set; }

        public LTDShopInfo(string ltdName, Vector3 checkout, Vector3 pnjCoords, Vector3 atm)
        {
            LTDName = ltdName;
            Checkout = checkout;
            PNJCoords = pnjCoords;
            ATM = atm;
        }
    }
    public class ClothShopInfo
    {
        public string ShopName { get; set; }
        public Vector3 Checkout { get; set; }
        public Vector3 PNJCoords { get; set; }

        public ClothShopInfo(string shopName, Vector3 checkout, Vector3 pnjCoords)
        {
            ShopName = shopName;
            Checkout = checkout;
            PNJCoords = pnjCoords;
        }
    }
}
