using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparePartsWeb.Data;
using Microsoft.AspNetCore.Authorization;

namespace SparePartsWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.EquipmentsCount = await _context.Equipments.CountAsync();
            ViewBag.VendorsCount = await _context.Vendors.CountAsync();
            ViewBag.SparePartsCount = await _context.SpareParts.CountAsync();

            return View();
        }
    }
}
