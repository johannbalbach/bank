using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using UserService.Interfaces;

namespace UserService.Consumers
{
    public class GetUserRoleConsumer: IConsumer<GetUserRoleRequest>
    {
        private readonly IUserRequestService _userService;
        public GetUserRoleConsumer(IUserRequestService userService)
        {
            _userService = userService;
        }

        public async Task Consume(ConsumeContext<GetUserRoleRequest> context)
        {
            var req = new GetUserRoleCommand();
            if (context.Message.UserId != null)
                req = await _userService.GetUserRole(context.Message.UserId);

            await context.RespondAsync(req);
        }
    }
}
