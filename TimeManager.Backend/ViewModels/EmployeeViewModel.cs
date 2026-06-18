using Microsoft.AspNetCore.Mvc.Rendering;

namespace TimeManager.Backend.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UniqueId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class EmployeeData
    {
        public EmployeeViewModel EmployeeView { get; set; }

        public int UserId { get; set; }
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }
}
