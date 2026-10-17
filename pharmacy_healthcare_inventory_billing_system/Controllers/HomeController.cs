using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pharmacy_healthcare_inventory_billing_system.Data;
using pharmacy_healthcare_inventory_billing_system.Models;
using pharmacy_healthcare_inventory_billing_system.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly PharmacyDbContext _context;

        public HomeController(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var tomorrow = today.AddDays(1);

            var expiryLimit = today.AddDays(30);

            var sales = await _context.Sales
                .Where(x =>
                    x.SaleDate >= today &&
                    x.SaleDate < tomorrow)
                .ToListAsync();

            var payments = await _context.Payments
                .Where(x =>
                    x.PaymentDate >= today &&
                    x.PaymentDate < tomorrow)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalMedicines =
                    await _context.Medicines.CountAsync(),

                LowStockMedicines =
                    await _context.Medicines.CountAsync(
                        x => x.StockQuantity <=
                             x.ReorderLevel),

                ExpiringMedicines =
                    await _context.Medicines.CountAsync(
                        x => x.ExpiryDate >= today &&
                             x.ExpiryDate <= expiryLimit),

                TodayOrders = sales.Count,

                TodaySales =
                    sales.Sum(x => x.TotalAmount),

                TodayCash =
                    payments
                    .Where(x =>
                        x.PaymentMethod == "Cash")
                    .Sum(x => x.Amount),

                TodayDigital =
                    payments
                    .Where(x =>
                        x.PaymentMethod != "Cash")
                    .Sum(x => x.Amount)
            };

            return View(model);
        }
    }
}
