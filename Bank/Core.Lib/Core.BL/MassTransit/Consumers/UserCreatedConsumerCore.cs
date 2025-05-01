using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using Core.DAL.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.BL.MassTransit.Consumers
{
    public class UserCreatedConsumerCore : BaseConsumer<CreateUserEvent>
    {
        private readonly CoreDbContext _coreDbContext;

        public UserCreatedConsumerCore(CoreDbContext coreDbContext, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _coreDbContext = coreDbContext;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(CreateUserEvent message)
        {
            var createUserEvent = message;

            bool userExists = await _coreDbContext.Users.AnyAsync(x => x.Id == createUserEvent.Id);

            if (userExists)
            {
                return new NoResponse();
            }

            var User = new User
            {
                Id = createUserEvent.Id,
                FullName = createUserEvent.UserName,
                CreateDateTime = DateTime.UtcNow
            };

            await _coreDbContext.Users.AddAsync(User);

            await _coreDbContext.SaveChangesAsync();

            return new NoResponse();
        }

        protected override async Task SendResponse(ConsumeContext<CreateUserEvent> context, baseResponse response)
        {

        }
    }
}
