using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncArea.Identity.Models
{
    // 照片表
    public class Photo
    {
        public Guid Id { get; set; }

        [Required]
        public byte[] ImageData { get; set; } = Array.Empty<byte>(); // 二进制图片

        public Guid? WorkItemId { get; set; }

        [ForeignKey("WorkItemId")]
        public WorkItem WorkItem { get; set; } = null!; // 双向导航
    }
}
