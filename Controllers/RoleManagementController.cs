using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SparePartsWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparePartsWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // List all users with their roles
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return View(userRolesViewModel);
        }

        // Show role management form for a user
        public async Task<IActionResult> Manage(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.Select(r => new RoleSelection
                {
                    RoleName = r.Name,
                    Selected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        // Handle form submission from Manage.cshtml
        [HttpPost]
        public async Task<IActionResult> Update(ManageUserRolesViewModel model)
        {
            if (model == null)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.RoleName).ToList();

            // Add new roles
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add roles.");
                return View("Manage", model);
            }

            // Remove unchecked roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove roles.");
                return View("Manage", model);
            }

            return RedirectToAction("Index");
        }
    }
}
