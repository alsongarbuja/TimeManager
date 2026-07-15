using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Superpower.Model;
using TimeManager.Backend.ViewModels;
using U = TimeManager.Backend.Models.AuthManagement.User;

namespace TimeManager.Backend.Controllers
{
    public class SettingController(UserManager<U> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel cpm)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", cpm);
            }

            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found. Please contact administration");
            }

            var isPasswordChanged = await userManager.ChangePasswordAsync(user, cpm.OldPassword, cpm.NewPassword);

            if (isPasswordChanged.Succeeded)
            {
                TempData["success"] = "Successfully updated the password. Please use new password to login from now";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in isPasswordChanged.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    ModelState.AddModelError("OldPassword", "The old password you entered is incorrect.");
                }
                else
                {
                    ModelState.AddModelError("NewPassword", error.Description);
                }
            }

            return View("Index", cpm);
        }
    }
}
