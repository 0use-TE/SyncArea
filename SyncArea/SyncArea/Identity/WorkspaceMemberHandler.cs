using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SyncArea.Identity.Models;
using SyncArea.Misc;
using System.Security.Claims;

namespace SyncArea.Identity
{
    public class WorkspaceMemberHandler : AuthorizationHandler<WorkspaceMemberRequirement>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkspaceMemberHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory, IHttpContextAccessor httpContextAccessor)
        {
            _dbContextFactory = dbContextFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            WorkspaceMemberRequirement requirement)
        {
            // 获取 HttpContext
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return;
            }

            // 检查用户是否具有 Admin 或 SuperAdmin 角色
            var isAdmin = context.User.HasClaim(ClaimTypes.Role, E_RoleName.Admin.ToString());
            var isSuperAdmin = context.User.HasClaim(ClaimTypes.Role, E_RoleName.SuperAdmin.ToString());
            if (isAdmin || isSuperAdmin)
            {
                context.Succeed(requirement);
                return;
            }

            // 获取路由中的 WorkspaceId
            var workspaceIdString = httpContext.Request.RouteValues["WorkspaceId"]?.ToString();
            if (!Guid.TryParse(workspaceIdString, out var workspaceId))
            {
                context.Fail();
                return;
            }

            // 获取用户 ID
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier); // 使用 NameIdentifier 而不是 Name
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            // 检查用户是否在工作区中
            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync();
                var isMember = await db.UserWorkspaces
                    .AnyAsync(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId);

                if (isMember)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            catch
            {
                context.Fail();
            }
        }
    }
}