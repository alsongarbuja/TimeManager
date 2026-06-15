using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.JobProfile
{
    public class IndexModel : PageModel
    {
        private readonly IJobProfileService jobProfileService;
        public IEnumerable<JobProfileData> JobProfiles;

        public IndexModel(IJobProfileService jobProfileService)
        {
            this.jobProfileService = jobProfileService;
        }

        public async Task OnGetAsync()
        {
            JobProfiles = await this.jobProfileService.GetJobProfilesAsync();
        }
    }
}
