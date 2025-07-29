using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SyncArea.Identity.Models;

namespace SyncArea.Services
{
    public class WorkspaceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISnackbar _snackbar;

        public WorkspaceService(ApplicationDbContext dbContext, ISnackbar snackbar)
        {
            _dbContext = dbContext;
            _snackbar = snackbar;
        }

        public async Task<bool> CreateWorkspaceAsync(string name, string roomNumber, string password)
        {
            if (await _dbContext.Workspaces.AnyAsync(w => w.RoomNumber == roomNumber))
            {
                _snackbar.Add("房间号已存在，创建失败！", Severity.Error);
                return false;
            }

            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = name,
                RoomNumber = roomNumber,
                Password = password,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Workspaces.Add(workspace);
            await _dbContext.SaveChangesAsync();
            _snackbar.Add("工作区创建成功！", Severity.Success);
            return true;
        }

        public async Task<List<WorkspaceDto>> GetWorkspacesAsync(int page = 1, int pageSize = 100)
        {
            return await _dbContext.Workspaces
                .OrderBy(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new WorkspaceDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    RoomNumber = w.RoomNumber,
                    CreatedAt = w.CreatedAt,
                    Password = w.Password
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateWorkspaceAsync(Guid id, string name, string? roomNumber = null, string? password = null)
        {
            var workspace = await _dbContext.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                _snackbar.Add("未找到要更新的工作区！", Severity.Error);
                return false;
            }

            if (roomNumber != null && roomNumber != workspace.RoomNumber)
            {
                if (await _dbContext.Workspaces.AnyAsync(w => w.RoomNumber == roomNumber && w.Id != id))
                {
                    _snackbar.Add("房间号已存在，更新失败！", Severity.Error);
                    return false;
                }
                workspace.RoomNumber = roomNumber;
            }

            workspace.Name = name;
            if (!string.IsNullOrEmpty(password))
            {
                workspace.Password = password;
            }

            await _dbContext.SaveChangesAsync();
            _snackbar.Add("工作区更新成功！", Severity.Success);
            return true;
        }

        public async Task<bool> DeleteWorkspaceAsync(Guid id)
        {
            var workspace = await _dbContext.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                _snackbar.Add("未找到要删除的工作区！", Severity.Error);
                return false;
            }

            _dbContext.Workspaces.Remove(workspace);
            await _dbContext.SaveChangesAsync();
            _snackbar.Add("工作区删除成功！", Severity.Success);
            return true;
        }
    }

    public class WorkspaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string? Password { get; set; } // 可选密码，可能用于显示或验证
        public DateTime CreatedAt { get; set; }
    }
}
