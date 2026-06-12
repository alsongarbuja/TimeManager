using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeManager.Backend.Models.Employee_Management;
using Unit = TimeManager.Backend.Models.Organization_Management.Unit;
using TimeManager.Backend.Models.Punch_Management;
using TimeManager.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace TimeManager.Backend.Pages.App
{
    public class ReportModel : PageModel
    {
        private readonly IPayPeriodService _payPeriodService;
        private readonly IJobProfileService _jobProfileService;
        //private readonly IUnitService _unitService;

        public ReportModel(IPayPeriodService payPeriodService, IJobProfileService jobProfileService, IUnitService unitService)
        {
            _payPeriodService = payPeriodService;
            _jobProfileService = jobProfileService;
            //_unitService = unitService;
        }

        public IEnumerable<PayPeriod> PayPeriods { get; set; } = [];
        public IEnumerable<Unit> Units { get; set; } = [];
        public IEnumerable<JobProfile> Users { get; set; } = [];

        public bool ShowReportModal { get; set; }
        public ReportMatrixResult ReportData { get; set; }

        public async Task OnGetAsync()
        {
            ShowReportModal = false;
            await PopulateDropdown();
        }

        public async Task PopulateDropdown()
        {
            PayPeriods = await _payPeriodService.GetPayPeriodsAsync();
            Users = await _jobProfileService.GetJobProfilesAsync();
            //Units = await _unitService.GetUnitsAysnc();
        }

        public async Task<IActionResult> OnGenerateReportAsync(int? payPeriodId, int? userId)
        {
            Console.WriteLine("Post Method called");

            Console.WriteLine($"Pay period id: {payPeriodId}, user id: {userId}");

            await PopulateDropdown();
            return Page();
        }

        public class ReportMatrixResult { public List<ReportRow> Rows { get; set; } }
        public class ReportRow
        {
            public string HourType { get; set; }
            public int[] WeekOneHours { get; set; } 
            public int[] WeekTwoHours { get; set; }
        }
    }
}
