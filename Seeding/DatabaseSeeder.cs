using E_Commers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace E_Commers.Seeding
{
    public class DatabaseSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ECommerceDbContext _context;

        public DatabaseSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ECommerceDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task EnsureAdminUserAsync()
        {
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin@123";
            var adminRole = "Admin";

            // 1. تأكد إن الرول موجودة
            if (!await _roleManager.RoleExistsAsync(adminRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // 2. هل فيه أدمن بالإيميل ده؟
            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    Role = "Admin",
                    FirstName = "Admin", // Ensure this field is set
                    LastName = "User",   // Ensure this field is set
                };

                // 3. إنشاء المستخدم
                var result = await _userManager.CreateAsync(newAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newAdmin, adminRole);
                    Console.WriteLine(" Admin user created successfully.");
                }
                else
                {
                    Console.WriteLine(" Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine(" Admin user already exists.");
            }
        }

    }
}
