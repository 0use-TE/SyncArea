using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using SyncArea.Pages.Account;

namespace SyncArea.Identity
{
    public class MyCircuit : CircuitHandler
    {
        private readonly ILogger<MyCircuit> _logger;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly UserCRUDService _userCRUDService;

        public MyCircuit(ILogger<MyCircuit> logger, AuthenticationStateProvider authenticationStateProvider, UserCRUDService userCRUDService)
        {
            _logger = logger;
            _authenticationStateProvider = authenticationStateProvider;
            _userCRUDService = userCRUDService;
        }

        public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            try
            {
                var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
                await _userCRUDService.InitializeAsync(user);
            }
            catch
            {
                _logger.LogError("初始化用户失败，请检查身份验证状态提供程序和用户服务是否正确配置。");
            }

        }
    }
}
