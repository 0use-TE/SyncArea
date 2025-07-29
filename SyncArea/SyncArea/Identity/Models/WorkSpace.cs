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

        public string? Password { get; set; }

        // 导航属性：工作区包含的工作项
        public List<WorkItem> WorkItems { get; set; } = new List<WorkItem>();

        // 导航属性：工作区关联的用户
        public List<UserWorkspace> UserWorkspaces { get; set; } = new List<UserWorkspace>();

        // 导航属性：工作区的分享记录
        public List<Share> Shares { get; set; } = new List<Share>();

    }
}
