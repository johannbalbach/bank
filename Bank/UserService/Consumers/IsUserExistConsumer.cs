using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using UserService.Interfaces;

namespace UserService.Consumers
{
    public class IsUserExistConsumer: IConsumer<IsUserExistRequest>
    {
        private readonly IUserRequestService _userService;
        public IsUserExistConsumer(IUserRequestService userService)
        {
            _userService = userService;
        }

        public async Task Consume(ConsumeContext<IsUserExistRequest> context)
        {
            var req = new IsUserExistCommand();
            if (context.Message.UserId != null)
                req = await _userService.GetIsUserExist(context.Message.UserId);

            await context.RespondAsync(req);
        }
    }
}
