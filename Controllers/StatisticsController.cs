using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SparePartsWeb.Data;

namespace SparePartsWeb.Controllers
{
    [Authorize] // All logged-in users
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Statistics
        [Authorize(Roles = "Admin,Manager,Employee")] // Only these roles can view stats
        public async Task<IActionResult> Index()
        {
            // 1️⃣ Spare Parts by Vendor
            var vendorData = await _context.Vendors
                .Include(v => v.SpareParts)
                .Select(v => new
                {
                    v.Name,
                    Count = v.SpareParts.Count
                })
                .ToListAsync();

            ViewBag.VendorNames = vendorData.Select(v => v.Name).ToList();
            ViewBag.PartCounts = vendorData.Select(v => v.Count).ToList();

            // 2️⃣ Spare Parts by Category
            var categoryData = await _context.SpareParts
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.Categories = categoryData.Select(c => c.Category ?? "Uncategorized").ToList();
            ViewBag.CategoryCounts = categoryData.Select(c => c.Count).ToList();

            // 3️⃣ Low Stock Parts (<5)
            var lowStock = await _context.SpareParts
                .Where(p => p.Quantity < 5)
                .Select(p => new { p.Name, p.Quantity })
                .ToListAsync();

            ViewBag.LowStockNames = lowStock.Select(p => p.Name).ToList();
            ViewBag.LowStockQty = lowStock.Select(p => p.Quantity).ToList();

            // 4️⃣ Recent Additions (Latest 5)
            var recent = await _context.SpareParts
                .OrderByDescending(p => p.Id)
                .Take(5)
                .Select(p => new { p.Name, p.Quantity })
                .ToListAsync();

            ViewBag.RecentNames = recent.Select(p => p.Name).ToList();
            ViewBag.RecentQty = recent.Select(p => p.Quantity).ToList();

            return View();
        }
    }
}
