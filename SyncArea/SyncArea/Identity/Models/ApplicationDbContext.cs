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
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 配置用户-工作区多对多关系
            builder.Entity<UserWorkspace>()
                .HasKey(uw => new { uw.UserId, uw.WorkspaceId });

            builder.Entity<UserWorkspace>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.UserWorkspaces)
                .HasForeignKey(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserWorkspace>()
                .HasOne(uw => uw.Workspace)
                .WithMany(w => w.UserWorkspaces)
                .HasForeignKey(uw => uw.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置工作项-用户关系
            builder.Entity<WorkItem>()
                .HasOne(w => w.User)
                .WithMany(u => u.WorkItems)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 用户删除时不删除工作项

            // 配置工作项-工作区关系
            builder.Entity<WorkItem>()
                .HasOne(w => w.Workspace)
                .WithMany(w => w.WorkItems)
                .HasForeignKey(w => w.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置照片-工作项关系（级联删除）
            builder.Entity<Photo>()
                .HasOne(p => p.WorkItem)
                .WithMany(w => w.Photos)
                .HasForeignKey(p => p.WorkItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置分享-工作区关系
            builder.Entity<Share>()
                .HasOne(s => s.Workspace)
                .WithMany(w => w.Shares)
                .HasForeignKey(s => s.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置日志-用户和工作区关系
            builder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Log>()
                .HasOne(l => l.Workspace)
                .WithMany(w => w.Logs)
                .HasForeignKey(l => l.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 确保房间号唯一
            builder.Entity<Workspace>()
                .HasIndex(w => w.RoomNumber)
                .IsUnique();

            // 种子数据：初始化角色
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "admin-role", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "user-role", Name = "User", NormalizedName = "USER" }
            );
        }
    }
}
