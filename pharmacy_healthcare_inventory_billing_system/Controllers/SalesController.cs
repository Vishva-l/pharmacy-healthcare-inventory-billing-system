using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharmacy_healthcare_inventory_billing_system.Data;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Controllers
{
    public class SalesController : Controller
    {
        private readonly PharmacyDbContext _context;

        public SalesController(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
                .Include(x => x.Payment)
                .OrderByDescending(x => x.SaleDate)
                .AsNoTracking()
                .ToListAsync();

            return View(sales);
        }

        public async Task<IActionResult> Details(int id)
        {
            var sale = await _context.Sales
                .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
                .Include(x => x.Payment)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SaleId == id);

            return sale == null ? NotFound() : View(sale);
        }
    }
}
