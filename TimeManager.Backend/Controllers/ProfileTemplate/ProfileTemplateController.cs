using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.ProfileTemplate
{
    public class ProfileTemplateController : Controller
    {
        private readonly IProfileTemplateService profileTemplateService;
        private readonly IUnitService unitService;
        private readonly IRoleService roleService;
        private readonly IEmployeeTypeService employeeTypeService;
        private readonly IPayFrequencyService payFrequencyService;

        public ProfileTemplateController(
            IProfileTemplateService profileTemplateService, 
            IUnitService unitService, 
            IRoleService roleService, 
            IEmployeeTypeService employeeTypeService,
            IPayFrequencyService payFrequencyService
        )
        {
            this.profileTemplateService = profileTemplateService;
            this.unitService = unitService;
            this.roleService = roleService;
            this.employeeTypeService = employeeTypeService;
            this.payFrequencyService = payFrequencyService;
        }

        public async Task<IActionResult> Index()
        {
            var profileTemplates = await profileTemplateService.GetProfileTemplatesAsync();
            return View(profileTemplates);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ProfileTemplateViewModel pvm = new ProfileTemplateViewModel
            {
                Units = (await unitService.GetUnitReportOptionsAsync()),
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
            ProfileTemplateViewModel pvm = new ProfileTemplateViewModel
            {
                Id = id,
                UnitId = pt.UnitId,
                EmployeeTypeId = pt.EmployeeTypeId,
                PayFrequencyId = pt.PayFrequencyId,
                RoleId = pt.RoleId,
                Units = (await unitService.GetUnitReportOptionsAsync()),
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
            ProfileTemplateViewModel pv = new ProfileTemplateViewModel
            {
                Id = id,
                UnitId = pt.UnitId,
                EmployeeTypeId = pt.EmployeeTypeId,
                PayFrequencyId = pt.PayFrequencyId,
                RoleId = pt.RoleId,
                Units = (await unitService.GetUnitReportOptionsAsync()),
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
