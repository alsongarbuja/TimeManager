using Microsoft.AspNetCore.Mvc.RazorPages;
using E = TimeManager.Backend.Models.Employee_Management.Employee;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.Employee
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService employeeService;
        public IEnumerable<E> Employees;


        public IndexModel(IEmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }

        public async Task OnGetAsync()
        {
            Employees = await this.employeeService.GetEmployeesAsync();
        }
    }
}
