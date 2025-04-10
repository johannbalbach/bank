using Bank.DAL.Enums;
using Core.DAL.Models.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Core.BL.SignalR
{
    public interface ICoreHub
    {
        public Task OperationCreated(string operation);
        public Task OperationCreated2(string da, string net);
    }

    [Authorize]
    public class CoreSignalRHub : Hub<ICoreHub>
    {
        private static string EmployeeGroup = UserRole.Employee.ToString();

        private readonly ILogger<CoreSignalRHub> _logger;

        public CoreSignalRHub(ILogger<CoreSignalRHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await PerformGroupAction(async () =>
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, EmployeeGroup);
            }, async (userId) =>
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await PerformGroupAction(async () =>
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, EmployeeGroup);
            }, async (userId) =>
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            });

            await base.OnDisconnectedAsync(exception);
        }

        private async Task PerformGroupAction(Func<Task> groupActionEmployee, Func<string, Task> groupActionCommon)
        {
            (var employeeRole, var userId) = GetUserInfo();

            if(userId == null)
            {
                _logger.LogError("User without id was found in hub");
                return;
            }

            await groupActionCommon(userId);

            if (employeeRole != null && employeeRole == UserRole.Employee.ToString())
            {
                await groupActionEmployee();
            }
        }

        private (string?, string?) GetUserInfo()
        {
            var user = Context.User;

            return (user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value, user?.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value);
        }
    }
}
