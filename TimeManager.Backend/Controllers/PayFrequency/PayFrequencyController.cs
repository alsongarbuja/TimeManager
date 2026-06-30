using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.PayFrequency
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class PayFrequencyController(IPayFrequencyService payFrequencyService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var payFrequencies = await payFrequencyService.GetPayFrequenciesAsync();
            return View(payFrequencies);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View(new PayFrequencyViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayFrequencyViewModel evm)
        {
            if (!ModelState.IsValid) return View(evm);
            await payFrequencyService.CreatePayFrequencyAsync(new PayFrequencyDto
            {
                Name = evm.Name,
                Description = evm.Description,
            });
            TempData["success"] = "Pay frequency created";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var et = await payFrequencyService.GetPayFrequencyByIdAsync(id);
            if (et == null) return NotFound();
            return View(new PayFrequencyViewModel
            {
                Id = id,
                Name = et.Name,
                Description = et.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PayFrequencyViewModel evm)
        {
            if (!ModelState.IsValid) return View(evm);
            var et = await payFrequencyService.UpdatePayFrequencyAsync(id, new PayFrequencyDto
            {
                Name = evm.Name,
                Description = evm.Description,
            });
            if (et == null)
            {
                TempData["error"] = "Pay frequency not found";
                return View();
            }
            TempData["success"] = "Pay frequency updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await payFrequencyService.DeletePayFrequencyByIdAsync(id);
                TempData["success"] = "Pay frequency deleted";
            } catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
