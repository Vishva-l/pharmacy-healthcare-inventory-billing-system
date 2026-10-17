using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Models
{
    public class CartItem
    {
        public int MedicineId { get; set; }

        public string MedicineName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
    }
}
