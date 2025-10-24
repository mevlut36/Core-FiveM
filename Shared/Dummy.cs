using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CitizenFX.Core;
using System.Threading.Tasks;

namespace Core.Shared
{
    class Shared
    {

    }

    public class ItemQuantity
    {
        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("type")]
        public string ItemType { get; set; }
    }

    class JobInfo
    {
        [JsonProperty("id")]
        public int JobID { get; set; }
        [JsonProperty("rank")]
        public int JobRank { get; set; }
    }

    public class Bills
    {
        [JsonProperty("company")]
        public string Company { get; set; }
        [JsonProperty("amount")]
        public int Amount { get; set; }
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

    public class PlayerState
    {
        [JsonProperty("health")]
        public int Health { get; set; }
        [JsonProperty("thirst")]
        public int Thirst { get; set; }
        [JsonProperty("isCuffed")]
        public bool IsCuffed { get; set; }
        [JsonProperty("ban")]
        public BanRecord Ban { get; set; }
    }

    public class BanRecord
    {
        public string playerName;
        public List<string> identifiers;
        public DateTime bannedUntil;
        public string banReason;
        public string bannedBy;
        public Guid uuid;

        public BanRecord(string playerName, List<string> identifiers, DateTime bannedUntil, string banReason, string bannedBy, Guid uuid)
        {
            this.playerName = playerName;
            this.identifiers = identifiers;
            this.bannedUntil = bannedUntil;
            this.banReason = banReason;
            string uuidSuffix = $"\nYour ban id: {uuid}";
            if (!this.banReason.Contains(uuidSuffix) && uuid != Guid.Empty)
            {
                this.banReason += uuidSuffix;
            }
            this.bannedBy = bannedBy;
            this.uuid = uuid;
        }
    }

    public class PlayerInstance
    {
        public string License { get; set; }
        public int Id { get; set; }
        public string State { get; set; }
        public string Discord { get; set; }
        public string Skin { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Rank { get; set; }
        public string Job { get; set; }
        public string Cars { get; set; }
        public int Bitcoin { get; set; }
        public string Birth { get; set; }
        public List<ClothingSet> Clothes { get; set; }
        public string Organisation { get; set; }
        public string ClothesList { get; set; }
        public int Money { get; set; }
        public string Bills { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public Vector3 LastPosition { get; set; }
    }

    public class InventoryItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("item")]
        public string Item { get; set; }

        private int quantity;
        [JsonProperty("quantity")]
        public virtual int Quantity
        {
            get => quantity;
            set => quantity = value;
        }
    }
    public class PlayerGangInfo
    {
        [JsonProperty("id")]
        public int GangID { get; set; }
        [JsonProperty("rank")]
        public int GangRank { get; set; }
    }

    public class CompanyInstance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InventoryItem> Chest { get; set; }
        public string Taxes { get; set; }
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

    public class LTDItems
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string Icon { get; set; }
        public string Category { get; set; }

        public LTDItems(string name, string description, int price, string icon, string category)
        {
            Name = name;
            Description = description;
            Price = price;
            Icon = icon;
            Category = category;
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

    public class ReportClass
    {
        public int Id { get; set; }
        public PlayerInstance Player { get; set; }
        public string Text { get; set; }
        public bool IsResolved { get; set; }
    }

    public class BootInfo
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
    }

    public class VehicleInfo
    {
        public string Model { get; set; }
        public List<BootInfo> Boot { get; set; }
        public string Plate { get; set; }
        public int EngineLevel { get; set; }
        public int BrakeLevel { get; set; }
        public int ColorType { get; set; }
        public int ColorPrimary { get; set; }
        public int ColorSecondary { get; set; }
        public int Spoiler { get; set; }
        public int Bumber_F { get; set; }
        public int Bumber_R { get; set; }
        public int Skirt { get; set; }
        public int Exhaust { get; set; }
        public int Chassis { get; set; }
        public int Grill { get; set; }
        public int Bonnet { get; set; }
        public int Wing_L { get; set; }
        public int Wing_R { get; set; }
        public int Roof { get; set; }
        public int Engine { get; set; }
        public int Brakes { get; set; }
        public int Gearbox { get; set; }
        public int Horn { get; set; }
        public int Suspension { get; set; }
        public int Armour { get; set; }
        public int Nitrous { get; set; }
        public int Turbo { get; set; }
        public int Subwoofer { get; set; }
        public int TyreSmoke { get; set; }
        public int Hydraulics { get; set; }
        public int XenonLights { get; set; }
        public int Wheels { get; set; }
        public int WheelsRearOrHydraulics { get; set; }
        public int PLTHolder { get; set; }
        public int PLTVanity { get; set; }
        public int Interior1 { get; set; }
        public int Interior2 { get; set; }
        public int Interior3 { get; set; }
        public int Interior4 { get; set; }
        public int Interior5 { get; set; }
        public int Seats { get; set; }
        public int Steering { get; set; }
        public int Knob { get; set; }
        public int Plaque { get; set; }
        public int Ice { get; set; }
        public int Trunk { get; set; }
        public int Hydro { get; set; }
        public int EngineBay1 { get; set; }
        public int EngineBay2 { get; set; }
        public int EngineBay3 { get; set; }
        public int Chassis2 { get; set; }
        public int Chassis3 { get; set; }
        public int Chassis4 { get; set; }
        public int Chassis5 { get; set; }
        public int Door_L { get; set; }
        public int Door_R { get; set; }
        public int LiveryMod { get; set; }
        public int LightBar { get; set; }
    }

    public class ClothingSet
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("components")]
        public List<ClothesComponent> Components { get; set; } = new List<ClothesComponent>();
    }

    public class ClothesComponent
    {
        [JsonProperty("componentId")]
        public int ComponentId { get; private set; }

        [JsonProperty("drawable")]
        public int Drawable { get; set; }

        [JsonProperty("texture")]
        public int Texture { get; set; }

        [JsonProperty("palette")]
        public int Palette { get; set; }

        public ClothesComponent(int componentId)
        {
            ComponentId = componentId;
        }
    }

    public enum NotificationType : int
    {
        Default = 0,
        Bubble = 1,
        Mail = 2,
        FriendRequest = 3,
        Default2 = 4,
        Reply = 7,
        ReputationPoints = 8,
        Money = 9
    }

}