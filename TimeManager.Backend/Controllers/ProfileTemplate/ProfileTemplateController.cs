using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.ProfileTemplate
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ProfileTemplateController : Controller
    {
        private readonly IProfileTemplateService profileTemplateService;
        private readonly IUnitService unitService;
        private readonly IRoleService roleService;
        private readonly IEmployeeTypeService employeeTypeService;
        private readonly IPayFrequencyService payFrequencyService;
        private readonly IConfiguration configuration;

        public ProfileTemplateController(
            IProfileTemplateService profileTemplateService, 
            IUnitService unitService, 
            IRoleService roleService, 
            IEmployeeTypeService employeeTypeService,
            IPayFrequencyService payFrequencyService,
            IConfiguration configuration
        )
        {
            this.profileTemplateService = profileTemplateService;
            this.unitService = unitService;
            this.roleService = roleService;
            this.employeeTypeService = employeeTypeService;
            this.payFrequencyService = payFrequencyService;
            this.configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var profileTemplates = await profileTemplateService.GetProfileTemplatesAsync(departmentId);
            return View(profileTemplates);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            ProfileTemplateViewModel pvm = new ProfileTemplateViewModel
            {
                Units = (await unitService.GetUnitReportOptionsAsync(departmentId)),
                Roles = (await roleService.GetRoleOptionsAsync()),
                EmployeeTypes = (await employeeTypeService.GetEmployeeTypeOptionsAsync()),
                PayFrequencies = (await payFrequencyService.GetPayFrequencyOptionsAsync())
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfileTemplateViewModel pvm)
        {
            await profileTemplateService.CreateProfileTemplateAsync(pvm);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var pt = await profileTemplateService.GetProfileTemplateByIdAsync(id);
            if (pt == null) return NotFound();
            int? departmentId = HttpContext.Session.GetDepartmentId();
            ProfileTemplateViewModel pvm = new ProfileTemplateViewModel
            {
                Id = id,
                UnitId = pt.UnitId,
                EmployeeTypeId = pt.EmployeeTypeId,
                PayFrequencyId = pt.PayFrequencyId,
                RoleId = pt.RoleId,
                Units = (await unitService.GetUnitReportOptionsAsync(departmentId)),
                Roles = (await roleService.GetRoleOptionsAsync()),
                EmployeeTypes = (await employeeTypeService.GetEmployeeTypeOptionsAsync()),
                PayFrequencies = (await payFrequencyService.GetPayFrequencyOptionsAsync())
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfileTemplateViewModel pvm)
        {
            var pt = await profileTemplateService.UpdateProfileTemplateASync(id, pvm);
            int? departmentId = HttpContext.Session.GetDepartmentId();
            ProfileTemplateViewModel pv = new ProfileTemplateViewModel
            {
                Id = id,
                UnitId = pt.UnitId,
                EmployeeTypeId = pt.EmployeeTypeId,
                PayFrequencyId = pt.PayFrequencyId,
                RoleId = pt.RoleId,
                Units = (await unitService.GetUnitReportOptionsAsync(departmentId)),
                Roles = (await roleService.GetRoleOptionsAsync()),
                EmployeeTypes = (await employeeTypeService.GetEmployeeTypeOptionsAsync()),
                PayFrequencies = (await payFrequencyService.GetPayFrequencyOptionsAsync())
            };
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await profileTemplateService.DeleteProfileTemplateAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
