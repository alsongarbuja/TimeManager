using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Models;

namespace TimeManager.Backend.ViewModels
{
    public class SettingViewModel
    {
        public Preferences Preferences { get; set; } = new();
        public ChangePasswordModel PasswordModel { get; set; } = new();

        public IEnumerable<SelectListItem> LimitOptions { get; set; } = [];
        public IEnumerable<SelectListItem> PunchesLimitOptions { get; set; } = [];
        public IEnumerable<SelectListItem> EmployeesLimitOptions { get; set; } = [];
        public IEnumerable<SelectListItem> JobProfilesLimitOptions { get; set; } = [];
        public IEnumerable<SelectListItem> PunchesOrderByOptions { get; set; } = [];
        public IEnumerable<SelectListItem> EmployeesOrderByOptions { get; set; } = [];
        public IEnumerable<SelectListItem> JobProfilesOrderByOptions { get; set; } = [];
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
