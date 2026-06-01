using System.ComponentModel.DataAnnotations;

namespace TimeManager.Frontend.Components.Pages.Admin.Organization.Unit
{
    public class UnitModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage="Index must be greater than 0"), Required(ErrorMessage = "Index is required")]
        public int Index { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot be longer than 100 characters")]
        public string Description { get; set; } = "";
    }
}
