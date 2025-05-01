using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using UserService.Interfaces;

namespace UserService.Consumers
{
    public class IsUserExistConsumer : BaseConsumer<IsUserExistRequest>
    {
        private readonly IUserRequestService _userService;

        public IsUserExistConsumer(IUserRequestService userService, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _userService = userService;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(IsUserExistRequest message)
        {
            return await _userService.GetIsUserExist(message.UserId);
        }

        protected override async Task SendResponse(ConsumeContext<IsUserExistRequest> context, baseResponse response)
        {
            await context.RespondAsync((IsUserExistCommand)response);
        }
    }
}
