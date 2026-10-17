using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacy_healthcare_inventory_billing_system.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pharmacy_healthcare_inventory_billing_system.ViewModels;

namespace pharmacy_healthcare_inventory_billing_system.Controllers
{
    public class ReportsController : Controller
    {
        private readonly PharmacyDbContext _context;

        public ReportsController(
            PharmacyDbContext context)
        {
            _context = context;
        }

        // 30-day expiry report
        public async Task<IActionResult> Expiry()
        {
            var today = DateTime.Today;

            var expiryLimit =
                today.AddDays(30);

            var medicines =
                await _context.Medicines
                .Where(x =>
                    x.ExpiryDate >= today &&
                    x.ExpiryDate <= expiryLimit)
                .OrderBy(x => x.ExpiryDate)
                .ToListAsync();

            return View(medicines);
        }

        // Daily cash vs digital
        public async Task<IActionResult>
            DailySales(DateTime? date)
        {
            var selectedDate =
                date?.Date ?? DateTime.Today;

            var nextDate =
                selectedDate.AddDays(1);

            var payments =
                await _context.Payments
                .Include(x => x.Sale)
                .Where(x =>
                    x.PaymentDate >=
                        selectedDate &&
                    x.PaymentDate <
                        nextDate)
                .ToListAsync();

            ViewBag.SelectedDate =
                selectedDate;

            ViewBag.Cash =
                payments
                .Where(x =>
                    x.PaymentMethod
                    == "Cash")
                .Sum(x => x.Amount);

            ViewBag.Digital =
                payments
                .Where(x =>
                    x.PaymentMethod
                    != "Cash")
                .Sum(x => x.Amount);

            ViewBag.Total =
                payments.Sum(x => x.Amount);

            return View(payments);
        }

        // Top medicines
        public async Task<IActionResult> TopMedicines()
        {
            var result =
                await _context.SaleItems
                .Include(x => x.Medicine)
                .GroupBy(x => new
                {
                    x.MedicineId,
                    x.Medicine.Name
                })
                .Select(g => new TopMedicineViewModel
                {
                    MedicineName = g.Key.Name,

                    QuantitySold =
                        g.Sum(x => x.Quantity),

                    Revenue =
                        g.Sum(x =>
                            x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(
                    x => x.QuantitySold)
                .Take(10)
                .ToListAsync();

            return View(result);
        }
    }
}
