using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitViewModel>> GetUnitsAysnc(int? departmentId);
        Task<UnitViewModel> GetUnitByIdAsync(int id);
        Task CreateUnitAsync(UnitViewModel uvm);
        Task<UnitViewModel?> UpdateUnitAsync(int id, UnitViewModel uvm);
        Task<int?> DeleteUnitByIdAsync(int id);

        Task<IEnumerable<SelectListItem>> GetUnitReportOptionsAsync(int? departmentId, int selectedId = 0);
    }

    public class UnitService(
        HrmsDbContext context, 
        ILogger<Unit> logger,
        IDepartmentService departmentService
    ) : IUnitService
    {
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

        public async Task<UnitViewModel> GetUnitByIdAsync(int id)
        {
            Unit unit = await context.Unit.Include(u => u.Department).Where(u => u.Id == id).FirstOrDefaultAsync();
            IEnumerable<SelectListItem> departments = await departmentService.GetDepartmentOptionsAsync(unit.DepartmentId);
            return new UnitViewModel
            {
                Id = unit.Id,
                Name = unit.Name,
                Description = unit.Description,
                DepartmentName = unit.Department.Name,
                DepartmentId = unit.DepartmentId,
                Index = unit.Index,
                Departments = departments
            };
        }

        public async Task CreateUnitAsync(UnitViewModel uvm)
        {
            context.Unit.Add(new Unit
            {
                Name = uvm.Name,
                Description = uvm.Description,
                DepartmentId = (int)uvm.DepartmentId!,
                Index = uvm.Index,
            });
            await context.SaveChangesAsync();
        }

        public async Task<UnitViewModel?> UpdateUnitAsync(int id, UnitViewModel uvm)
        {
            var unit = await context.Unit.FindAsync(id);
            if (unit == null)
            {
                logger.LogWarning($"Unit with id: {id} not found");
                return null;
            }

            context.Entry(unit).CurrentValues.SetValues(uvm);
            await context.SaveChangesAsync();
            IEnumerable<SelectListItem> departments = await departmentService.GetDepartmentOptionsAsync(unit.DepartmentId);

            return new UnitViewModel
            {
                Id = unit.Id,
                Name = unit.Name,
                Description = unit.Description,
                DepartmentId = unit.DepartmentId,
                Index = unit.Index,
                DepartmentName = unit.Department.Name,
                Departments = departments
            };
        }

        public async Task<int?> DeleteUnitByIdAsync(int id)
        {
            var unit = await context.Unit.FindOrThrowAsync(id);
            context.Unit.Remove(unit);
            await context.SaveChangesAsync();
            return id;
        }

        public async Task<IEnumerable<SelectListItem>> GetUnitReportOptionsAsync(int? departmentId, int selectedId = 0)
        {
            IEnumerable<SelectListItem> units = [];
            if (departmentId == null)
            {
                units = await context.Unit.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} ({u.Index}) - {u.Department.Name}",
                    Selected = u.Id == selectedId,
                }).ToListAsync();
            } else
            {
                units = await context.Unit
                    .Where(u => u.DepartmentId == departmentId)
                    .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} ({u.Index})",
                    Selected = u.Id == selectedId,
                }).ToListAsync();
            }
            return units;
        }
    }
}
