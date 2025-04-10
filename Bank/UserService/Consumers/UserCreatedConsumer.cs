using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.Db;

namespace UserService.Consumers
{
    public class UserCreatedConsumer : IConsumer<CreateUserEvent>
    {
        private readonly UserDbContext _context;

        public UserCreatedConsumer(UserDbContext coreDbContext)
        {
            _context = coreDbContext;
        }

        public async Task Consume(ConsumeContext<CreateUserEvent> context)
        {
            var createUserEvent = context.Message;

            bool userExists = await _context.Users.AnyAsync(x => x.Id == createUserEvent.Id);

            if (userExists)
            {
                return;
            }

            var User = new Db.Entities.User
            {
                Id = createUserEvent.Id,
                UserName = createUserEvent.UserName,
                Email = createUserEvent.Email,
                IsManuallyBlocked = false,
                Role = createUserEvent.Role,
                CreateDateTime = DateTime.UtcNow,
                Password = ""
            };

            await _context.Users.AddAsync(User);

            await _context.SaveChangesAsync();
        }
    }
}
