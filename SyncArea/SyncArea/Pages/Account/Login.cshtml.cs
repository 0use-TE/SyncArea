using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncArea.Identity.Models;

namespace SyncArea.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty(SupportsGet = true)]
        public string? Username { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Password { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                var result = await _signInManager.PasswordSignInAsync(Username, Password, true, false);
                if (result.Succeeded)
                {
                    // 登录成功，跳转主页或其他页面
                    Response.Redirect("/");
                }
                else
                {
                    // 登录失败逻辑
                    Response.Redirect("/Login");
                }
            }
        }
    }
}
