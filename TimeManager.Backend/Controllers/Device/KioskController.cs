using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Common;
using TimeManager.Backend.Controllers.Device.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers.Device
{
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController(
        IKioskService kioskService,
        HrmsDbContext context,
        UserManager<U> userManager,
        IDepartmentService departmentService
    ) : ControllerBase
    {
        [HttpPost("init")]
        [AllowAnonymous]
        public async Task<IActionResult> Init([FromBody] KioskSetupViewModel data)
        {
            if (string.IsNullOrWhiteSpace(data.UniqueId))
            {
                return BadRequest(new { message = "An ID is required to set up this kiosk" });
            }

            var user = await context.Users.Where(u => u.UniqueId == data.UniqueId).FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { message = "User not authorized to setup Kiosk" });
            }

            var roles = await userManager.GetRolesAsync(user);

            if (!roles.Contains(AppConstants.SUPER_ADMIN_ROLE))
            {
                return Unauthorized(new { message = "User is not authorized to setup kiosk" });
            }

            return Ok(new
            {
                message = "Successfully identified authorized user",
            });
        }

        [HttpGet("departments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDeptOptsAsync()
        {
            return Ok(new
            {
                departments = await departmentService.GetDepartmentOptionsAsync()
            });
        }

        [HttpPost("provision")]
        [AllowAnonymous]
        public async Task<IActionResult> Provision([FromBody] KioskViewModel kvm)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return BadRequest(new { message = firstError ?? "Invalid kiosk details provided" });
            }

            try
            {
                string token = await kioskService.CreateKioskAsync(kvm);
                return Ok(new KioskSessionResponse(Token: token));
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Unable to provision this kiosk. Please try again or contact IT." });
            }
        }

        [HttpPost("{id:int}/revoke")]
        [Authorize(Roles = AppConstants.SUPER_ADMIN_ROLE)]
        public async Task<IActionResult> RevokeToken(int id)
        {
            string newToken = await kioskService.RegenerateKioskTokenAsync(id);
            return Ok(new KioskSessionResponse(Token: newToken));
        }
    }
}