using Microsoft.AspNetCore.Mvc;
using TimeManager.Backend.Controllers.EmployeeManagement.Dto;
using TimeManager.Backend.Services;
using TimeManager.Backend.ViewModels;

namespace TimeManager.Backend.Controllers.PayFrequency
{
    public class PayFrequencyController : Controller
    {
        private readonly IPayFrequencyService _payFrequencyService;

        public PayFrequencyController(IPayFrequencyService payFrequencyService)
        {
            _payFrequencyService = payFrequencyService;
        }

        public async Task<IActionResult> Index()
        {
            var payFrequencies = await _payFrequencyService.GetPayFrequenciesAsync();
            return View(payFrequencies);
        }

        [HttpGet]
        public async Task<IActionResult> Create() => View(new PayFrequencyViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayFrequencyViewModel evm)
        {
            if (!ModelState.IsValid) return View(evm);
            await _payFrequencyService.CreatePayFrequencyAsync(new PayFrequencyDto
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
            var et = await _payFrequencyService.GetPayFrequencyByIdAsync(id);
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
            var et = await _payFrequencyService.UpdatePayFrequencyAsync(id, new PayFrequencyDto
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
            var et = await _payFrequencyService.DeletePayFrequencyByIdAsync(id);
            if (et == null)
            {
                TempData["error"] = "Pay frequency not found";
            }
            else
            {
                TempData["success"] = "Pay frequency deleted";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
