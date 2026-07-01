using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.JobProfile
{
    [Authorize(Policy = "AdminPolicy")]
    public class JobProfileController : Controller
    {
        private readonly IJobProfileService jobProfileService;
        private readonly IEmployeeService employeeService;
        private readonly IProfileTemplateService profileTemplateService;

        public JobProfileController(IJobProfileService jobProfileService, IEmployeeService employeeService, IProfileTemplateService profileTemplateService)
        {
            this.jobProfileService = jobProfileService;
            this.employeeService = employeeService;
            this.profileTemplateService = profileTemplateService;
        }

        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var jp = await jobProfileService.GetJobProfilesAsync(departmentId);
            return View(jp);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            JobProfileViewModel pvm = new JobProfileViewModel
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
            JobProfileViewModel pvm = new JobProfileViewModel
            {
                Id = id,
                Employees = (await employeeService.GetEmployeeOptionAsync(pt.EmployeeId)),
                ProfileTemplates = (await profileTemplateService.GetProfileTemplateOptionAsync(pt.ProfileTemplateId)),
                EmployeeId = pt.EmployeeId,
                ProfileTemplateId = pt.ProfileTemplateId,
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobProfileViewModel pvm)
        {
            var pt = await jobProfileService.UpdateJobProfileASync(id, pvm);
            //JobProfileViewModel pv = new JobProfileViewModel
            //{
            //    Id = id,
            //    Employees = (await employeeService.GetEmployeeOptionAsync()),
            //    ProfileTemplates = (await employeeService.GetEmployeeOptionAsync()),
            //    EmployeeId = pt.EmployeeId,
            //    ProfileTemplateId = pt.ProfileTemplateId,
            //};
            return RedirectToAction(nameof(Index));
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
