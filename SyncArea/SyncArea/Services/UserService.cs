using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SyncArea.Identity.Models;
using SyncArea.Misc;

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
                _snackbar.Add("项目号不存在", Severity.Error);
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

        public async Task<bool> CreateUserAsync(string name, string username, string password, E_RoleName role, List<Guid>? workspaceIds = null)
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Name = name
            };
            if (password.Length < 6)
            {
                _snackbar.Add("密码长度不能少于6位", Severity.Error);
                return false;
            }

            // 1. 使用 UserManager 创建用户
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return false;
            }

            // 2. 分配角色
            result = await _userManager.AddToRoleAsync(user, role.ToString());
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
                _snackbar.Add($"创建用户{username}成功!", Severity.Success);
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
                    Username = u.UserName ?? string.Empty,
                    Name = u.Name
                });

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);

                user.Role = appUser != null
                    && Enum.TryParse<E_RoleName>(
                        (await _userManager.GetRolesAsync(appUser)).FirstOrDefault() ?? nameof(E_RoleName.User),
                        ignoreCase: true,
                        out var parsedRole
                    )
                    ? parsedRole
                    : E_RoleName.User;

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
        public async Task<bool> UpdateUserAsync(string userId, string name, string username, E_RoleName roleName, string? password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _snackbar.Add("用户不存在", Severity.Error);
                return false;
            }

            bool hasChanges = false;

            // 更新 Name
            if (user.Name != name)
            {
                user.Name = name;
                hasChanges = true;
            }
            // 更新 Role
            // 获取当前用户的所有角色
            var currentRoles = await _userManager.GetRolesAsync(user);

            // 如果新角色和当前不同
            if (!currentRoles.Contains(roleName.ToString()))
            {
                // 先移除旧角色
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // 再添加新角色
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                hasChanges = true;
            }


            // 更新 UserName
            if (user.UserName != username)
            {
                var existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null && existingUser.Id != userId)
                {
                    _snackbar.Add("用户名已存在", Severity.Error);
                    return false;
                }
                user.UserName = username;
                user.NormalizedUserName = username.ToUpperInvariant(); // 确保 NormalizedUserName 同步更新
                hasChanges = true;
            }

            // 保存用户信息（如果有更改）
            if (hasChanges)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _snackbar.Add($"更新用户信息失败：{string.Join(", ", updateResult.Errors.Select(e => e.Description))}", Severity.Error);
                    return false;
                }
            }

            // 更新密码（如果提供）
            if (!string.IsNullOrEmpty(password))
            {
                // 检查密码长度
                if (password.Length < 6)
                {
                    _snackbar.Add("密码必须至少 6 位", Severity.Error);
                    return false;
                }

                // 验证密码是否符合 Identity 策略
                var passwordValidator = _userManager.PasswordValidators.FirstOrDefault();
                if (passwordValidator != null)
                {
                    var validationResult = await passwordValidator.ValidateAsync(_userManager, user, password);
                    if (!validationResult.Succeeded)
                    {
                        _snackbar.Add($"密码不符合要求：{string.Join(", ", validationResult.Errors.Select(e => e.Description))}", Severity.Error);
                        return false;
                    }
                }

                // 移除旧密码并添加新密码
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    _snackbar.Add($"移除旧密码失败：{string.Join(", ", removeResult.Errors.Select(e => e.Description))}", Severity.Error);
                    return false;
                }

                var addResult = await _userManager.AddPasswordAsync(user, password);
                if (!addResult.Succeeded)
                {
                    _snackbar.Add($"更新密码失败：{string.Join(", ", addResult.Errors.Select(e => e.Description))}", Severity.Error);
                    return false;
                }
            }

            _snackbar.Add("用户信息更新成功", Severity.Success);
            return true;
        }

        public async Task RemoveUserFromWorkSpace(WorkspaceDto workspaceDto, string userId)
        {
            if (workspaceDto == null)
            {
                _snackbar.Add("无效的工作区信息", Severity.Warning);
                return;
            }

            // 找到中间表记录
            var userWorkspace = await _dbContext.UserWorkspaces
                .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WorkspaceId == workspaceDto.Id);

            if (userWorkspace == null)
            {
                _snackbar.Add("该用户不在此工作区", Severity.Info);
                return;
            }

            // 删除关联
            _dbContext.UserWorkspaces.Remove(userWorkspace);
            await _dbContext.SaveChangesAsync();

            _snackbar.Add($"已将用户移出工作区 \"{workspaceDto.Name}\"", Severity.Success);
        }

    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public E_RoleName Role { get; set; } = E_RoleName.User;
    }
}
