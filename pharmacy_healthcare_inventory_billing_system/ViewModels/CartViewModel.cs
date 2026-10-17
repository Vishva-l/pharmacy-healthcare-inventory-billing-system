using pharmacy_healthcare_inventory_billing_system.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal GrandTotal
        {
            get
            {
                return Items.Sum(x => x.Total);
            }
        }
    }
}
