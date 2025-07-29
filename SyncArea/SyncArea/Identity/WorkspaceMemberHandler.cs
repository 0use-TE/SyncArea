using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SyncArea.Identity.Models;

namespace SyncArea.Identity
{
    public class WorkspaceMemberHandler : AuthorizationHandler<WorkspaceMemberRequirement>
    {
        private readonly ApplicationDbContext _dbContext;

        public WorkspaceMemberHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, WorkspaceMemberRequirement requirement)
        {
            // 获取路由中的 WorkspaceId
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                var workspaceIdString = mvcContext.RouteData.Values["WorkspaceId"]?.ToString();
                if (Guid.TryParse(workspaceIdString, out var workspaceId))
                {
                    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (userId != null)
                    {
                        // 检查用户是否在工作区中
                        var isMember = await _dbContext.UserWorkspaces
                            .AnyAsync(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId);
                        if (isMember)
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }
                }
            }
            context.Fail();
        }
    }
}