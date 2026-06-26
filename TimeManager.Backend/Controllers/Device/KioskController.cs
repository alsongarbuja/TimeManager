using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TimeManager.Backend.Controllers.Device.Dto;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Controllers.Device
{
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController : ControllerBase
    {
        private readonly IKioskService kioskService;

        public KioskController(IKioskService kioskService)
        {
            this.kioskService = kioskService;
        }

        [HttpPost("init")]
        [AllowAnonymous]
        public async Task<IActionResult> Init()
        {
            var clientIp = GetClientIp();

            if (clientIp == null)
            {
                return Unauthorized(new { error = "Unable to determine client Ip address" });
            }

            var kiosk = await kioskService.ResolveKioskByIpAsync(clientIp);
            if (kiosk == null)
            {
                return Unauthorized(new { error = "This device is not authorized as a kiosk" });
            }

            var token = kioskService.GenerateKisokToken(kiosk);

            return Ok(new KioskSessionResponse(
                Token: token,
                KioskName: kiosk.Name,
                DepartmentId: kiosk.DepartmentId,
                DepartmentName: kiosk.Department.Name,
                ExpiresAt: DateTime.UtcNow.AddHours(12)
            ));
        }

        private IPAddress? GetClientIp()
        {
            // In production, scope to known proxy IPs.
            var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var firstIp = forwarded.Split(',')[0].Trim();
                if (IPAddress.TryParse(firstIp, out _))
                    return IPAddress.Parse(firstIp);
            }

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4();
        }
    }
}
