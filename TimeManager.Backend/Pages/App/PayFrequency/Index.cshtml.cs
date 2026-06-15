using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PF = TimeManager.Backend.Models.Employee_Management.PayFrequency;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.PayFrequency
{
    public class IndexModel : PageModel
    {
        private readonly IPayFrequencyService payFrequencyService;
        public IEnumerable<PF> PayFrequencies;

        public IndexModel(IPayFrequencyService payFrequencyService)
        {
            this.payFrequencyService = payFrequencyService;
        }

        public async Task OnGetAsync()
        {
            PayFrequencies = await this.payFrequencyService.GetPayFrequenciesAsync();
        }
    }
}
