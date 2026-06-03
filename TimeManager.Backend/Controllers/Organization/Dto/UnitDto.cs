using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Controllers.Organization.Dto
{
    public class UnitDto
    {
        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Index is required")]
        public int Index { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot be longer than 100")]
        public string? Description { get; set; }
    }
}
