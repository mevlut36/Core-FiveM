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
        public string ItemType { get; set; }
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

    public class ClothesInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("component")]
        public int Component { get; set; }
        [JsonProperty("drawable")]
        public int Drawable { get; set; }
        [JsonProperty("texture")]
        public int Texture { get; set; }
        [JsonProperty("palette")]
        public int Palette { get; set; }
    }

    public class PlayerInstance
    {
        public int Id { get; set; }
        public string Skin { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Rank { get; set; }
        public string Job { get; set; }
        public string Organisation { get; set; }
        public int Bitcoin { get; set; }
        public string Cars { get; set; }
        public string Birth { get; set; }
        public string Clothes { get; set; }
        public string ClothesList { get; set; }
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

    public class SkinInfo
    {
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("dad")]
        public int Dad { get; set; }

        [JsonProperty("mom")]
        public int Mom { get; set; }

        [JsonProperty("dadMomPercent")]
        public int DadMomPercent { get; set; }

        [JsonProperty("eyecolor")]
        public int EyeColor { get; set; }

        [JsonProperty("eyebrow")]
        public int Eyebrow { get; set; }

        [JsonProperty("eyebrowOpacity")]
        public int EyebrowOpacity { get; set; }

        [JsonProperty("beard")]
        public int Beard { get; set; }

        [JsonProperty("beardOpacity")]
        public int BeardOpacity { get; set; }

        [JsonProperty("beardColor")]
        public int BeardColor { get; set; }

        [JsonProperty("hair")]
        public int Hair { get; set; }

        [JsonProperty("hairText")]
        public int HairColor { get; set; }
    }

    public class ReportClass
    {
        public int Id { get; set; }
        public PlayerInstance Player { get; set; }
        public string Text { get; set; }
        public bool IsResolved { get; set; }
    }
}
