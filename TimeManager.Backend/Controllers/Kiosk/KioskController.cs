using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Extensions;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.Kiosk
{
    public class KioskController : Controller
    {
        private readonly IKioskService kioskService;
        private readonly IDepartmentService departmentService;

        public KioskController(IKioskService kioskService, IDepartmentService departmentService)
        {
            this.kioskService = kioskService;
            this.departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            var kiosks = await kioskService.GetKiosksAsync(departmentId);
            return View(kiosks);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new KioskViewModel
            {
                
                Departments = (await departmentService.GetDepartmentOptionsAsync())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KioskViewModel kvm)
        {
            int? departmentId = HttpContext.Session.GetDepartmentId();
            await kioskService.CreateKioskAsync(new KioskViewModel {
                AllowedIPAddress = kvm.AllowedIPAddress,
                DepartmentId = departmentId ?? kvm.DepartmentId,
                Name = kvm.Name,
                Description = kvm.Description,
            });
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var k = await kioskService.GetKioskByIdAsync(id);
            KioskViewModel kvm = new KioskViewModel { 
                Id = k.Id,
                Name = k.Name,
                Description = k.Description,
                AllowedIPAddress = k.AllowedIPAddress,
                Departments = (await departmentService.GetDepartmentOptionsAsync())
            };
            return View(kvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KioskViewModel kvm)
        {
            var k = await kioskService.UpdateKioskAsync(id, kvm);
            if (k == null) { return View(kvm); }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await kioskService.DeleteKioskByIdAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
