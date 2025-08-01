using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SyncArea.Identity.Models;

namespace SyncArea.Services
{
    public class WorkspaceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISnackbar _snackbar;
        private readonly ImageBuildService _imageBuildService;
        private readonly ILogger<WorkspaceService> _logger;
        public WorkspaceService(ApplicationDbContext dbContext, ISnackbar snackbar, ImageBuildService imageBuildService, ILogger<WorkspaceService> logger)
        {
            _dbContext = dbContext;
            _snackbar = snackbar;
            _imageBuildService = imageBuildService;
            _logger = logger;
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
        /// <summary>
        /// 更新工作区
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="roomNumber"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> UpdateWorkspaceAsync(Guid id, string name, string? roomNumber = null, string? password = null)
        {
            try
            {

                var workspace = await _dbContext.Workspaces.FindAsync(id);
                if (workspace == null)
                {
                    _snackbar.Add("未找到要更新的工作区！", Severity.Error);
                    return false;
                }

                //修改项目名
                if (name != null && name != workspace.Name)
                {
                    var projectName = workspace.Name;
                    var oldDir = _imageBuildService.BuildProjectNameDir(projectName);
                    var newProjectNameDir = _imageBuildService.BuildProjectNameDir(name);
                    Directory.Move(oldDir, newProjectNameDir);
                    workspace.Name = name;
                }
                //修改项目号
                if (roomNumber != null && roomNumber != workspace.RoomNumber)
                {
                    if (await _dbContext.Workspaces.AnyAsync(w => w.RoomNumber == roomNumber && w.Id != id))
                    {
                        _snackbar.Add("项目号已存在，更新失败！", Severity.Error);
                        return false;
                    }
                    var oldDir = _imageBuildService.BuildProjectNumberDir(workspace.Name, workspace.RoomNumber);
                    var newDir = _imageBuildService.BuildProjectNumberDir(workspace.Name, roomNumber);
                    Directory.Move(oldDir, newDir);
                    workspace.RoomNumber = roomNumber;
                }

                if (!string.IsNullOrEmpty(password))
                {
                    workspace.Password = password;
                }

                await _dbContext.SaveChangesAsync();
                _snackbar.Add("工作区更新成功！", Severity.Success);
                return true;
            }
            catch (Exception ex)
            {
                _snackbar.Add("工作区更新失败！", Severity.Error);
                return false;
            }
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
