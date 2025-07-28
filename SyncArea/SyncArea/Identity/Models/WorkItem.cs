using System.ComponentModel.DataAnnotations.Schema;

namespace SyncArea.Identity.Models
{
    // 工作项表
    public class WorkItem
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public string? Remark { get; set; }

        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public Workspace Workspace { get; set; } = null!; // 双向导航

        public string UserId { get; set; } = string.Empty; // 创建者

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!; // 双向导航

        // 导航属性：工作项包含的照片
        public List<Photo> Photos { get; set; } = new List<Photo>();
    }
}
