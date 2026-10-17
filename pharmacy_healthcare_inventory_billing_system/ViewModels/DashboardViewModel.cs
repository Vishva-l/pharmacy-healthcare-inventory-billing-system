using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalMedicines { get; set; }

        public int LowStockMedicines { get; set; }

        public int ExpiringMedicines { get; set; }

        public int TodayOrders { get; set; }

        public decimal TodaySales { get; set; }

        public decimal TodayCash { get; set; }

        public decimal TodayDigital { get; set; }
    }
}
