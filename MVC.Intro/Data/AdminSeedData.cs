using Microsoft.AspNetCore.Identity;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    /// <summary>
    /// Seeds the Admin role and initial administrator account.
    /// Reads credentials from configuration (User Secrets or environment variables):
    ///   AdminSettings:Email
    ///   AdminSettings:InitialPassword
    ///
    /// If not configured, the account is NOT created and instructions are logged.
    /// Safe to call on every startup — idempotent.
    /// </summary>
    public static class AdminSeedData
    {
        public const string AdminRole = "Admin";

        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            // 1. Ensure the Admin role exists
            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }

            // 2. Read admin credentials from secure configuration
            var adminEmail = configuration["AdminSettings:Email"];
            var adminPassword = configuration["AdminSettings:InitialPassword"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                // Credentials not configured — do NOT create a default insecure account.
                // Log instructions on how to configure them.
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║  ADMIN ACCOUNT NOT CONFIGURED                               ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  To create the admin account, run from project directory:   ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  dotnet user-secrets set \"AdminSettings:Email\"              ║");
                Console.WriteLine("║      \"your@email.com\"                                       ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  dotnet user-secrets set \"AdminSettings:InitialPassword\"    ║");
                Console.WriteLine("║      \"YourStr0ngP@ssword!\"                                  ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  Then restart the application.                              ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                return;
            }

            // 3. Create admin account if it doesn't exist
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Gerchev Fragrance"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                    Console.WriteLine($"[AdminSeedData] Admin account created: {adminEmail}");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"[AdminSeedData] ERROR creating admin account: {errors}");
                }
            }
            else
            {
                // Ensure existing account is in Admin role
                if (!await userManager.IsInRoleAsync(existingAdmin, AdminRole))
                {
                    await userManager.AddToRoleAsync(existingAdmin, AdminRole);
                    Console.WriteLine($"[AdminSeedData] Admin role assigned to existing account: {adminEmail}");
                }
            }
        }
    }
}
