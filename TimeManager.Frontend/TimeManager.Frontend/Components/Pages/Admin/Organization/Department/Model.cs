using System.ComponentModel.DataAnnotations;

namespace TimeManager.Frontend.Components.Pages.Admin.Organization.Department
{
    public class DepartmentModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Description { get; set; } = "";
    }
}
