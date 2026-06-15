using Microsoft.AspNetCore.Mvc.RazorPages;
using PF = TimeManager.Backend.Models.Employee_Management.ProfileTemplate;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.ProfileTemplate
{
    public class IndexModel : PageModel
    {
        private readonly IProfileTemplateService profileTemplateService;
        public IEnumerable<PF> ProfileTemplates; 

        public IndexModel(IProfileTemplateService profileTemplateService)
        {
            this.profileTemplateService = profileTemplateService;
        }

        public async Task OnGetAsync()
        {
            ProfileTemplates = await profileTemplateService.GetProfileTemplatesAsync();
        }
    }
}
