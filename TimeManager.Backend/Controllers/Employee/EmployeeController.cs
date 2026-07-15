using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Requests;
using E = TimeManager.Backend.Models.Employee_Management.Employee;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Employee
{
    [Authorize(Policy = "AdminPolicy")]
    public class EmployeeController(
        HrmsDbContext context,
        ILogger<E> logger,
        IEmployeeService employeeService, 
        IDepartmentService departmentService,
        IUserService userService,
        IExcelService excelService
        ) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] PaginationQuery filter)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            PagedResponse<EmployeeViewModel> employees = await employeeService.GetEmployeesAsync(departmentId, filter);
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            EmployeeData employeeData = new() { 
                EmployeeView = new EmployeeViewModel(),
                Departments = (await departmentService.GetDepartmentOptionsAsync()),
                Users = (await userService.GetUserOptionsAsync())
            };
            return View(employeeData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeData employeeData)
        {
            try
            {
                int id = await employeeService.CreateEmployeeAsync(new EmployeeDto
                {
                    Email = employeeData.EmployeeView.Email,
                    UniqueId = employeeData.EmployeeView.UniqueId,
                    FirstName = employeeData.EmployeeView.FirstName,
                    LastName = employeeData.EmployeeView.LastName,
                    UserId = employeeData.UserId,
                    DepartmentId = employeeData.DepartmentId,
                });
                TempData["success"] = "Employee Data created successfully";
                return View(new EmployeeData
                {
                    EmployeeView = new EmployeeViewModel(),
                    Departments = (await departmentService.GetDepartmentOptionsAsync()),
                    Users = (await userService.GetUserOptionsAsync())
                });
            } catch (ArgumentException ex)
            {
                ModelState.AddModelError("EmployeeView.UniqueId", ex.Message);

                return View(new EmployeeData
                {
                    EmployeeView = employeeData.EmployeeView,
                    Departments = (await departmentService.GetDepartmentOptionsAsync(employeeData.UserId)),
                    Users = (await userService.GetUserOptionsAsync(employeeData.UserId))
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(IFormFile excelFile)
        {
            (List<Dictionary<string, string>> d, string? error) = excelService.ParseExcelFileToList(excelFile, ["Email", "First", "Last", "Unique ID"]);

            if (!string.IsNullOrEmpty(error))
            {
                TempData["error"] = error;
                return RedirectToAction(nameof(Index));
            }

            var validRows = d.Where(item =>
                    !string.IsNullOrEmpty(item["Unique ID"]) &&
                    !string.IsNullOrEmpty(item["Email"]) &&
                    !string.IsNullOrEmpty(item["First"]) &&
                    !string.IsNullOrEmpty(item["Last"])
                ).ToList();

            if (validRows.Count == 0)
            {
                TempData["info"] = "No valid records found to import.";
                return RedirectToAction(nameof(Index));
            }

            var existingSOIds = await context.Employee.Select(e => e.UniqueId).ToHashSetAsync(StringComparer.OrdinalIgnoreCase);
            var targetEmails = validRows
                .Select(item => item["Email"])
                .Distinct()
                .ToList();
            var userMaps = await context.Users
                .Where(u => targetEmails.Contains(u.Email))
                .Select(u => new { u.Email, u.Id })
                .ToDictionaryAsync(u => u.Email, u => u.Id, StringComparer.OrdinalIgnoreCase);

            var strategy = context.Database.CreateExecutionStrategy();
            int addedCount = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var employeesToCreate = new List<E>();

                    foreach (var item in validRows)
                    {
                        if (existingSOIds.Contains(item["Unique ID"]))
                        {
                            continue;
                        }

                        userMaps.TryGetValue(item["Email"], out var userId);

                        var emp = new E
                        {
                            FirstName = item["First"],
                            LastName = item["Last"],
                            UniqueId = item["Unique ID"],
                            UserId = userId
                        };
                        employeesToCreate.Add(emp);
                        addedCount++;
                    }

                    if (employeesToCreate.Count > 0)
                    {
                        await context.Employee.AddRangeAsync(employeesToCreate);
                        await context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    TempData["success"] = $"Successfully imported {addedCount} employee";
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
            var e = await employeeService.GetEmployeeByIdAsync(id);
            if (e == null) return NotFound();
            int? departmentId = HttpContext.Session.GetDepartmentId();
            return View(new EmployeeData
            {
                EmployeeView = new EmployeeViewModel
                {
                    Id = e.Id,
                    UniqueId = e.UniqueId,
                    FirstName = e.FirstName, 
                    LastName = e.LastName,
                },
                Users = (await userService.GetUserOptionsAsync(e.UserId)),
                Departments = (await departmentService.GetDepartmentOptionsAsync(departmentId ?? 0))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeData employeeData)
        {
            var e = await employeeService.UpdateEmployeeAsync(id, new EmployeeDto
            {
                Email = employeeData.EmployeeView.Email,
                UniqueId = employeeData.EmployeeView.UniqueId,
                FirstName = employeeData.EmployeeView.FirstName,
                LastName = employeeData.EmployeeView.LastName,
                UserId = employeeData.UserId,
                DepartmentId = employeeData.DepartmentId,
            });
            if (e == null)
            {
                TempData["error"] = "Employee not found";
                return View(employeeData);
            }

            //await _employeeDepartmentService.UpdateEmployeeDepartmentAsync(e.Id, )

            TempData["success"] = "Employee updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await employeeService.DeleteEmployeeByIdAsync(id);
                TempData["success"] = "Employee deleted";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

    }    
}
