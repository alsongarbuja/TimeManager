using Microsoft.AspNetCore.Mvc.RazorPages;
using R = TimeManager.Backend.Models.Employee_Management.Role;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.Role
{
    public class IndexModel : PageModel
    {
        private readonly IRoleService roleService;
        public IEnumerable<R> Roles;

        public IndexModel(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        public async Task OnGetAsync()
        {
            Roles = await this.roleService.GetRolesAsync();
        }
    }
}
