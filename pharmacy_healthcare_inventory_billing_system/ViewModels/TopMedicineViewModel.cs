using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.ViewModels
{
    public class TopMedicineViewModel
    {
        public string MedicineName { get; set; }

        public int QuantitySold { get; set; }

        public decimal Revenue { get; set; }
    }
}
