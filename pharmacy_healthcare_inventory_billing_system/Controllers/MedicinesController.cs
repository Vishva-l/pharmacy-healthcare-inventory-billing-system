using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacy_healthcare_inventory_billing_system.Data;
using pharmacy_healthcare_inventory_billing_system.Models;

namespace pharmacy_healthcare_inventory_billing_system.Controllers
{
    public class MedicinesController : Controller
    {
        private readonly PharmacyDbContext _context;

        public MedicinesController(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Medicines.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    (x.GenericName != null && x.GenericName.Contains(search)) ||
                    (x.Manufacturer != null && x.Manufacturer.Contains(search)) ||
                    (x.BatchNumber != null && x.BatchNumber.Contains(search)));
            }

            ViewBag.Search = search;
            return View(await query.OrderBy(x => x.Name).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var medicine = await _context.Medicines
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MedicineId == id);

            return medicine == null ? NotFound() : View(medicine);
        }

        [HttpGet]
        public IActionResult Create() => View(new Medicine
        {
            ExpiryDate = DateTime.Today.AddYears(1),
            CreatedAt = DateTime.Now
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medicine medicine)
        {
            medicine.CreatedAt = DateTime.Now;

            if (medicine.SellingPrice < medicine.PurchasePrice)
                ModelState.AddModelError(nameof(medicine.SellingPrice),
                    "Selling price should not be less than purchase price.");

            if (medicine.ExpiryDate <= DateTime.Today)
                ModelState.AddModelError(nameof(medicine.ExpiryDate),
                    "Expiry date must be a future date.");

            if (!ModelState.IsValid)
                return View(medicine);

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Medicine saved successfully in the database.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var medicine = await _context.Medicines.FindAsync(id);
            return medicine == null ? NotFound() : View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Medicine medicine)
        {
            if (id != medicine.MedicineId) return NotFound();

            if (medicine.SellingPrice < medicine.PurchasePrice)
                ModelState.AddModelError(nameof(medicine.SellingPrice),
                    "Selling price should not be less than purchase price.");

            if (!ModelState.IsValid) return View(medicine);

            var existing = await _context.Medicines.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = medicine.Name;
            existing.GenericName = medicine.GenericName;
            existing.Manufacturer = medicine.Manufacturer;
            existing.BatchNumber = medicine.BatchNumber;
            existing.ExpiryDate = medicine.ExpiryDate;
            existing.StockQuantity = medicine.StockQuantity;
            existing.ReorderLevel = medicine.ReorderLevel;
            existing.PurchasePrice = medicine.PurchasePrice;
            existing.SellingPrice = medicine.SellingPrice;
            existing.Category = medicine.Category;
            existing.RequiresPrescription = medicine.RequiresPrescription;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Medicine updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(x => x.MedicineId == id);
            return medicine == null ? NotFound() : View(medicine);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicine = await _context.Medicines
                .Include(x => x.SaleItems)
                .FirstOrDefaultAsync(x => x.MedicineId == id);

            if (medicine == null) return NotFound();

            var usedInPrescription = await _context.PrescriptionItems
                .AnyAsync(x => x.MedicineId == id);

            if (medicine.SaleItems.Any() || usedInPrescription)
            {
                TempData["Error"] = "This medicine is already used in a sale/prescription and cannot be deleted. Edit its stock instead.";
                return RedirectToAction(nameof(Index));
            }

            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Medicine deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
