using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using Core.DAL.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.MassTransit.Consumers
{
    public class UserCreatedConsumerCore : IConsumer<CreateUserEvent>
    {
        private readonly CoreDbContext _coreDbContext;

        public UserCreatedConsumerCore(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task Consume(ConsumeContext<CreateUserEvent> context)
        {
            var createUserEvent = context.Message;

            bool userExists = await _coreDbContext.Users.AnyAsync(x => x.Id == createUserEvent.Id);

            if (userExists)
            {
                return;
            }

            var User = new User
            {
                Id = createUserEvent.Id,
                FullName = createUserEvent.UserName,
                CreateDateTime = DateTime.UtcNow
            };

            await _coreDbContext.Users.AddAsync(User);

            await _coreDbContext.SaveChangesAsync();
        }
    }
}
