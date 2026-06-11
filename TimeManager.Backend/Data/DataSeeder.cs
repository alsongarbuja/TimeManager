using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TimeManager.Backend.Data
{
    public class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<HrmsDbContext>();
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                var configuration = services.GetRequiredService<IConfiguration>();
            
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                await SeedIdentityAsync(userManager, roleManager, configuration);

                await SeedHrmsLookupDataAsync(context);
            } catch (Exception ex)
            {
                //var logger = services.GetRequiredService<ILogger>();
                //logger.LogInformation(ex, "An error occured during database seeding");
                Console.WriteLine("An error occured during database seeding", ex);
                throw;
            }
        }

        private static async Task SeedIdentityAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration) {
            string[] roles = { "SuperAdmin", "Admin", "Manager", "Employee", "Student Employee", "Lead", "Temp Employee" };
            
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var defaultSuperAdmin = configuration["SeedSettings:SuperAdminEmail"] ?? "superadmin@hrms.com";
            var defaultSuperAdminPassword = configuration["SeedSettings:SuperAdminPassword"];

            if (string.IsNullOrEmpty(defaultSuperAdminPassword))
            {
                throw new InvalidOperationException("Seeding failed: 'SeedSettings:SuperAdminPassword' environment variable is missing.");
            }

            var superAdminUser = await userManager.FindByEmailAsync(defaultSuperAdmin);

            if (superAdminUser == null)
            {
                try
                {
                    var newSuperAdmin = new IdentityUser
                    {
                        UserName = "superadmin",
                        Email = defaultSuperAdmin,
                        EmailConfirmed = true,
                    };

                    var createResult = await userManager.CreateAsync(newSuperAdmin, defaultSuperAdminPassword);

                    if (createResult.Succeeded)
                    {
                        Console.WriteLine("created new");

                        await userManager.AddToRoleAsync(newSuperAdmin, "SuperAdmin");
                    } else
                    {
                        throw new InvalidDataException(createResult.Errors.ToList()[0].Description);
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static async Task SeedHrmsLookupDataAsync(HrmsDbContext context)
        {
            await Task.CompletedTask;
        }
    }
}
