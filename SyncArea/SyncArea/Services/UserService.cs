using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SyncArea.Identity.Models;

namespace SyncArea.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISnackbar _snackbar;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ISnackbar snackbar)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _snackbar = snackbar;
        }

        public async Task<bool> JoinWorkspaceAsync(string userId, string roomNumber, string? password)
        {
            var workspace = await _dbContext.Workspaces
                .FirstOrDefaultAsync(w => w.RoomNumber == roomNumber);
            if (workspace == null)
            {
                _snackbar.Add("房间号不存在", Severity.Error);
                return false;
            }

            // 验证密码（明文比较）
            if (!string.IsNullOrEmpty(workspace.Password) && workspace.Password != password)
            {
                _snackbar.Add("密码错误", Severity.Error);
                return false;
            }

            if (await _dbContext.UserWorkspaces.AnyAsync(uw => uw.UserId == userId && uw.WorkspaceId == workspace.Id))
            {
                _snackbar.Add("你已加入该工作区", Severity.Info);
                return false;
            }

            var userWorkspace = new UserWorkspace
            {
                UserId = userId,
                WorkspaceId = workspace.Id
            };
            _dbContext.UserWorkspaces.Add(userWorkspace);

            await _dbContext.SaveChangesAsync();
            _snackbar.Add("成功加入工作区", Severity.Success);
            return true;
        }

        public async Task<bool> CreateUserAsync(string username, string password, List<Guid>? workspaceIds = null)
        {
            var user = new ApplicationUser
            {
                UserName = username,
            };

            // 1. 使用 UserManager 创建用户
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return false;
            }

            // 2. 分配角色
            result = await _userManager.AddToRoleAsync(user, "User");
            if (!result.Succeeded)
            {
                return false;
            }

            // 3. 确保用户记录在 _dbContext 中可用
            // 手动将用户添加到 _dbContext 的跟踪状态
            var trackedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (trackedUser == null)
            {
                _dbContext.Users.Attach(user);
                await _dbContext.SaveChangesAsync();
            }

            // 5. 添加用户到工作区
            if (workspaceIds != null && workspaceIds.Any())
            {
                // 验证工作区 ID 是否存在
                var validWorkspaces = await _dbContext.Workspaces
                    .Where(w => workspaceIds.Contains(w.Id))
                    .Select(w => w.Id)
                    .ToListAsync();

                if (validWorkspaces.Any())
                {
                    foreach (var workspaceId in validWorkspaces)
                    {
                        var userWorkspace = new UserWorkspace
                        {
                            UserId = user.Id,
                            WorkspaceId = workspaceId
                        };
                        _dbContext.UserWorkspaces.Add(userWorkspace);

                    }
                }
            }
            // 6. 保存所有更改
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // 记录详细错误信息以便调试
                Console.WriteLine($"SaveChanges failed: {ex.InnerException?.Message}");
                return false;
            }

            return true;
        }
        public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 100)
        {
            var query = _userManager.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Email = u.Email
                });

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                user.Role = (await _userManager.GetRolesAsync(appUser)).FirstOrDefault() ?? "User";
            }

            return users;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _snackbar.Add("删除失败，用户不存在", Severity.Error);
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {

                await _dbContext.SaveChangesAsync();

                _snackbar.Add($"用户 {user.UserName} 已删除", Severity.Info);
                return true;
            }

            _snackbar.Add($"删除用户失败: {string.Join(";", result.Errors.Select(e => e.Description))}", Severity.Error);
            return false;
        }

        public async Task<List<WorkspaceDto>> GetUserWorkspacesAsync(string userId)
        {
            return await _dbContext.UserWorkspaces
                .Where(uw => uw.UserId == userId)
                .Join(_dbContext.Workspaces,
                    uw => uw.WorkspaceId,
                    w => w.Id,
                    (uw, w) => new WorkspaceDto
                    {
                        Id = w.Id,
                        Name = w.Name,
                        RoomNumber = w.RoomNumber,
                        CreatedAt = w.CreatedAt
                    })
                .ToListAsync();
        }

        public async Task<bool> AddUserToWorkspaceAsync(string userId, Guid workspaceId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _snackbar.Add("添加失败，用户不存在", Severity.Error);
                return false;
            }

            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                _snackbar.Add("添加失败，工作区不存在", Severity.Error);
                return false;
            }

            if (await _dbContext.UserWorkspaces.AnyAsync(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId))
            {
                _snackbar.Add("用户已在此工作区中", Severity.Warning);
                return false;
            }

            var userWorkspace = new UserWorkspace
            {
                UserId = userId,
                WorkspaceId = workspaceId
            };
            _dbContext.UserWorkspaces.Add(userWorkspace);

            await _dbContext.SaveChangesAsync();
            _snackbar.Add($"用户 {user.UserName} 已加入工作区 {workspace.Name}", Severity.Success);
            return true;
        }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
