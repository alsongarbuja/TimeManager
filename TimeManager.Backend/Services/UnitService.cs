using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Organization_Management;

namespace TimeManager.Backend.Services
{
    public interface IUnitService
    {
        Task<IEnumerable<Unit>> GetUnitsAysnc();
    }

    public class UnitService: IUnitService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<UnitService> _logger;

        public UnitService(HrmsDbContext context, ILogger<UnitService> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Unit>> GetUnitsAysnc()
        {
            var units = await _context.Unit.ToListAsync();
            return units;
        }
    }
}
