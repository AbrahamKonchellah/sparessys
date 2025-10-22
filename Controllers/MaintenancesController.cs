using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SparePartsWeb.Data;
using SparePartsWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace SparePartsWeb.Controllers
{
    [Authorize] // All actions require login
    public class MaintenancesController : Controller
    {
        private readonly AppDbContext _context;

        public MaintenancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Maintenances
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Maintenances.Include(m => m.Equipment);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Maintenances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var maintenance = await _context.Maintenances
                .Include(m => m.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenance == null)
                return NotFound();

            return View(maintenance);
        }

        // GET: Maintenances/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewData["EquipmentId"] = new SelectList(_context.Equipments, "Id", "Name");
            return View();
        }

        // POST: Maintenances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Id,EquipmentId,ScheduledDate,Description,Cost")] Maintenance maintenance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(maintenance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EquipmentId"] = new SelectList(_context.Equipments, "Id", "Name", maintenance.EquipmentId);
            return View(maintenance);
        }

        // GET: Maintenances/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
                return NotFound();

            ViewData["EquipmentId"] = new SelectList(_context.Equipments, "Id", "Name", maintenance.EquipmentId);
            return View(maintenance);
        }

        // POST: Maintenances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EquipmentId,ScheduledDate,Description,Cost")] Maintenance maintenance)
        {
            if (id != maintenance.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceExists(maintenance.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EquipmentId"] = new SelectList(_context.Equipments, "Id", "Name", maintenance.EquipmentId);
            return View(maintenance);
        }

        // GET: Maintenances/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var maintenance = await _context.Maintenances
                .Include(m => m.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenance == null)
                return NotFound();

            return View(maintenance);
        }

        // POST: Maintenances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance != null)
                _context.Maintenances.Remove(maintenance);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceExists(int id)
        {
            return _context.Maintenances.Any(e => e.Id == id);
        }
    }
}
