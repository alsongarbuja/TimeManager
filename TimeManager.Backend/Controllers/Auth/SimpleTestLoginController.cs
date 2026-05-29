using Microsoft.AspNetCore.Mvc;

namespace TimeManager.Backend.Controllers.Auth
{
    [IgnoreAntiforgeryToken]
    [ApiController]
    [Route("api/auth")]
    public class SimpleTestLoginController: ControllerBase
    {
        [HttpPost("simpletestlogin")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto loginData)
        {
            await Task.Delay(2000);

            if (loginData.Username.Equals("admin") && loginData.Password.Equals("fm@12345"))
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Status = "OK",
                    Message = "Logged in successfully"
                });
            }

            return BadRequest(new
            {
                StatusCode = 400,
                Status = "Failed",
                Message = "Username or password not matched"
            });
        }
    }
}
