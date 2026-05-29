using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Name cannot be more than 100 letters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be atleast 8 characters long")]
        public string Password { get; set; } = string.Empty;
    }
}
