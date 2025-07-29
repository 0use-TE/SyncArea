using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncArea.Identity.Models;
using System.Security.Claims;

namespace SyncArea.Services
{
    public class UserCRUDService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        // 缓存当前用户
        public ApplicationUser? CurrentUser { get; private set; }
        public bool isAdmin { get; private set; }

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
            //判断角色是否为管理员
            isAdmin = userPrincipal.FindFirstValue(ClaimTypes.Role) == "Admin" ? true : false;
        }

        // 保存修改
        public async Task SaveAsync()
        {
            if (CurrentUser == null) return;

            await using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Users.Update(CurrentUser);
            await dbContext.SaveChangesAsync();
        }
    }

}
