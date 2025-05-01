using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using UserService.Interfaces;

namespace UserService.Consumers
{
    public class GetUserRoleConsumer: BaseConsumer<GetUserRoleRequest>
    {
        private readonly IUserRequestService _userService;
        public GetUserRoleConsumer(IUserRequestService userService, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _userService = userService;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(GetUserRoleRequest message)
        {
            return await _userService.GetUserRole(message.UserId);
        }

        protected override async Task SendResponse(ConsumeContext<GetUserRoleRequest> context, baseResponse response)
        {
            await context.RespondAsync((GetUserRoleCommand)response);
        }
    }
}
