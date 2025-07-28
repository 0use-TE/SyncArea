using System.ComponentModel.DataAnnotations.Schema;

namespace SyncArea.Identity.Models
{
    // 用户-工作区关系表（多对多）
    public class UserWorkspace
    {
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!; // 双向导航

        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public Workspace Workspace { get; set; } = null!; // 双向导航
    }
}
