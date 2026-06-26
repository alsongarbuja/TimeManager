using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }
    }

    public class RegisterViewModel
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [MinLength(8, ErrorMessage = "Password must be atleast 8 characters long")]
        [StringLength(100, ErrorMessage = "Password cannot be more than 100 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).+$",
    ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role is required"), Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> AvailableRoles { get; set; } = [];
    }
}
