using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using TimeManager.Backend.Data;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Models.Device_Management;
using TimeManager.Backend.ViewModels;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TimeManager.Backend.Services
{
    public interface IKioskService
    {
        Task<IEnumerable<KioskViewModel>> GetKiosksAsync(int? departmentId);
        Task<Kiosk> GetKioskByIdAsync(int id);
        Task CreateKioskAsync(KioskViewModel kvm);
        Task<Kiosk?> UpdateKioskAsync(int id, KioskViewModel kvm);
        Task<int?> DeleteKioskByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetKioskOptionsAsync();
        Task<Kiosk?> ResolveKioskByIpAsync(IPAddress ipAddress);
        string GenerateKisokToken(Kiosk kiosk);
    }

    public class KioskService(HrmsDbContext hrmsDbContext, IConfiguration configuration) : IKioskService
    {
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
            var kiosk = await hrmsDbContext.Kiosk.FindOrThrowAsync(id);
            hrmsDbContext.Kiosk.Remove(kiosk);
            await hrmsDbContext.SaveChangesAsync();
            return id;
        }

        public string GenerateKisokToken(Kiosk kiosk)
        {
            string kioskSecret = configuration["JWT:KioskSecret"] ?? throw new InvalidOperationException("JWT kiosk secret not configured in env");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(kioskSecret));

            var claims = new[]
            {
                new Claim("kiosk_id", kiosk.Id.ToString()),
                new Claim("kiosk_name", kiosk.Name),
                new Claim("department_id", kiosk.DepartmentId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            string issuer = configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured in the env");
            string audience = configuration["JWT:KioskAudience"] ?? throw new InvalidOperationException("JWT kiosk audience is not configured in the env");
            int expiryHours = configuration.GetValue<int>("JWT:KioskExpiryHours", 12);

            var token = new JwtSecurityToken(
                issuer, audience, 
                claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Kiosk> GetKioskByIdAsync(int id)
        {
            return await hrmsDbContext.Kiosk.FindOrThrowAsync(id);
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

        public async Task<IEnumerable<KioskViewModel>> GetKiosksAsync(int? departmentId)
        {

            IEnumerable<KioskViewModel> kiosks = [];
            
             if (departmentId == null)
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
                kiosks = await hrmsDbContext.Kiosk.Where(k => k.DepartmentId == departmentId).Select(k => new KioskViewModel
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

        public async Task<Kiosk?> ResolveKioskByIpAsync(IPAddress ipAddress)
        {
            return await hrmsDbContext.Kiosk.Include(k => k.Department).FirstOrDefaultAsync(k => k.AllowedIPAddress == ipAddress);
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
