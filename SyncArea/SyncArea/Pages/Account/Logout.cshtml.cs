using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncArea.Identity.Models;

namespace SyncArea.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }
        public async Task OnGet()
        {
            await _signInManager.SignOutAsync();
            Response.Redirect("/login");
        }
    }
}
