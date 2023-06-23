using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Client
{
    public enum VehicleImport
    {
        [Display("a1gt63", "Mercedes AMG GT 63", 3000)]
        MercedesAMGGT63,

        [Display("cls63amg", "Mercedes CLS 350 AMG", 2500)]
        CLS63AMG,

        [Display("rs7", "Audi RS7", 2000)]
        AudiRS7,
        
        [Display("m5", "BMW M5 Competition", 3000)]
        BMWM5
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
