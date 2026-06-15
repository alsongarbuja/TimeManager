using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.Department
{
    public class AddModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Department name is required")]
        public string Name { get; set; }


        [BindProperty]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        private readonly IDepartmentService departmentService;

        public AddModel(IDepartmentService departmentService)
        {
            this.departmentService = departmentService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await this.departmentService.CreateDepartmentAsync(new DepartmentDto
            {
                Name = Name,
                Description = Description,
            });

            return Page();
        }
    }
}
