using FoodProject.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Razorpay.Api;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoodProject.Data
{
    public static class DataSeeding
    {
        public static async Task SeedUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();

            // 1. Create Admin Role if not exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = "Admin"
                });
            }

            // 2. Create Admin User if not exists
            var email = "admin@gmail.com";

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = "Admin",
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user,"Admin@123456");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(", ", result.Errors.Select(x => x.Description))
                    );
                }
            }

            // 3. Assign Admin Role to User
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}