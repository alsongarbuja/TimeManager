using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models;
using TimeManager.Backend.Models.Employee_Management;
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
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var model = new JobHistoryRowViewModel
            {
                ProfileTemplates = await profileTemplateService.GetProfileTemplateOptionAsync(departmentId)
            };

            ViewData.TemplateInfo.HtmlFieldPrefix = $"JobHistories[{index}]";

            return PartialView("JobHistorySection", model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            JobProfileViewModel pvm = new()
            {
                Employees = (await employeeService.GetEmployeeOptionAsync(departmentId)),
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobProfileViewModel pvm)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            if (pvm.JobHistories.Count <= 0)
            {
                TempData["error"] = "Please add atleast one job history to continue";
                return View(new JobProfileViewModel
                {
                    Employees = (await employeeService.GetEmployeeOptionAsync(departmentId)),
                });
            }

            int? id = await jobProfileService.CreateJobProfileAsync(new JobProfileViewModel
            {
                EarlyBuffer = pvm.EarlyBuffer,
                EmployeeId = pvm.EmployeeId,
                ProfileTemplateId = pvm.JobHistories[0].ProfileTemplateId,
            });
            
            if (id == null)
            {
                TempData["error"] = "Error while creating the job profile.";
                return View(new JobProfileViewModel
                {
                    Employees = (await employeeService.GetEmployeeOptionAsync(departmentId)),
                });
            }

            await jobHistoryService.CreateJobHistoryForProfileId((int)id, pvm.JobHistories[0]);
            TempData["success"] = "Job Profile successfully created";
            return View(new JobProfileViewModel
            {
                Employees = (await employeeService.GetEmployeeOptionAsync(departmentId)),
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(IFormFile excelFile)
        {
            (List<Dictionary<string, string>> data, string? error) = excelService.ParseExcelFileToList(excelFile, ["Index", "Unique ID", "Role", "Hire Date"]);

            if (!string.IsNullOrEmpty(error))
            {
                TempData["error"] = error;
                return RedirectToAction(nameof(Index));
            }

            var validRows = data.Where(item =>
                    !string.IsNullOrEmpty(item["Index"]) &&
                    !string.IsNullOrEmpty(item["Unique ID"]) &&
                    !string.IsNullOrEmpty(item["Role"]) &&
                    !string.IsNullOrEmpty(item["Hire Date"])
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

                    var historyTrackingList = new List<(JP JobProfile, int ProfileTemplateId, string HireDate)>();

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
                        historyTrackingList.Add((jp, profileTemplateId, row["Hire Date"]));
                        addedCount++;
                    }

                    if (jobProfileToCreate.Count > 0)
                    {
                        await context.JobProfile.AddRangeAsync(jobProfileToCreate);
                        await context.SaveChangesAsync();

                        var jobHistoriesToCreate = new List<JobHistory>();

                        foreach (var item in historyTrackingList)
                        {
                            if (DateTime.TryParse(item.HireDate, out var JoinDate))
                            {
                                jobHistoriesToCreate.Add(new JobHistory { 
                                    JobProfileId = item.JobProfile.Id,
                                    ProfileTemplateId = item.ProfileTemplateId,
                                    JoinDate = JoinDate.ToUniversalTime(),
                                    EndDate = null
                                });
                            } else
                            {
                                logger.LogWarning($"Failed to parse hire date string: {item.HireDate}");
                            }
                        }

                        if (jobHistoriesToCreate.Count > 0)
                        {
                            await context.JobHistory.AddRangeAsync(jobHistoriesToCreate);
                            await context.SaveChangesAsync();
                        }
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
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var pt = await jobProfileService.GetJobProfileByIdAsync(id);
            if (pt == null) return NotFound();
            var jobHistories = await jobHistoryService.GetJobHistoriesByProfileId(id);
            var profileTemplates = await profileTemplateService.GetProfileTemplateOptionAsync(departmentId);
            JobProfileViewModel pvm = new()
            {
                Id = id,
                Employees = (await employeeService.GetEmployeeOptionAsync(departmentId, pt.EmployeeId)),
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
                }).ToList()
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobProfileViewModel pvm)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var profileTemplates = await profileTemplateService.GetProfileTemplateOptionAsync(departmentId);
            
            async Task PopulateViewDropdownsAsync()
            {
                pvm.Employees = await employeeService.GetEmployeeOptionAsync(departmentId, pvm.EmployeeId);

                foreach (var jh in pvm.JobHistories)
                {
                    jh.ProfileTemplates = await profileTemplateService.GetProfileTemplateOptionAsync(departmentId, jh.ProfileTemplateId);
                    jh.JoinDate = jh.JoinDate.ToLocalTime();
                    jh.EndDate = jh.EndDate?.ToLocalTime();
                }
            }

            if (pvm.JobHistories.Count <= 0)
            {
                TempData["error"] = "Add at least one job history to save the profile";
                await PopulateViewDropdownsAsync();
                return View(pvm);
            }

            var prevJH = await jobHistoryService.GetJobHistoriesByProfileId(pvm.Id);
            if (pvm.JobHistories.Count < prevJH.Count)
            {
                var prevJHIds = prevJH.Select(p => p.Id);
                var currentJHIds = pvm.JobHistories.Select(j => j.Id);

                for (int i = 0; i < prevJHIds.Count(); i++)
                {
                    if (!currentJHIds.Contains(prevJHIds.ElementAt(i)))
                    {
                        await jobHistoryService.DeleteJobHistoryById(prevJHIds.ElementAt(i));
                    }
                }
            }

            foreach (var jh in pvm.JobHistories)
            {
                if (jh.Id == null)
                {
                    await jobHistoryService.CreateJobHistoryForProfileId(pvm.Id, jh);
                } else
                {
                    await jobHistoryService.UpdateJobHistoryById((int)jh.Id, jh);
                }
            }

            var pt = pvm.JobHistories.FirstOrDefault(j => j.EndDate == null);
            pvm.ProfileTemplateId = pt != null ? pt.ProfileTemplateId : pvm.JobHistories[^1].ProfileTemplateId;

            var jp = await jobProfileService.UpdateJobProfileASync(id, pvm);
            if (jp == null)
            {
                TempData["error"] = "Unexpected error occured. No job profile found";
                return View(pvm);
            }
            await PopulateViewDropdownsAsync();
            TempData["success"] = "Job profile successfully updated";
            return View(new JobProfileViewModel
            {
                Id = id,
                Employees = (await employeeService.GetEmployeeOptionAsync(departmentId, jp.EmployeeId)),
                EmployeeId = jp.EmployeeId,
                ProfileTemplateId = jp.ProfileTemplateId,
                EarlyBuffer = jp.EarlyBuffer,
                JobHistories = pvm.JobHistories,
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
