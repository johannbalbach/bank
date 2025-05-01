using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using UserService.Db;
using UserService.Interfaces;

namespace UserService.Consumers
{
    public class UserCreatedConsumer : BaseConsumer<CreateUserEvent>
    {
        private readonly IUserRequestService _userService;

        public UserCreatedConsumer(IUserRequestService userService, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
        : base(idempotencyService, serviceProvider)
        {
            _userService = userService;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(CreateUserEvent message)
        {
            if (message != null)
            {
                await _userService.CreateUser(message);

                return new NoResponse();
            }
            return new NoResponse();
        }

        protected override async Task SendResponse(ConsumeContext<CreateUserEvent> context, baseResponse response)
        {

        }
    }
}
