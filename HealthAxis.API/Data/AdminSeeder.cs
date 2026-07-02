using HealthAxis.API.Models;
using Microsoft.AspNetCore.Identity;

namespace HealthAxis.API.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            const string email = "admin@healthaxis.com";
            const string password = "Admin@123";

            // Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Check if admin already exists
            var existingAdmin = await userManager.FindByEmailAsync(email);

            if (existingAdmin != null)
            {
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}