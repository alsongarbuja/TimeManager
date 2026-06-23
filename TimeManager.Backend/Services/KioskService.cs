using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.Device.Dto;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.Device_Management;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Services
{
    public interface IKioskService
    {
        Task<IEnumerable<KioskViewModel>> GetKiosksAsync();
        Task<Kiosk> GetKioskByIdAsync(int id);
        Task CreateKioskAsync(KioskViewModel kvm);
        Task<Kiosk?> UpdateKioskAsync(int id, KioskViewModel kvm);
        Task<int?> DeleteKioskByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetKioskOptionsAsync();
    }

    public class KioskService : IKioskService
    {
        private readonly HrmsDbContext hrmsDbContext;
        private readonly CurrentEmployeeService currentEmployeeService;

        public KioskService(HrmsDbContext hrmsDbContext, CurrentEmployeeService currentEmployeeService)
        {
            this.hrmsDbContext = hrmsDbContext;
            this.currentEmployeeService = currentEmployeeService;
        }

        public async Task CreateKioskAsync(KioskViewModel kvm)
        {
            hrmsDbContext.Kiosk.Add(new Kiosk
            {
                Name = kvm.Name,
                AllowedIPAddress = kvm.AllowedIPAddress,
                Description = kvm.Description,
                DepartmentId = kvm.DepartmentId,
            });
            await hrmsDbContext.SaveChangesAsync();
        }

        public async Task<int?> DeleteKioskByIdAsync(int id)
        {
            var kiosk = await hrmsDbContext.Kiosk.FindAsync(id);
            if (kiosk == null) return null;
            hrmsDbContext.Kiosk.Remove(kiosk);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public async Task<Kiosk> GetKioskByIdAsync(int id)
        {
            var kiosk = await hrmsDbContext.Kiosk.FindAsync(id);
            return kiosk;
        }

        public async Task<IEnumerable<SelectListItem>> GetKioskOptionsAsync()
        {
            var options = await hrmsDbContext.Kiosk.Select(k => new SelectListItem
            {
                Text = k.Name,
                Value = k.Id.ToString(),
            }).ToListAsync();
            return options;
        }

        public async Task<IEnumerable<KioskViewModel>> GetKiosksAsync()
        {
            var isSuperUser = currentEmployeeService.IsCurrentUserSuperAdmin();

            IEnumerable<KioskViewModel> kiosks = [];
            
             if (isSuperUser)
            {
                kiosks = await hrmsDbContext.Kiosk.Select(k => new KioskViewModel
                {
                    Id = k.Id,
                    Name = k.Name,
                    DepartmentId = k.DepartmentId,
                    AllowedIPAddress = k.AllowedIPAddress,
                    DepartmentName = k.Department.Name,
                }).ToListAsync();
            } else
            {
                int dId = await currentEmployeeService.GetCurrentEmployeeDepartmentIdAsync(null);
                kiosks = await hrmsDbContext.Kiosk.Where(k => k.DepartmentId == dId).Select(k => new KioskViewModel
                {
                    Id = k.Id,
                    Name = k.Name,
                    DepartmentId = k.DepartmentId,
                    AllowedIPAddress = k.AllowedIPAddress,
                    DepartmentName = k.Department.Name,
                }).ToListAsync();
            }

            return kiosks;
        }

        public async Task<Kiosk?> UpdateKioskAsync(int id, KioskViewModel kvm)
        {
            var k = await hrmsDbContext.Kiosk.FindAsync(id);
            if (k == null) return null;

            hrmsDbContext.Entry(k).CurrentValues.SetValues(kvm);
            await hrmsDbContext.SaveChangesAsync();
            return k;
        }
    }
}
