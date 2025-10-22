using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using SparePartsWeb.Models;

namespace SparePartsWeb.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ✅ Define all roles you want
            string[] roleNames = { "Admin", "Manager", "Employee", "Vendor" };

            // ✅ Create each role if it doesn’t exist
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (roleResult.Succeeded)
                        Console.WriteLine($"✅ Created role: {roleName}");
                    else
                        Console.WriteLine($"⚠️ Failed to create role: {roleName}");
                }
            }

            // ✅ Create default Admin user
            string adminEmail = "admin@spareparts.com";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"👑 Admin user created: {adminEmail}");
                }
                else
                {
                    Console.WriteLine("⚠️ Failed to create admin user.");
                    foreach (var error in createAdmin.Errors)
                        Console.WriteLine($"   • {error.Description}");
                }
            }
            else
            {
                Console.WriteLine($"ℹ️ Admin user already exists: {adminEmail}");
            }
        }
    }
}
