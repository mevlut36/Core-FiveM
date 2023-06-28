using System;

namespace Core.Client
{
    public enum VehicleImport
    {
        [Display("a1gt63", "Mercedes AMG GT 63", 3000)]
        MercedesAMGGT63,

        [Display("cls63amg", "Mercedes CLS 350 AMG", 2500)]
        CLS63AMG,

        [Display("c6320", "Mercedes C 63 AMG S 2020", 2500)]
        C63S,

        [Display("rs7", "Audi RS7", 2000)]
        AudiRS7,

        [Display("q82023", "Audi Q8 2023", 2500)]
        AudiQ82023,

        [Display("m5", "BMW M5 Competition", 3000)]
        BMWM5,

        [Display("nismo20", "Nissan GTR Nismo 2020", 2600)]
        Nismo20,

        [Display("pandema90", "Nissan Supra Pandem A90", 2500)]
        Pandema90,

        [Display("Corvettezr1", "Chevrolet Corvette ZR1 2019", 3000)]
        CorvetteZR1,

        [Display("teslapd", "Tesla Model PD", 2000)]
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
