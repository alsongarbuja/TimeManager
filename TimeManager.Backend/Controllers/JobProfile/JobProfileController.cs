using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using JP = TimeManager.Backend.Models.Employee_Management.JobProfile;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace TimeManager.Backend.Controllers.JobProfile
{
    [Authorize(Policy = "AdminPolicy")]
    public class JobProfileController(
        IJobProfileService jobProfileService, 
        IEmployeeService employeeService, 
        IProfileTemplateService profileTemplateService,
        IExcelService excelService,
        HrmsDbContext context,
        ILogger<JP> logger
        ) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] PaginationFilter filter)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            PagedResponse<JobProfileViewModel> jp = await jobProfileService.GetJobProfilesAsync(departmentId, filter);
            return View(jp);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            JobProfileViewModel pvm = new()
            {
                Employees = (await employeeService.GetEmployeeOptionAsync()),
                ProfileTemplates = (await profileTemplateService.GetProfileTemplateOptionAsync())
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobProfileViewModel pvm)
        {
            await jobProfileService.CreateJobProfileAsync(pvm);
            TempData["success"] = "Job Profile successfully created";
            return View(new JobProfileViewModel
            {
                Employees = (await employeeService.GetEmployeeOptionAsync()),
                ProfileTemplates = (await profileTemplateService.GetProfileTemplateOptionAsync())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(IFormFile excelFile)
        {
            (List<Dictionary<string, string>> data, string? error) = excelService.ParseExcelFileToList(excelFile, ["Index", "Unique ID", "Role"]);

            if (!string.IsNullOrEmpty(error))
            {
                TempData["error"] = error;
                return RedirectToAction(nameof(Index));
            }

            var validRows = data.Where(item =>
                    !string.IsNullOrEmpty(item["Index"]) &&
                    !string.IsNullOrEmpty(item["Unique ID"]) &&
                    !string.IsNullOrEmpty(item["Role"]) 
                ).ToList();

            if (validRows.Count == 0)
            {
                TempData["error"] = "The excel file is empty";
                return RedirectToAction(nameof(Index));
            }

            var uniqueIds = validRows.Select(vr => vr["Unique ID"]).Distinct().ToList();
            var uniqueIndexes = validRows.Select(vr => vr["Index"]).Distinct().ToList();
            var uniqueRoles = validRows.Select(vr => vr["Role"]).Distinct().ToList();
            var employeeIdMaps = await context.Employee
                .Where(e => uniqueIds.Contains(e.UniqueId))
                .Select(e => new { e.UniqueId, e.Id })
                .ToDictionaryAsync(e => e.UniqueId, e => e.Id, StringComparer.OrdinalIgnoreCase);
            var profileTemplateIdMaps = await context.ProfileTemplate
                .Where(pt => uniqueIndexes.Contains(pt.Unit.Index.ToString()) && uniqueRoles.Contains(pt.Role.Name))
                .Select(pt => new { Index = pt.Unit.Index.ToString(), RoleName = pt.Role.Name, pt.Id })
                .ToListAsync();
            var profileTemplateMaps = profileTemplateIdMaps
                .ToDictionary(
                    pt => (Index: pt.Index.ToLowerInvariant(), RoleName: pt.RoleName.ToLowerInvariant()),
                    pt => pt.Id
                );

            var jobProfileIdMaps = await context.JobProfile.Select(jp => new
            {
                Pt = jp.ProfileTemplateId.ToString(),
                E = jp.EmployeeId,
                jp.Id,
            }).ToListAsync();
            var jpLookUpMap = jobProfileIdMaps
                .ToDictionary(
                    jp => (jp.Pt, jp.E),
                    jp => jp.Id
                );

            var strategy = context.Database.CreateExecutionStrategy();
            int addedCount = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var jobProfileToCreate = new List<JP>();

                    foreach (var row in validRows)
                    {
                        var ptLookupKey = (Index: row["Index"].ToLowerInvariant(), RoleName: row["Role"].ToLowerInvariant());
                        if (!profileTemplateMaps.TryGetValue(ptLookupKey, out var profileTemplateId))
                        {
                            logger.LogInformation($"Profile template data not found for index: {row["Index"]} and role name: {row["Role"]}");
                            continue;
                        }

                        if (!employeeIdMaps.TryGetValue(row["Unique ID"], out var employeeId))
                        {
                            logger.LogInformation($"Employee data not found for {row["Unique ID"]}");
                            continue;
                        }

                        var lookUpKey = (Pt: profileTemplateId.ToString(), E: employeeId);
                        if (jpLookUpMap.TryGetValue(lookUpKey, out var jpId))
                        {
                            logger.LogInformation($"Skipping because already exists");
                            continue;
                        }

                        var jp = new JP
                        {
                            ProfileTemplateId = profileTemplateId,
                            EmployeeId = employeeId
                        };
                        jobProfileToCreate.Add(jp);
                        addedCount++;
                    }

                    if (jobProfileToCreate.Count > 0)
                    {
                        await context.JobProfile.AddRangeAsync(jobProfileToCreate);
                        await context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    TempData["success"] = $"Successfully imported {addedCount} job profiles";
                }
                catch (Exception ex)
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
            var pt = await jobProfileService.GetJobProfileByIdAsync(id);
            if (pt == null) return NotFound();
            JobProfileViewModel pvm = new()
            {
                Id = id,
                Employees = (await employeeService.GetEmployeeOptionAsync(pt.EmployeeId)),
                ProfileTemplates = (await profileTemplateService.GetProfileTemplateOptionAsync(pt.ProfileTemplateId)),
                EmployeeId = pt.EmployeeId,
                ProfileTemplateId = pt.ProfileTemplateId,
                //JoinDate = pt.JoinDate,
                //EndDate = pt.EndDate,
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobProfileViewModel pvm)
        {
            var pt = await jobProfileService.UpdateJobProfileASync(id, pvm);
            if (pt == null)
            {
                TempData["error"] = "Unexpected error occured. No job profile found";
                return View(new JobProfileViewModel
                {
                    Id = id,
                    Employees = (await employeeService.GetEmployeeOptionAsync(pvm.EmployeeId)),
                    ProfileTemplates = (await employeeService.GetEmployeeOptionAsync(pvm.ProfileTemplateId)),
                    EmployeeId = pvm.EmployeeId,
                    ProfileTemplateId = pvm.ProfileTemplateId,
                    //JoinDate = pvm.JoinDate,
                    //EndDate = pvm.EndDate,
                });
            }
            TempData["success"] = "Job profile successfully updated";
            return View(new JobProfileViewModel
            {
                Id = id,
                Employees = (await employeeService.GetEmployeeOptionAsync(pt.EmployeeId)),
                ProfileTemplates = (await employeeService.GetEmployeeOptionAsync(pt.ProfileTemplateId)),
                EmployeeId = pt.EmployeeId,
                ProfileTemplateId = pt.ProfileTemplateId,
                //JoinDate = pt.JoinDate,
                //EndDate = pt.EndDate,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await jobProfileService.DeleteJobProfileAsync(id);
                TempData["success"] = "Successfully deleted the job profile";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
