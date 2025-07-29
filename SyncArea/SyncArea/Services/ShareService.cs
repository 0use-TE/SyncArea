using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using SyncArea.Identity.Models;

namespace SyncArea.Services
{
    public class ShareService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISnackbar _snackbar;
        private readonly NavigationManager _navigationManager;

        public ShareService(ApplicationDbContext dbContext, ISnackbar snackbar, NavigationManager navigationManager)
        {
            _dbContext = dbContext;
            _snackbar = snackbar;
            _navigationManager = navigationManager;
        }
        public async Task<bool> CreateShareAsync(Guid workspaceId, DateTime expiryDate)
        {
            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                _snackbar.Add("工作区不存在", Severity.Error);
                return false;
            }

            var shareId = Guid.NewGuid();
            var share = new Share
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                Url = shareId.ToString(), // 只存查询字符串
                ExpiryDate = expiryDate,
                IsActive = true
            };

            _dbContext.Shares.Add(share);
            await _dbContext.SaveChangesAsync();
            _snackbar.Add($"分享链接创建成功：{share.Url}", Severity.Success);
            return true;
        }

        public async Task<List<ShareDto>> GetSharesAsync(int page = 1, int pageSize = 100)
        {
            var shares = await _dbContext.Shares
                .Include(s => s.Workspace)
                .OrderBy(s => s.ExpiryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShareDto
                {
                    Id = s.Id,
                    WorkspaceId = s.WorkspaceId,
                    WorkspaceName = s.Workspace.Name,
                    Url = s.Url,
                    ExpiryDate = s.ExpiryDate,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            if (!shares.Any())
            {
                _snackbar.Add("暂无分享链接", Severity.Info);
            }

            return shares;
        }

        public async Task<bool> RevokeShareAsync(Guid shareId)
        {
            var share = await _dbContext.Shares.FindAsync(shareId);
            if (share == null)
            {
                _snackbar.Add("分享链接不存在", Severity.Error);
                return false;
            }

            share.IsActive = false;
            await _dbContext.SaveChangesAsync();
            _snackbar.Add("分享链接已收回", Severity.Success);
            return true;
        }

        public async Task<bool> DeleteShareAsync(Guid shareId)
        {
            var share = await _dbContext.Shares.FindAsync(shareId);
            if (share == null)
            {
                _snackbar.Add("分享链接不存在", Severity.Error);
                return false;
            }

            _dbContext.Shares.Remove(share);
            await _dbContext.SaveChangesAsync();
            _snackbar.Add("分享链接已删除", Severity.Success);
            return true;
        }

        public async Task<bool> UpdateShareExpiryDateAsync(Guid shareId, DateTime expiryDate)
        {
            var share = await _dbContext.Shares.FindAsync(shareId);
            if (share == null)
            {
                _snackbar.Add("分享链接不存在", Severity.Error);
                return false;
            }

            if (expiryDate < DateTime.Today.AddDays(1))
            {
                _snackbar.Add("过期日期必须为未来日期", Severity.Error);
                return false;
            }

            share.ExpiryDate = expiryDate;
            await _dbContext.SaveChangesAsync();
            _snackbar.Add("分享链接过期日期已更新", Severity.Success);
            return true;
        }

        public async Task<ShareDto?> GetShareByUrlAsync(string shareId)
        {
            var share = await _dbContext.Shares
                .Include(s => s.Workspace)
                .FirstOrDefaultAsync(s => s.Url == shareId && s.IsActive && s.ExpiryDate > DateTime.UtcNow);

            if (share == null)
            {
                _snackbar.Add("分享链接无效或已过期", Severity.Error);
                return null;
            }

            return new ShareDto
            {
                Id = share.Id,
                WorkspaceId = share.WorkspaceId,
                WorkspaceName = share.Workspace.Name,
                Url = share.Url,
                ExpiryDate = share.ExpiryDate,
                IsActive = share.IsActive
            };
        }
    }

    public class ShareDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}