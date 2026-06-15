using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Department
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            this.departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await this.departmentService.GetDepartmentsAsync();
            return View(departments);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create() => View(new DepartmentViewModel());
    }
}
