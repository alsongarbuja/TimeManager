using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string UniqueId { get; set; } = string.Empty;
    }
}
