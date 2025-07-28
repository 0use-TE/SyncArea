using System.ComponentModel.DataAnnotations.Schema;

namespace SyncArea.Identity.Models
{
    // 日志表
    public class Log
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!; // 双向导航

        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public Workspace Workspace { get; set; } = null!; // 双向导航

        public DateTime ActionTime { get; set; }

        public string Action { get; set; } = string.Empty; // 例如 "JoinWorkspace"
    }
}
