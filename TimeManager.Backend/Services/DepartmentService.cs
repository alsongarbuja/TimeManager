using Microsoft.AspNetCore.Mvc;
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
        Task CreateDepartmentAsync(DepartmentDto departmentDto);
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
    }

    public class DepartmentDto {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        public string? Description { get; set; }
    }
}
