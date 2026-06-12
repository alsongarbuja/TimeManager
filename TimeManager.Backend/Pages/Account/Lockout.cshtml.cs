using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeManager.Backend.Models;

namespace TimeManager.Backend.Pages.Account
{
    public class LockoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LockoutModel(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToPage("/Account/Login");
        }
    }
}