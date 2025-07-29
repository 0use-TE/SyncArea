using Microsoft.AspNetCore.Identity;

namespace SyncArea.Identity.Models
{
    // 用户表（基于 Identity）
    public class ApplicationUser : IdentityUser
    {
        // 导航属性：用户关联的工作区
        public List<UserWorkspace> UserWorkspaces { get; set; } = new List<UserWorkspace>();

        // 导航属性：用户创建的工作项
        public List<WorkItem> WorkItems { get; set; } = new List<WorkItem>();

        // 导航属性：用户相关的日志
        public List<Log> Logs { get; set; } = new List<Log>();
        /// <summary>
        /// 真名
        /// </summary>
        public string? Name { get; set; }
    }
}
