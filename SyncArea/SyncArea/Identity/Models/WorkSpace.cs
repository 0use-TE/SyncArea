using System.ComponentModel.DataAnnotations;

namespace SyncArea.Identity.Models
{
    // 工作区表
    public class Workspace
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty; // 唯一房间号

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // 哈希密码

        // 导航属性：工作区包含的工作项
        public List<WorkItem> WorkItems { get; set; } = new List<WorkItem>();

        // 导航属性：工作区关联的用户
        public List<UserWorkspace> UserWorkspaces { get; set; } = new List<UserWorkspace>();

        // 导航属性：工作区的分享记录
        public List<Share> Shares { get; set; } = new List<Share>();

        // 导航属性：工作区相关的日志
        public List<Log> Logs { get; set; } = new List<Log>();
    }
}
