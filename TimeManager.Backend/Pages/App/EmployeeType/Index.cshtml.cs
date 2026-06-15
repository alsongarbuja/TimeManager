using Microsoft.AspNetCore.Mvc.RazorPages;
using ET = TimeManager.Backend.Models.Employee_Management.EmployeeType;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.EmployeeType
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeTypeService employeeTypeService;
        public IEnumerable<ET> EmployeeTypes;

        public IndexModel(IEmployeeTypeService employeeTypeService)
        {
            this.employeeTypeService = employeeTypeService;
        }

        public async Task OnGetAsync()
        {
            EmployeeTypes = await this.employeeTypeService.GetEmployeeTypesAsync();
        }
    }
}
