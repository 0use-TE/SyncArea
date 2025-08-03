using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncArea.Identity.Models;
using SyncArea.Misc;
using System.Security.Claims;

namespace SyncArea.Pages.Account
{
    public class UserCRUDService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        // 缓存当前用户
        public ApplicationUser? CurrentUser { get; private set; }
        public bool IsAdmin;
        public bool IsSuperAdmin;

        public bool CanManageUsers => IsSuperAdmin || IsAdmin;
        public UserCRUDService(IDbContextFactory<ApplicationDbContext> dbContextFactory, UserManager<ApplicationUser> userManager)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
        }

        // 初始化当前用户（在Circuit打开或首次调用时调用）
        public async Task InitializeAsync(ClaimsPrincipal userPrincipal)
        {
            if (userPrincipal?.Identity?.IsAuthenticated != true)
            {
                CurrentUser = null;
                return;
            }
            // 从UserManager加载用户
            CurrentUser = await _userManager.GetUserAsync(userPrincipal);

            IsAdmin = userPrincipal.FindFirstValue(ClaimTypes.Role) == E_RoleName.Admin.ToString() ? true : false;
            IsSuperAdmin = userPrincipal.FindFirstValue(ClaimTypes.Role) == E_RoleName.SuperAdmin.ToString() ? true : false;
        }

        // 保存修改
        public async Task SaveAsync()
        {
            if (CurrentUser == null) return;

            await using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Users.Update(CurrentUser);
            await dbContext.SaveChangesAsync();
        }
        public async Task<bool> ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                if (CurrentUser == null || CurrentUser.PasswordHash == null)
                    return false;
                var result = await _userManager.ChangePasswordAsync(CurrentUser, oldPassword, newPassword);
                if (result.Succeeded)
                    return true;
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
