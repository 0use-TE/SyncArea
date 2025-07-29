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
                    // ��¼�ɹ�����ת��ҳ������ҳ��
                    Response.Redirect("/");
                }
                else
                {
                    // ��¼ʧ���߼�
                    Response.Redirect("/Login");
                }
            }
        }
    }
}
