using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Requests;
using TimeManager.Backend.Models.Responses;
using TimeManager.Backend.Services;
using TimeManager.Backend.Utility;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Punch
{
    [Authorize(Policy = "AdminPolicy")]
    public class PunchController(IPunchServices punchServices, IJobProfileService jobProfileService) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] PaginationQuery pagFilter, [FromQuery] FilterCondition filter)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            PagedResponse<PunchViewModel> data = await punchServices.GetPunchesAsync(departmentId, pagFilter, filter);
            IEnumerable<SelectListItem> employees = await jobProfileService.GetUserOptionsAsync(departmentId);
            return View(new PunchViewOverall
            {
                Employees = employees,
                Data = data,
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var punch = await punchServices.GetPunchByIdAsync(id);
            if (punch == null) return NotFound();
            return View(new PunchViewModel
            {
                Id = punch.Id,
                ClockInTime = punch.ClockIn,
                ClockOutTime = punch.ClockOut,
                EmployeeId = punch.JobProfileId,
                Name = $"{punch.JobProfile.Employee.FirstName} {punch.JobProfile.Employee.LastName}"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PunchViewModel pvm)
        {
            if (!ModelState.IsValid) return View(pvm);
            var d = await punchServices.UpdatePunchAsync(id, new PunchDto
            {
                ClockIn = pvm.ClockInTime,
                ClockOut = pvm.ClockOutTime,
            });

            if (d == null)
            {
                TempData["error"] = "Error while updating the data";
                return View(pvm);
            }

            TempData["success"] = "Successfully edited the data";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await punchServices.DeletePunchByIdAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
