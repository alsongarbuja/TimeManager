using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.JobProfile
{
    [Authorize(Policy = "AdminPolicy")]
    public class JobProfileController(IJobProfileService jobProfileService, IEmployeeService employeeService, IProfileTemplateService profileTemplateService) : Controller
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
                JoinDate = pt.JoinDate,
                EndDate = pt.EndDate,
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
                return View(new JobProfileViewModel
                {
                    Id = id,
                    Employees = (await employeeService.GetEmployeeOptionAsync(pvm.EmployeeId)),
                    ProfileTemplates = (await employeeService.GetEmployeeOptionAsync(pvm.ProfileTemplateId)),
                    EmployeeId = pvm.EmployeeId,
                    ProfileTemplateId = pvm.ProfileTemplateId,
                    JoinDate = pvm.JoinDate,
                    EndDate = pvm.EndDate,
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
                JoinDate = pt.JoinDate,
                EndDate = pt.EndDate,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await jobProfileService.DeleteJobProfileAsync(id);
                TempData["success"] = "Successfully deleted the data";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
