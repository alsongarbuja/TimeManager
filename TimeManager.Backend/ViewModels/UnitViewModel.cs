using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.ViewModels
{
    public class UnitViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "DepartmentId")]
        public int DepartmentId { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
