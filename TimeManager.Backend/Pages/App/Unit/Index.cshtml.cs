using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using U = TimeManager.Backend.Models.Organization_Management.Unit;
using TimeManager.Backend.Services;

namespace TimeManager.Backend.Pages.App.Unit
{
    public class IndexModel : PageModel
    {
        private readonly IUnitService unitService;
        public IEnumerable<U> Units;

        public IndexModel(IUnitService unitService)
        {
            this.unitService = unitService;
        }

        public async Task OnGetAsync()
        {
            Units = await this.unitService.GetUnitsAysnc();
        }
    }
}
