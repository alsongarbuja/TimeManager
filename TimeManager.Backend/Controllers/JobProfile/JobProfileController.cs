using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;
using TimeManager.Backend.ViewModels.PartialViews;
using JP = TimeManager.Backend.Models.Employee_Management.JobProfile;

namespace TimeManager.Backend.Controllers.JobProfile
{
    [Authorize(Policy = "AdminPolicy")]
    public class JobProfileController(
        IJobProfileService jobProfileService, 
        IEmployeeService employeeService, 
        IJobHistoryService jobHistoryService,
        IProfileTemplateService profileTemplateService,
        IExcelService excelService,
        ICacheService cacheService,
        HrmsDbContext context,
        ILogger<JP> logger
        ) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] PaginationQuery query, [FromQuery] FilterCondition filter)
        {
            var uClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
             
            if (!int.TryParse(uClaim, out int userId))
            {
                return Unauthorized();
            }

            Preferences prefs = await cacheService.GetPreferencesAsync(userId);
            int? departmentId = HttpContext.Session.GetDepartmentId();
            PagedResponse<JobProfileViewModel> jp = await jobProfileService.GetJobProfilesAsync(
                departmentId, 
                query, 
                filter,
                new PaginationQuery
                {
                    PageSize = prefs.JobProfilesPref.Limit,
                    OrderBy = prefs.JobProfilesPref.OrderBy,
                    IsOrderDescending = prefs.JobProfilesPref.IsOrderDescending,
                });
            IEnumerable<SelectListItem> employees = await jobProfileService.GetUserOptionsAsync(departmentId);
            return View(new JobProfileOverall
            {
                Employees = employees,
                JobProfiles = jp,
            });
        }

        [HttpGet]
        public async Task<IActionResult> AddJobHistoryRow(int index)
        {
            var model = new JobHistoryRowViewModel
            {
                ProfileTemplates = await profileTemplateService.GetProfileTemplateOptionAsync()
            };

            ViewData.TemplateInfo.HtmlFieldPrefix = $"JobHistories[{Guid.NewGuid():N}]";

            return PartialView("JobHistorySection", model);
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
            var jobHistories = await jobHistoryService.GetJobHistoriesByProfileId(id);
            var profileTemplates = await profileTemplateService.GetProfileTemplateOptionAsync();
            JobProfileViewModel pvm = new()
            {
                Id = id,
                Employees = (await employeeService.GetEmployeeOptionAsync(pt.EmployeeId)),
                ProfileTemplates = (await profileTemplateService.GetProfileTemplateOptionAsync(pt.ProfileTemplateId)),
                EmployeeId = pt.EmployeeId,
                ProfileTemplateId = pt.ProfileTemplateId,
                EarlyBuffer = pt.EarlyBuffer,
                JobHistories = jobHistories.Select(j => new JobHistoryRowViewModel
                {
                    Id = j.Id,
                    JobProfileId = j.JobProfileId,
                    JoinDate = j.JoinDate.ToLocalTime(),
                    EndDate = j.EndDate?.ToLocalTime(),
                    ProfileTemplateId = j.ProfileTemplateId,
                    ProfileTemplates = profileTemplates.Select(pt => new SelectListItem
                    {
                        Text = pt.Text,
                        Value = pt.Value,
                        Selected = pt.Value.ToString() == j.ProfileTemplateId.ToString()
                    })
                })
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobProfileViewModel pvm)
        {
            Console.WriteLine(pvm.JobHistories.Count());
            foreach (var jh in pvm.JobHistories)
            {
                Console.WriteLine("END DATE => " + jh.EndDate);
                if (jh.Id == null)
                {
                    await jobHistoryService.CreateJobHistoryForProfileId(pvm.Id, jh);
                } else
                {
                    await jobHistoryService.UpdateJobHistoryById((int)jh.Id, jh);
                }
            }

            var pt = pvm.JobHistories.FirstOrDefault(j => j.EndDate == null);

            var jp = await jobProfileService.UpdateJobProfileASync(id, pvm);
            if (jp == null)
            {
                TempData["error"] = "Unexpected error occured. No job profile found";
                return View(new JobProfileViewModel
                {
                    Id = id,
                    Employees = (await employeeService.GetEmployeeOptionAsync(pvm.EmployeeId)),
                    ProfileTemplates = (await profileTemplateService.GetProfileTemplateOptionAsync(pvm.ProfileTemplateId)),
                    EmployeeId = pvm.EmployeeId,
                    ProfileTemplateId = pt.ProfileTemplateId,
                    EarlyBuffer = pvm.EarlyBuffer,
                });
            }
            TempData["success"] = "Job profile successfully updated";
            return View(new JobProfileViewModel
            {
                Id = id,
                Employees = (await employeeService.GetEmployeeOptionAsync(jp.EmployeeId)),
                ProfileTemplates = (await employeeService.GetEmployeeOptionAsync(jp.ProfileTemplateId)),
                EmployeeId = jp.EmployeeId,
                ProfileTemplateId = jp.ProfileTemplateId,
                EarlyBuffer = jp.EarlyBuffer,
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
