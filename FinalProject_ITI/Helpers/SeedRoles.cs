using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Identity;

namespace FinalProject_ITI.Helpers;
public static class SeedRoles
{
    public static async Task SeedRolesAndAdminAsync(IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roles = { "Customer", "ADMIN", "DeliveryBoy", "BrandOwner" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            string adminEmail = "admin@admin.com";
            string adminPassword = "Admin@123";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    AccountType = "ADMIN",
                    FirstName = "System",
                    LastName = "Admin",
                    PhoneNumber = "0000000000"
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "ADMIN");
                }
            }
        }
    }
}