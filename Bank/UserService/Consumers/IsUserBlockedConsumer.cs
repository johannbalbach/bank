using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using UserService.Interfaces;

namespace UserService.Consumers
{
    public class IsUserBlockedConsumer: BaseConsumer<IsUserBlockedRequest>
    {
        private readonly IUserRequestService _userService;
        public IsUserBlockedConsumer(IUserRequestService userService, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _userService = userService;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(IsUserBlockedRequest message)
        {
            return await _userService.GetIsUserBlocked(message.UserId);
        }

        protected override async Task SendResponse(ConsumeContext<IsUserBlockedRequest> context, baseResponse response)
        {
            await context.RespondAsync((IsUserBlockedCommand)response);
        }
    }
}
