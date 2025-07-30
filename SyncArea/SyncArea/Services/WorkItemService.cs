using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SyncArea.Identity.Models;

namespace SyncArea.Services
{
    public class WorkItemService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISnackbar _snackbar;

        public WorkItemService(ApplicationDbContext dbContext, ISnackbar snackbar)
        {
            _dbContext = dbContext;
            _snackbar = snackbar;
        }

        public async Task<bool> CreateWorkItemAsync(string userId, Guid workspaceId, string? remark, DateTime createDate, List<byte[]>? images)
        {
            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                _snackbar.Add("工作区不存在", Severity.Error);
                return false;
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                _snackbar.Add("用户不存在", Severity.Error);
                return false;
            }

            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                Date = createDate,
                Remark = remark,
                WorkspaceId = workspaceId,
                UserId = userId,
                Photos = new List<Photo>()
            };

            if (images != null && images.Any())
            {
                foreach (var image in images)
                {
                    // 保存图片到 wwwroot/images
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine("wwwroot/images", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // 确保目录存在
                    await File.WriteAllBytesAsync(filePath, image);
                    workItem.Photos.Add(new Photo
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = $"/images/{fileName}" // 存储相对路径
                    });
                }
            }

            _dbContext.WorkItems.Add(workItem);
            await _dbContext.SaveChangesAsync();

            foreach (var photo in workItem.Photos)
            {
                photo.WorkItemId = workItem.Id;
            }
            await _dbContext.SaveChangesAsync();

            _snackbar.Add("工作项创建成功", Severity.Success);
            return true;
        }

        public async Task<List<WorkItemDto>> GetWorkItemsByWorkspaceAsync(Guid workspaceId)
        {
            var workItems = await _dbContext.WorkItems
                .Where(wi => wi.WorkspaceId == workspaceId)
                .Include(wi => wi.User)
                .Include(wi => wi.Photos)
                .OrderBy(wi => wi.Date)
                .Select(wi => new WorkItemDto
                {
                    Id = wi.Id,
                    Date = wi.Date,
                    Remark = wi.Remark,
                    Username = wi.User != null ? wi.User.UserName : "未知用户",
                    PhotoCount = wi.Photos.Count,
                    PhotoUrls = wi.Photos.Take(5).Select(p => p.ImageUrl).ToList()
                })
                .ToListAsync();

            if (!workItems.Any())
            {
                _snackbar.Add("当前工作区暂无工作项", Severity.Info);
            }

            return workItems;
        }
    }

    public class WorkItemDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string? Remark { get; set; }
        public string Username { get; set; } = string.Empty;
        public int PhotoCount { get; set; }
        public List<string> PhotoUrls { get; set; } = new(); // 使用 URL 替代 Base64
    }
}