using Microsoft.EntityFrameworkCore;
using SyncArea.Identity.Models;
using System.Diagnostics;

namespace SyncArea.Services
{
    public class WorkItemService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ImageBuildService _imageBuildService;
        private readonly ILogger<WorkItemService> _logger;
        public WorkItemService(ApplicationDbContext dbContext, ImageBuildService imageBuildService, ILogger<WorkItemService> logger)
        {
            _dbContext = dbContext;
            _imageBuildService = imageBuildService;
            _logger = logger;
        }

        public async Task<WorkItemDto> CreateWorkItemAsync(string userId, Guid workspaceId, string? remark, DateTime createDate, List<byte[]>? images)
        {
            // 验证工作区和用户
            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId)
                ?? throw new Exception("工作区不存在");

            var user = await _dbContext.Users.FindAsync(userId)
                ?? throw new Exception("用户不存在");

            // 创建工作项
            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                Date = createDate,
                Remark = remark,
                WorkspaceId = workspaceId,
                UserId = userId,
                Photos = new List<Photo>()
            };

            Trace.WriteLine("图片传送到服务端数量: " + images?.Count);
            // 处理图片
            if (images?.Any() == true)
            {
                var imagesPath = _imageBuildService.BuildImagePath(workspace) ?? throw new Exception("图片路径生成失败");
                foreach (var image in images)
                {
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(imagesPath, fileName);
                    Console.WriteLine(filePath);
                    await File.WriteAllBytesAsync(filePath, image);
                    workItem.Photos.Add(new Photo
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = fileName,
                        WorkItemId = workItem.Id
                    });
                }
            }

            // 保存到数据库
            _dbContext.WorkItems.Add(workItem);
            await _dbContext.SaveChangesAsync();

            // 转换为 DTO
            return new WorkItemDto
            {
                Id = workItem.Id,
                Username = user.UserName ?? "Unknown",
                Remark = workItem.Remark,
                Date = workItem.Date,
                PhotoUrls = workItem.Photos.Select(p => p.ImageUrl ?? string.Empty).ToList(),
                PhotoCount = workItem.Photos.Count
            };
        }

        public async Task<List<WorkItemDto>> GetWorkItemsByWorkspaceAsync(Guid workspaceId)
        {
            // 1. 先查出 workspace 实体
            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                return new List<WorkItemDto>();

            // 2. 调用服务构建对外访问的图片基路径
            var imageUrlBase = _imageBuildService.BuildWebImagePath(workspace);

            // 3. 再查询 WorkItems
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
                    Name = wi.User != null ? wi.User.Name : "暂未设置",
                    PhotoCount = wi.Photos.Count,
                    PhotoUrls = wi.Photos.Select(p => imageUrlBase + "/" + p.ImageUrl).ToList(),
                })
                .ToListAsync();

            return workItems;
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
}