using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using UserService.Interfaces;

namespace UserService.Consumers
{
    public class IsUserBlockedConsumer: IConsumer<IsUserBlockedRequest>
    {
        private readonly IUserRequestService _userService;
        public IsUserBlockedConsumer(IUserRequestService userService)
        {
            _userService = userService;
        }

        public async Task Consume(ConsumeContext<IsUserBlockedRequest> context)
        {
            var req = new IsUserBlockedCommand();
            if (context.Message.UserId != null)
                req = await _userService.GetIsUserBlocked(context.Message.UserId);

            await context.RespondAsync(req);
        }
    }
}
