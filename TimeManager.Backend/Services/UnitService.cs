using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.Organization.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitViewModel>> GetUnitsAysnc();
        Task<Unit> GetUnitByIdAsync(int id);
        Task CreateUnitAsync(UnitDto departmentDto);
        Task<Unit?> UpdateUnitAsync(int id, UnitDto departmentDto);
        Task<int?> DeleteUnitByIdAsync(int id);

        Task<IEnumerable<SelectListItem>> GetUnitReportOptionsAsync();
    }

    public class UnitService: IUnitService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<UnitService> _logger;

        public UnitService(HrmsDbContext context, ILogger<UnitService> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task CreateUnitAsync(UnitDto unitDto)
        {
            _context.Unit.Add(new Unit
            {
                Name = unitDto.Name,
                Description = unitDto.Description,
                DepartmentId = unitDto.DepartmentId,
            });
            await _context.SaveChangesAsync();
        }

        public async Task<int?> DeleteUnitByIdAsync(int id)
        {
            var unit = await _context.Unit.FindAsync(id);
            if (unit == null) return null;

            _context.Unit.Remove(unit);
            await _context.SaveChangesAsync();

            return id;
        }

        public async Task<Unit> GetUnitByIdAsync(int id)
        {
            var unit = await _context.Unit.Where(u => u.Id == id).Select(u => new Unit { 
                Id = u.Id,
                Name = u.Name,
                Description = u.Description,
                Department = u.Department,
            }).FirstOrDefaultAsync();
            return unit;
        }

        public async Task<IEnumerable<SelectListItem>> GetUnitReportOptionsAsync()
        {
            var units = await _context.Unit.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Name} - {u.Department.Name}",
            }).ToListAsync();
            return units;
        }

        public async Task<IEnumerable<UnitViewModel>> GetUnitsAysnc()
        {
            var units = await _context.Unit.Select(u => new UnitViewModel
            {
                Id = u.Id,
                Name = u.Name,
                DepartmentName = u.Department.Name,
                Description = u.Description,
            }).ToListAsync();
            return units;
        }

        public async Task<Unit?> UpdateUnitAsync(int id, UnitDto unitDto)
        {
            var unit = await _context.Unit.FindAsync(id);
            if (unit == null) return null;

            _context.Entry(unit).CurrentValues.SetValues(unitDto);
            await _context.SaveChangesAsync();

            return unit;
        }
    }
}
