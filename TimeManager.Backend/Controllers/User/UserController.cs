using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers.User
{
    [Authorize(Roles = "SuperAdmin")]
    public class UserController(
        UserManager<U> userManager, 
        HrmsDbContext context,
        IUserService userService, 
        IRoleService roleService, 
        IConfiguration configuration,
        IExcelService excelService,
        ILogger<U> logger
        ) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] PaginationQuery filter)
        {
            PagedResponse<UserViewModel> users = await userService.GetUsersAsync(filter);
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new RegisterViewModel
            {
                AvailableRoles = (await roleService.GetRoleOptionsAsync())
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel rvm)
        {
            if (!ModelState.IsValid)
            {
                return View(new RegisterViewModel {
                    Email = rvm.Email,
                    Role = rvm.Role,
                    Password = rvm.Password,
                    ConfirmPassword = rvm.ConfirmPassword,
                    AvailableRoles = (await roleService.GetRoleOptionsAsync(rvm.Role))
                });
            }

            var user = new U { UserName = rvm.Email.Split("@")[0], Email = rvm.Email, EmailConfirmed = true };
            var defaultPassword = configuration["Auth:DefaultPassword"] ?? throw new InvalidOperationException("Default password is not configured in the env");
            var toUserPassword = rvm.Password ?? defaultPassword;
            var result = await userManager.CreateAsync(user, toUserPassword);

            if (result.Succeeded)
            {
                try
                {
                    var role = await roleService.GetRoleByIdAsync(rvm.Role) ?? throw new KeyNotFoundException("Role not found for the given Id");
                    await userManager.AddToRoleAsync(user, role.Name!);
                    return RedirectToAction(nameof(Index));
                } catch (KeyNotFoundException ex)
                {
                    TempData["error"] = ex.Message;
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return View(new RegisterViewModel
            {
                Email = rvm.Email,
                Role = rvm.Role,
                Password = rvm.Password,
                ConfirmPassword = rvm.ConfirmPassword,
                AvailableRoles = (await roleService.GetRoleOptionsAsync(rvm.Role))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(IFormFile excelFile)
        {
            (List<Dictionary<string, string>> d, string? error) = excelService.ParseExcelFileToList(excelFile, ["Email", "Role", "UserName"]);

            if (!string.IsNullOrEmpty(error))
            {
                TempData["error"] = error;
                return RedirectToAction(nameof(Index));
            }

            var validRows = d.Where(item =>
                    !string.IsNullOrEmpty(item["Email"]) &&
                    !string.IsNullOrEmpty(item["Role"]) &&
                    !string.IsNullOrEmpty(item["UserName"]) 
                ).ToList();

            var rolesCache = (await roleService.GetRolesAsync())
                .ToDictionary(r => r.Id.ToString(), r => r.Name, StringComparer.OrdinalIgnoreCase);

            var existingEmails = userManager.Users
                .Select(u => u.Email)
                .Where(email => email != null)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var defaultPassword = configuration["Auth:DefaultPassword"] ?? throw new InvalidOperationException("Default password is not configured in the env");
            var defaultPasswordHash = userManager.PasswordHasher.HashPassword(null!, defaultPassword);

            var strategy = context.Database.CreateExecutionStrategy();
            int addedCount = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    const int chunkSize = 100;
                    for (int i = 0; i < validRows.Count; i += chunkSize)
                    {
                        var chunk = validRows.OrderBy(e => e["Role"]).Skip(i).Take(chunkSize);

                        foreach (var item in chunk)
                        {
                            if (existingEmails.Contains(item["Email"]))
                            {
                                continue;
                            }

                            var user = new U
                            {
                                UserName = item["UserName"],
                                Email = item["Email"],
                                EmailConfirmed = true,
                                PasswordHash = defaultPasswordHash
                            };

                            var result = await userManager.CreateAsync(user);
                            if (result.Succeeded)
                            {
                                if (rolesCache.TryGetValue(item["Role"], out var roleName))
                                {
                                    await userManager.AddToRoleAsync(user, roleName);
                                }
                            }
                            addedCount++;
                        }
                    }

                    await transaction.CommitAsync();
                    TempData["success"] = $"Successfully imported {addedCount} users";
                } catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogDebug(ex.Message);
                    TempData["error"] = "An error occured during bulk import. No changes saved";
                }
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (user, role) = await userService.GetUserByIdAsync(id);

            if (user == null || role == null) {
                throw new KeyNotFoundException("User or Role is not found");
            }

            var model = new RegisterViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                Role = role.Id,
                AvailableRoles = (await roleService.GetRoleOptionsAsync(role.Id))
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RegisterViewModel rvm)
        {
            var u = await userService.UpdateUserAsync(id, rvm);
            if (u == null) return View(rvm);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await userService.DeleteUserByIdAsync(id);
            TempData["success"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
