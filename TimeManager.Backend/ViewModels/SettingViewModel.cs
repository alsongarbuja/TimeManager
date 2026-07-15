using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class SettingViewModel
    {

    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Old Password is required"), DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required"), DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required"), DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}
