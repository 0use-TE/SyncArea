using Microsoft.EntityFrameworkCore;
using SyncArea.Identity.Models;

namespace SyncArea.Services
{
    public class WorkItemService
    {
        private readonly ApplicationDbContext _dbContext;

        public WorkItemService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkItemDto> CreateWorkItemAsync(string userId, Guid workspaceId, string? remark, DateTime createDate, List<byte[]>? images)
        {
            try
            {
                var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
                if (workspace == null)
                {
                    throw new Exception("工作区不存在");
                }

                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new Exception("用户不存在");
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
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images");
                    Directory.CreateDirectory(imagesPath); // 确保目录存在
                    foreach (var image in images)
                    {
                        var fileName = $"{Guid.NewGuid()}.jpg";
                        var filePath = Path.Combine(imagesPath, fileName);
                        await File.WriteAllBytesAsync(filePath, image);
                        workItem.Photos.Add(new Photo
                        {
                            Id = Guid.NewGuid(),
                            ImageUrl = fileName // 只存文件名，如 "guid.jpg"
                        });
                    }
                }

                _dbContext.WorkItems.Add(workItem);

                foreach (var photo in workItem.Photos)
                {
                    photo.WorkItemId = workItem.Id;
                }
                await _dbContext.SaveChangesAsync();

                // 转换为 WorkItemDto
                var workItemDto = new WorkItemDto
                {
                    Id = workItem.Id,
                    Username = user.UserName ?? "Unknown",
                    Remark = workItem.Remark,
                    Date = workItem.Date,
                    PhotoUrls = workItem.Photos.Select(p => p.ImageUrl ?? string.Empty).ToList(),
                    PhotoCount = workItem.Photos.Count
                };

                return workItemDto;
            }
            catch (Exception ex)
            {
                throw; // 抛出异常，客户端处理
            }
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
                    PhotoUrls = wi.Photos.Select(p => "images/" + p.ImageUrl).ToList(),
                    Name = wi.User != null ? wi.User.Name : "暂未设置",
                })
                .ToListAsync();

            return workItems; // 移除 Snackbar 通知
        }
    }

    public class WorkItemDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string? Remark { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public int PhotoCount { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
    }
}