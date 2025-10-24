using System;

namespace Core.Client
{
    public enum VehicleImport
    {
        [Display("ae86", "Ayoto AE86 Trueno [~d~Initial ~r~D~s~ Edition]", 2000)]
        TOYOTAAE86TRUENO,

        [Display("rx7fd", "Masda RU-7 [~d~Initial ~r~D~s~ Edition]", 2000)]
        MAZDARX7,

        [Display("c6320", "Merzedes C 63 AMG S 2020", 2500)]
        C63S,

        [Display("rs7", "Odi RS7", 2000)]
        AudiRS7,
        [Display("r820", "Odi R8 2020", 3000)]
        AudiR82020,

        [Display("q82023", "Odi Q8 2023", 2500)]
        AudiQ82023,

        [Display("m5", "BMVV M5 Competition", 3000)]
        BMWM5,

        [Display("nismo20", "Nizzan GTR Nismo 2020", 2600)]
        Nismo20,

        [Display("pandema90", "Nizzan Supra Pandem A90", 2500)]
        Pandema90,

        [Display("Corvettezr1", "Chevrolette Corvet ZR1 2019", 3000)]
        CorvetteZR1,

        [Display("e-tron", "Odi E-Tron", 2500)]
        ETRON,

        [Display("teslapd", "Tezla", 2000)]
        TeslaPD
    }

    public class DisplayAttribute : Attribute
    {
        public string VehicleName { get; }
        public string Name { get; }
        public int Price { get; }

        public DisplayAttribute(string vehicleName, string name, int price)
        {
            VehicleName = vehicleName;
            Name = name;
            Price = price;
        }
    }


}
