using Microsoft.AspNetCore.Mvc.RazorPages;
using Deprt = TimeManager.Backend.Models.Organization_Management.Department;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.Department
{
    public class IndexModel : PageModel
    {
        private readonly IDepartmentService _departmentService;
        public IEnumerable<Deprt> Departments { get; set; }

        public IndexModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public async Task OnGetAsync()
        {
            Departments = await _departmentService.GetDepartmentsAsync();
        }
    }
}
