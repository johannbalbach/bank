using Bank.DAL.Exceptions;
using Bank.DTO.DTOs.ServiceBusDto;
using BCrypt.Net;
using IdentityServer.Dtos;
using IdentityServer.Interfaces;
using IndentityServer.Db;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Services
{
    public class UserService: IUserService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;

        public UserService(AuthDbContext context, IConfiguration configuration, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<UserDto> RegisterUser(UserRegisterDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && !u.DeleteDateTime.HasValue))
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var user = new User
            {
                UserName = userDto.UserName,
                Patronymic = userDto.UserName.ToLower().Normalize(),
                Email = userDto.Email,
                Password = hashedPassword,
                Role = userDto.Role,
                IsManuallyBlocked = false
            };

            _context.Users.Add(user);

            await _publishEndpoint.Publish(new CreateUserEvent
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Password = user.Password,
                IsManuallyBlocked = false,
            });

            await _context.SaveChangesAsync();

            return MapToUserDto(user);
        }
        public async Task<UserDto> Login(LoginRequestDto loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && !u.DeleteDateTime.HasValue);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                throw new InvalidLoginException("Неверный email или пароль.");

            return MapToUserDto(user);
        }
        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.DeleteDateTime.HasValue);
            return user != null ? MapToUserDto(user) : null;
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Patronymic = user.Patronymic,
                Email = user.Email,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                IsManuallyBlocked = user.IsManuallyBlocked,
                Role = user.Role
            };
        }
    }
}
