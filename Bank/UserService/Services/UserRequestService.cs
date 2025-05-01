using Bank.DTO.DTOs.ServiceBusDto;
using Microsoft.EntityFrameworkCore;
using UserService.Db;
using UserService.Interfaces;

namespace UserService.Services
{
    public class UserRequestService: IUserRequestService
    {
        private readonly UserDbContext _context;
        public UserRequestService(UserDbContext context)
        {
            _context = context;
        }
        public async Task<IsUserBlockedCommand> GetIsUserBlocked(Guid UserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id ==  UserId && !u.DeleteDateTime.HasValue);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден.");

            return new IsUserBlockedCommand { IsBlocked = user.IsManuallyBlocked };
        }
        public async Task<IsUserExistCommand> GetIsUserExist(Guid UserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == UserId && !u.DeleteDateTime.HasValue);

            if (user == null)
                return new IsUserExistCommand { IsExist = false };

            return new IsUserExistCommand { IsExist = true };
        }
        public async Task<GetUserRoleCommand> GetUserRole(Guid UserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == UserId && !u.DeleteDateTime.HasValue);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден.");

            return new GetUserRoleCommand { Role = user.Role};
        }
        public async Task CreateUser(CreateUserEvent user)
        {
            var createUserEvent = user;

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
