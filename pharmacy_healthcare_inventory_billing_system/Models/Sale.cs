using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Models
{
    public class Sale
    {
        public int SaleId { get; set; }

        public DateTime SaleDate { get; set; }

        public string CustomerName { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<SaleItem> Items { get; set; }
            = new List<SaleItem>();

        public Payment Payment { get; set; }
    }
}
