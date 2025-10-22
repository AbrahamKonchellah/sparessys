using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparePartsWeb.Data;
using SparePartsWeb.Models;

namespace SparePartsWeb.Controllers
{
    [Authorize] // All actions require login
    public class SparePartsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SparePartsController> _logger;

        public SparePartsController(AppDbContext context, ILogger<SparePartsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /SpareParts
        public async Task<IActionResult> Index()
        {
            var spareParts = await _context.SpareParts
                                           .Include(s => s.Vendor)
                                           .OrderByDescending(s => s.Id)
                                           .ToListAsync();
            return View(spareParts);
        }

        // GET: /SpareParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sparePart = await _context.SpareParts
                                          .Include(s => s.Vendor)
                                          .FirstOrDefaultAsync(s => s.Id == id);

            if (sparePart == null) return NotFound();

            return View(sparePart);
        }

        // GET: /SpareParts/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewBag.VendorList = new SelectList(_context.Vendors, "Id", "Name");
            return View();
        }

        // POST: /SpareParts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(SparePart sparePart)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VendorList = new SelectList(_context.Vendors, "Id", "Name", sparePart.VendorId);
                TempData["Error"] = "Please fill all required fields correctly.";
                return View(sparePart);
            }

            try
            {
                _context.Add(sparePart);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Spare part '{sparePart.Name}' added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating spare part");
                TempData["Error"] = "An unexpected error occurred while saving. Try again.";
                ViewBag.VendorList = new SelectList(_context.Vendors, "Id", "Name", sparePart.VendorId);
                return View(sparePart);
            }
        }

        // GET: /SpareParts/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sparePart = await _context.SpareParts.FindAsync(id);
            if (sparePart == null) return NotFound();

            ViewBag.VendorList = new SelectList(_context.Vendors, "Id", "Name", sparePart.VendorId);
            return View(sparePart);
        }

        // POST: /SpareParts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, SparePart sparePart)
        {
            if (id != sparePart.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.VendorList = new SelectList(_context.Vendors, "Id", "Name", sparePart.VendorId);
                TempData["Error"] = "Please correct the errors and try again.";
                return View(sparePart);
            }

            try
            {
                _context.Update(sparePart);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"✅ Spare part '{sparePart.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SpareParts.Any(e => e.Id == sparePart.Id))
                    return NotFound();

                throw;
            }
        }

        // GET: /SpareParts/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sparePart = await _context.SpareParts
                                          .Include(s => s.Vendor)
                                          .FirstOrDefaultAsync(s => s.Id == id);

            if (sparePart == null) return NotFound();
            return View(sparePart);
        }

        // POST: /SpareParts/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sparePart = await _context.SpareParts.FindAsync(id);
            if (sparePart != null)
            {
                _context.SpareParts.Remove(sparePart);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"🗑️ Spare part '{sparePart.Name}' deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
