﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacy_healthcare_inventory_billing_system.Data;
using pharmacy_healthcare_inventory_billing_system.Models;
using pharmacy_healthcare_inventory_billing_system.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Controllers
{
    public class CartController : Controller
    {
        private readonly PharmacyDbContext _context;

        private const string CartKey =
            "PHARMACY_CART";

        public CartController(
            PharmacyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();

            var model = new CartViewModel
            {
                Items = cart
            };

            return View(model);
        }

        public async Task<IActionResult> Add(
            int id,
            int quantity = 1)
        {
            var medicine =
                await _context.Medicines
                .FirstOrDefaultAsync(
                    x => x.MedicineId == id);

            if (medicine == null)
            {
                return NotFound();
            }

            if (medicine.ExpiryDate <
                DateTime.Today)
            {
                TempData["Error"] =
                    "Expired medicine cannot be sold.";

                return RedirectToAction(
                    nameof(Index));
            }

            if (medicine.StockQuantity <= 0)
            {
                TempData["Error"] =
                    "Medicine is out of stock.";

                return RedirectToAction(
                    nameof(Index));
            }

            var cart = GetCart();

            var existing =
                cart.FirstOrDefault(
                    x => x.MedicineId == id);

            if (existing == null)
            {
                cart.Add(new CartItem
                {
                    MedicineId =
                        medicine.MedicineId,

                    MedicineName =
                        medicine.Name,

                    Quantity =
                        Math.Min(
                            quantity,
                            medicine.StockQuantity),

                    UnitPrice =
                        medicine.SellingPrice
                });
            }
            else
            {
                existing.Quantity =
                    Math.Min(
                        existing.Quantity + quantity,
                        medicine.StockQuantity);
            }

            SaveCart(cart);

            return RedirectToAction(
                nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MedicineId == id);

            if (item == null)
                return RedirectToAction(nameof(Index));

            if (quantity <= 0)
            {
                cart.Remove(item);
                SaveCart(cart);
                return RedirectToAction(nameof(Index));
            }

            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity = Math.Min(quantity, medicine.StockQuantity);
                item.UnitPrice = medicine.SellingPrice;
                item.MedicineName = medicine.Name;

                if (item.Quantity <= 0)
                    cart.Remove(item);
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();

            cart.RemoveAll(
                x => x.MedicineId == id);

            SaveCart(cart);

            return RedirectToAction(
                nameof(Index));
        }

        public async Task<IActionResult>
            AddPrescription(int prescriptionId)
        {
            var prescription =
                await _context.Prescriptions
                .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
                .FirstOrDefaultAsync(
                    x => x.PrescriptionId
                         == prescriptionId);

            if (prescription == null)
            {
                return NotFound();
            }

            var cart = GetCart();

            foreach (var item
                in prescription.Items)
            {
                var medicine =
                    item.Medicine;

                if (medicine == null)
                    continue;

                if (medicine.ExpiryDate <
                    DateTime.Today)
                    continue;

                if (medicine.StockQuantity <= 0)
                    continue;

                int quantity =
                    Math.Min(
                        item.Quantity,
                        medicine.StockQuantity);

                var existing =
                    cart.FirstOrDefault(
                        x => x.MedicineId
                             == medicine.MedicineId);

                if (existing == null)
                {
                    cart.Add(new CartItem
                    {
                        MedicineId =
                            medicine.MedicineId,

                        MedicineName =
                            medicine.Name,

                        Quantity =
                            quantity,

                        UnitPrice =
                            medicine.SellingPrice
                    });
                }
                else
                {
                    existing.Quantity =
                        Math.Min(
                            existing.Quantity
                            + quantity,
                            medicine.StockQuantity);
                }
            }

            SaveCart(cart);

            return RedirectToAction(
                nameof(Index));
        }

        private List<CartItem> GetCart()
        {
            string data =
                HttpContext.Session
                .GetString(CartKey);

            if (string.IsNullOrEmpty(data))
            {
                return new List<CartItem>();
            }

            return JsonSerializer
                .Deserialize<List<CartItem>>(data)
                ?? new List<CartItem>();
        }

        private void SaveCart(
            List<CartItem> cart)
        {
            HttpContext.Session.SetString(
                CartKey,
                JsonSerializer.Serialize(cart));
        }
    }
}
