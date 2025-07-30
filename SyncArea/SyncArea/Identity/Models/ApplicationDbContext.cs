using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SyncArea.Identity.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<UserWorkspace> UserWorkspaces { get; set; }
        public DbSet<Share> Shares { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 用户-工作区 多对多，级联删除
            builder.Entity<UserWorkspace>()
                .HasKey(uw => new { uw.UserId, uw.WorkspaceId });

            builder.Entity<UserWorkspace>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.UserWorkspaces)
                .HasForeignKey(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Cascade); // 用户删了，关联关系也删

            builder.Entity<UserWorkspace>()
                .HasOne(uw => uw.Workspace)
                .WithMany(w => w.UserWorkspaces)
                .HasForeignKey(uw => uw.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade); // 工作区删了，关联关系也删

            // 工作项-用户（用户删了，工作项也删）
            builder.Entity<WorkItem>()
                .HasOne(w => w.User)
                .WithMany(u => u.WorkItems)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 工作项-工作区（工作区删了，工作项也删）
            builder.Entity<WorkItem>()
                .HasOne(w => w.Workspace)
                .WithMany(w => w.WorkItems)
                .HasForeignKey(w => w.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 照片-工作项（工作项删了，保留照片，外键设为 NULL）
            builder.Entity<Photo>()
                .HasOne(p => p.WorkItem)
                .WithMany(w => w.Photos)
                .HasForeignKey(p => p.WorkItemId)
                .OnDelete(DeleteBehavior.SetNull); // 注意外键必须允许 NULL

            // 分享-工作区（工作区删了，分享也删）
            builder.Entity<Share>()
                .HasOne(s => s.Workspace)
                .WithMany(w => w.Shares)
                .HasForeignKey(s => s.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 确保房间号唯一
            builder.Entity<Workspace>()
                .HasIndex(w => w.RoomNumber)
                .IsUnique();

            // 种子数据
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "admin-role", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "user-role", Name = "User", NormalizedName = "USER" }
            );
        }

    }
}
