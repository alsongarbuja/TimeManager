using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using TimeManager.Backend.Common;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.Services;

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
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<Role>>();
                var employeeTypeManager = services.GetRequiredService<IEmployeeTypeService>();
                var payFrequencyManager = services.GetRequiredService<IPayFrequencyService>();

                var configuration = services.GetRequiredService<IConfiguration>();
            
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                await SeedIdentityAsync(userManager, roleManager, configuration);

                await SeedOrganizationAsync(employeeTypeManager, payFrequencyManager);

                await SeedHrmsLookupDataAsync();
            } catch (Exception ex)
            {
                //var logger = services.GetRequiredService<ILogger>();
                //logger.LogInformation(ex, "An error occured during database seeding");
                Console.WriteLine("An error occured during database seeding", ex);
                throw;
            }
        }

        private static async Task SeedIdentityAsync(UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration configuration) {
            var roles = new[] {
                new Role { Name = AppConstants.SUPER_ADMIN_ROLE, Description = "Super user with all access" },
                new Role { Name = AppConstants.ADMIN_ROLE, Description = "Admin user with all access in a department" },
                new Role { Name = AppConstants.MANAGER_ROLE, Description = "Manager with department based managing tools" }, 
                new Role { Name = AppConstants.EMPLOYEE_ROLE, Description = "Full time employee" }, 
                new Role { Name = AppConstants.STUDENT_ROLE, Description = "Student employee" }, 
                new Role { Name = AppConstants.LEAD_ROLE, Description = "Full time employee with few extra settings" }, 
                new Role { Name = AppConstants.TEMP_EMPLOYEE_ROLE, Description = "Temporary worker" }, 
            };
            
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name!))
                {
                    await roleManager.CreateAsync(role);
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
                    var newSuperAdmin = new User
                    {
                        UserName = "superadmin",
                        Email = defaultSuperAdmin,
                        EmailConfirmed = true,
                    };

                    var createResult = await userManager.CreateAsync(newSuperAdmin, defaultSuperAdminPassword);

                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newSuperAdmin, AppConstants.SUPER_ADMIN_ROLE);
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

        private static async Task SeedOrganizationAsync(IEmployeeTypeService employeeTypeService, IPayFrequencyService payFrequencyService)
        {
            var empTypes = new[]
            {
                new EmployeeTypeDto { Name = "Full Time", Description = "Full time worker" },
                new EmployeeTypeDto { Name = "Part Time", Description = "Part time worker" },
            };

            foreach (var eT in empTypes)
            {
                var et = await employeeTypeService.GetEmployeeTypeByNameAsync(eT.Name);
                if (et == null)
                {
                    await employeeTypeService.CreateEmployeeTypeAsync(eT);
                }
            }

            var pfs = new[]
            {
                new PayFrequencyDto { Name = "Bi-weekly", Description = "Pay in bi-weekly time period" },
                new PayFrequencyDto { Name = "Monthly", Description = "Pay in monthly time period" },
            };

            foreach(var pf in pfs)
            {
                var p = await payFrequencyService.GetPayFrequencyByNameAsync(pf.Name);
                if (p == null)
                {
                    await payFrequencyService.CreatePayFrequencyAsync(pf);
                }
            } 
        }

        private static async Task SeedHrmsLookupDataAsync()
        {
            await Task.CompletedTask;
        }
    }
}
