using Microsoft.AspNetCore.Http;
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
    public class CheckoutController : Controller
    {
        private readonly PharmacyDbContext _context;
        private const string CartKey = "PHARMACY_CART";

        public CheckoutController(PharmacyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View(new CheckoutViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var validMethods = new[] { "Cash", "UPI", "Card", "Online" };

            if (!validMethods.Contains(model.PaymentMethod))
                ModelState.AddModelError(nameof(model.PaymentMethod), "Select a valid payment method.");

            if (!ModelState.IsValid)
                return View(model);

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var medicineIds = cart.Select(x => x.MedicineId).Distinct().ToList();

                var medicines = await _context.Medicines
                    .Where(x => medicineIds.Contains(x.MedicineId))
                    .ToDictionaryAsync(x => x.MedicineId);

                decimal total = 0;

                foreach (var cartItem in cart)
                {
                    if (!medicines.TryGetValue(cartItem.MedicineId, out var medicine))
                        throw new InvalidOperationException("One of the medicines no longer exists.");

                    if (cartItem.Quantity <= 0)
                        throw new InvalidOperationException("Invalid medicine quantity.");

                    if (medicine.ExpiryDate < DateTime.Today)
                        throw new InvalidOperationException(medicine.Name + " has expired.");

                    if (medicine.StockQuantity < cartItem.Quantity)
                        throw new InvalidOperationException(
                            "Insufficient stock for " + medicine.Name + ".");

                    total += medicine.SellingPrice * cartItem.Quantity;
                }

                var sale = new Sale
                {
                    CustomerName = model.CustomerName.Trim(),
                    SaleDate = DateTime.Now,
                    TotalAmount = total
                };

                foreach (var cartItem in cart)
                {
                    var medicine = medicines[cartItem.MedicineId];

                    sale.Items.Add(new SaleItem
                    {
                        MedicineId = medicine.MedicineId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = medicine.SellingPrice
                    });

                    medicine.StockQuantity -= cartItem.Quantity;
                }

                sale.Payment = new Payment
                {
                    PaymentMethod = model.PaymentMethod,
                    Amount = total,
                    PaymentDate = DateTime.Now,
                    TransactionId = model.PaymentMethod == "Cash"
                        ? null
                        : "TXN-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove(CartKey);

                return RedirectToAction(nameof(Success), new { id = sale.SaleId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        public async Task<IActionResult> Success(int id)
        {
            var sale = await _context.Sales
                .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
                .Include(x => x.Payment)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SaleId == id);

            return sale == null ? NotFound() : View(sale);
        }

        private List<CartItem> GetCart()
        {
            var data = HttpContext.Session.GetString(CartKey);
            if (string.IsNullOrEmpty(data))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(data)
                   ?? new List<CartItem>();
        }
    }
}
