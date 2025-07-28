using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncArea.Identity.Models
{
    // 分享表
    public class Share
    {
        public Guid Id { get; set; }

        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public Workspace Workspace { get; set; } = null!; // 双向导航

        [Required]
        public string Url { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; }
    }
}