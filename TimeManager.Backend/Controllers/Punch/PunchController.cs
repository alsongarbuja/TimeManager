using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Controllers.PunchManagement.Dto;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Punch
{
    [Authorize(Policy = "AdminPolicy")]
    public class PunchController : Controller
    {
        private readonly IPunchServices punchServices;

        public PunchController(IPunchServices punchServices)
        {
            this.punchServices = punchServices;
        }

        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var data = await punchServices.GetPunchesAsync(departmentId);
            return View(data);
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
    }
}
