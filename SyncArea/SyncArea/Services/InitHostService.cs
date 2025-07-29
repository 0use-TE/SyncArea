
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SyncArea.Identity.Models;
using SyncArea.Models.Options;

namespace SyncArea.Services
{
    public class InitHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<DefaultAdminModel> _defaultAdminOptions;

        public InitHostService(IServiceProvider serviceProvider, IOptions<DefaultAdminModel> defaultAdminOptions)
        {
            _serviceProvider = serviceProvider;
            _defaultAdminOptions = defaultAdminOptions;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var admins = await userManager.GetUsersInRoleAsync(adminRole);
            if (admins.Count == 0)
            {
                var opt = _defaultAdminOptions.Value;

                var user = new ApplicationUser
                {
                    UserName = opt.Account,
                    Name = opt.Name
                };

                var result = await userManager.CreateAsync(user, opt.Password ?? "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, adminRole);
                }
                else
                {
                    var err = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception("创建管理员失败：" + err);
                }
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
