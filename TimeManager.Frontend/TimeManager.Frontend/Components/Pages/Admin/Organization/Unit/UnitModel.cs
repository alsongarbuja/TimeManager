using System.ComponentModel.DataAnnotations;
using TimeManager.Frontend.Components.Pages.Admin.Organization.Department;

namespace TimeManager.Frontend.Components.Pages.Admin.Organization.Unit
{
    public class UnitModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage="Index must be greater than 0"), Required(ErrorMessage = "Index is required")]
        public int Index { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot be longer than 100 characters")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        public DepartmentModel? Department { get; set; }
    }
}
