using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Models.Responses;

namespace TimeManager.Backend.ViewModels
{
    public class PunchViewOverall
    {
        public IEnumerable<SelectListItem> Employees { get; set; } = [];

        public PagedResponse<PunchViewModel> Data { get; set; }
    }

    public class PunchViewModel
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Clock in")]
        public DateTime ClockInTime { get; set; }
            
        [Display(Name = "Clock out")]
        public DateTime? ClockOutTime { get; set; }

        public IEnumerable<SelectListItem> Employees { get; set; } = [];
    }

    public class PunchStatusModel
    {
        public bool IsActive { get; set; }

        public DateTime? TimeStamp { get; set; }

        public DateTime? LastTimeStamp { get; set; }
    }
}
