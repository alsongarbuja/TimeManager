using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.ProfileTemplate
{
    [Authorize(Policy = "AdminPolicy")]
    public class ProfileTemplateController(
        IProfileTemplateService profileTemplateService,
        IUnitService unitService,
        IRoleService roleService,
        IEmployeeTypeService employeeTypeService,
        IPayFrequencyService payFrequencyService
        ) : Controller
    {
        public async Task<IActionResult> Index(PaginationFilter filter)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var profileTemplates = await profileTemplateService.GetProfileTemplatesAsync(departmentId, filter);
            return View(profileTemplates);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            ProfileTemplateViewModel pvm = new()
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
            ProfileTemplateViewModel pvm = new()
            {
                Id = id,
                UnitId = pt.UnitId,
                EmployeeTypeId = pt.EmployeeTypeId,
                PayFrequencyId = pt.PayFrequencyId,
                RoleId = pt.RoleId,
                Units = (await unitService.GetUnitReportOptionsAsync(departmentId, pt.UnitId)),
                Roles = (await roleService.GetRoleOptionsAsync(pt.RoleId)),
                EmployeeTypes = (await employeeTypeService.GetEmployeeTypeOptionsAsync(pt.EmployeeTypeId)),
                PayFrequencies = (await payFrequencyService.GetPayFrequencyOptionsAsync(pt.PayFrequencyId)),
                ShiftStartTime = pt.ShiftStartTime,
                EarlyClockInBufferMin = pt.EarlyClockInBufferMin,
            };
            return View(pvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfileTemplateViewModel pvm)
        {
            try
            {
                var pt = await profileTemplateService.UpdateProfileTemplateASync(id, pvm);
                if (pt == null)
                {
                    TempData["error"] = "Profile Template not updated. Please try again later";
                    return RedirectToAction(nameof(Index));
                }
                int? departmentId = HttpContext.Session.GetDepartmentId();
                ProfileTemplateViewModel pv = new()
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
            } catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
                int? departmentId = HttpContext.Session.GetDepartmentId();
                return View(new ProfileTemplateViewModel
                {
                    Id = id,
                    UnitId = pvm.UnitId,
                    EmployeeTypeId = pvm.EmployeeTypeId,
                    PayFrequencyId = pvm.PayFrequencyId,
                    RoleId = pvm.RoleId,
                    Units = (await unitService.GetUnitReportOptionsAsync(departmentId, pvm.UnitId)),
                    Roles = (await roleService.GetRoleOptionsAsync(pvm.RoleId)),
                    EmployeeTypes = (await employeeTypeService.GetEmployeeTypeOptionsAsync(pvm.EmployeeTypeId)),
                    PayFrequencies = (await payFrequencyService.GetPayFrequencyOptionsAsync(pvm.PayFrequencyId))
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await profileTemplateService.DeleteProfileTemplateAsync(id);
                TempData["success"] = "Successfully removed the data";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
