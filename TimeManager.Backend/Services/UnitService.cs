using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitViewModel>> GetUnitsAysnc(int? departmentId);
        Task<Unit> GetUnitByIdAsync(int id);
        Task CreateUnitAsync(UnitDto departmentDto);
        Task<Unit?> UpdateUnitAsync(int id, UnitDto departmentDto);
        Task<int?> DeleteUnitByIdAsync(int id);

        Task<IEnumerable<SelectListItem>> GetUnitReportOptionsAsync(int? departmentId, int selectedId = 0);
    }

    public class UnitService(HrmsDbContext context, ILogger<Unit> logger) : IUnitService
    {
        public async Task CreateUnitAsync(UnitDto unitDto)
        {
            context.Unit.Add(new Unit
            {
                Name = unitDto.Name,
                Description = unitDto.Description,
                DepartmentId = unitDto.DepartmentId,
                Index = unitDto.Index,
            });
            await context.SaveChangesAsync();
        }

        public async Task<int?> DeleteUnitByIdAsync(int id)
        {
            var unit = await context.Unit.FindOrThrowAsync(id);
            context.Unit.Remove(unit);
            await context.SaveChangesAsync();
            return id;
        }

        public async Task<Unit> GetUnitByIdAsync(int id)
        {
            return await context.Unit.Include(u => u.Department).Where(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SelectListItem>> GetUnitReportOptionsAsync(int? departmentId, int selectedId = 0)
        {
            IEnumerable<SelectListItem> units = [];
            if (departmentId == null)
            {
                units = await context.Unit.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} - {u.Department.Name}",
                    Selected = u.Id == selectedId,
                }).ToListAsync();
            } else
            {
                units = await context.Unit
                    .Where(u => u.DepartmentId == departmentId)
                    .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name}",
                    Selected = u.Id == selectedId,
                }).ToListAsync();
            }
            return units;
        }

        public async Task<IEnumerable<UnitViewModel>> GetUnitsAysnc(int? departmentId)
        {
            var units = await context.Unit.Select(u => new UnitViewModel
            {
                Id = u.Id,
                Name = u.Name,
                DepartmentName = u.Department.Name,
                Description = u.Description,
                Index = u.Index,
                DepartmentId = u.DepartmentId,
            }).ToListAsync();

            if (departmentId != null)
            {
                units = [.. units.Where(u => u.DepartmentId == (int)departmentId)];
            }
            return units;
        }

        public async Task<Unit?> UpdateUnitAsync(int id, UnitDto unitDto)
        {
            var unit = await context.Unit.FindAsync(id);
            if (unit == null)
            {
                logger.LogWarning($"Unit with id: {id} not found");
                return null;
            }

            context.Entry(unit).CurrentValues.SetValues(unitDto);
            await context.SaveChangesAsync();

            return unit;
        }
    }

    public class UnitDto
    {
        [Required(ErrorMessage = "Department Id is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Index is required")]
        public int Index { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot be longer than 100")]
        public string? Description { get; set; }
    }
}
