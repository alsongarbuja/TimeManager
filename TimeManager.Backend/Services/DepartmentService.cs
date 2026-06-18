using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentViewModel>> GetDepartmentsAsync();
        Task<Department> GetDepartmentByIdAsync(int id);
        Task CreateDepartmentAsync(DepartmentDto departmentDto);
        Task<Department?> UpdateDepartmentAsync(int id, DepartmentDto departmentDto);
        Task<int?> DeleteDepartmentByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetDepartmentOptionsAsync();
    }

    public class DepartmentService: IDepartmentService
    {
        private readonly HrmsDbContext _context;

        public DepartmentService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task CreateDepartmentAsync(DepartmentDto departmentDto)
        {
            _context.Department.Add(new Department { Name = departmentDto.Name, Description = departmentDto.Description });
            await _context.SaveChangesAsync();
        }

        public async Task<int?> DeleteDepartmentByIdAsync(int id)
        {
            var dept = await _context.Department.FindAsync(id);
            if (dept == null) return null;

            _context.Department.Remove(dept);
            await _context.SaveChangesAsync();

            return id;
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            var data = await _context.Department.FindAsync(id);
            return data;
        }

        public async Task<IEnumerable<SelectListItem>> GetDepartmentOptionsAsync()
        {
            var data = await _context.Department.Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString(),
            }).ToListAsync();
            return data;
        }

        public async Task<IEnumerable<DepartmentViewModel>> GetDepartmentsAsync()
        {
            var data = await _context.Department.Select(d => new DepartmentViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
            }).ToListAsync();
            return data;
        }

        public async Task<Department?> UpdateDepartmentAsync(int id, DepartmentDto departmentDto)
        {
            var dept = await _context.Department.FindAsync(id);
            if (dept == null) return null;

            _context.Entry(dept).CurrentValues.SetValues(departmentDto);
            await _context.SaveChangesAsync();

            return dept;
        }
    }

    public class DepartmentDto {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        public string? Description { get; set; }
    }
}
