using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace PharmacyStock.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { "Admin", "Yetkili" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user if it doesn't exist
            var adminEmail = "admin@pharmacy.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var createPowerUser = await userManager.CreateAsync(adminUser, "Admin123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create yetkili user if it doesn't exist
            var yetkiliEmail = "yetkili@pharmacy.com";
            var yetkiliUser = await userManager.FindByEmailAsync(yetkiliEmail);
            if (yetkiliUser == null)
            {
                yetkiliUser = new IdentityUser
                {
                    UserName = "yetkili",
                    Email = yetkiliEmail,
                    EmailConfirmed = true
                };
                var createYetkiliUser = await userManager.CreateAsync(yetkiliUser, "Yetkili123!");
                if (createYetkiliUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(yetkiliUser, "Yetkili");
                }
            }
        }
    }
}
