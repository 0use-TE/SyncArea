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

        public async Task<bool> CreateWorkItemAsync(string userId, Guid workspaceId, string? remark, List<byte[]>? images)
        {
            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return false;
            }

            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date,
                Remark = remark,
                WorkspaceId = workspaceId,
                UserId = userId,
                Photos = images?.Select(img => new Photo
                {
                    Id = Guid.NewGuid(),
                    ImageData = img,
                    WorkItemId = Guid.NewGuid()
                }).ToList() ?? new List<Photo>()
            };

            _dbContext.WorkItems.Add(workItem);


            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<WorkItemDto>> GetWorkItemsByWorkspaceAsync(Guid workspaceId)
        {
            return await _dbContext.WorkItems
                .Where(wi => wi.WorkspaceId == workspaceId)
                .Include(wi => wi.User)
                .Include(wi => wi.Photos)
                .OrderBy(wi => wi.Date)
                .Select(wi => new WorkItemDto
                {
                    Id = wi.Id,
                    Date = wi.Date,
                    Remark = wi.Remark,
                    Username = wi.User.UserName,
                    PhotoCount = wi.Photos.Count
                })
                .ToListAsync();
        }
    }
    public class WorkItemDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string? Remark { get; set; }
        public string Username { get; set; } = string.Empty;
        public int PhotoCount { get; set; }
    }
}