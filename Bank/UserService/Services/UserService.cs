using Bank.DAL.Enums;
using Bank.DAL.Exceptions;
using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Db;
using UserService.Db.Entities;
using UserService.Dtos;
using UserService.Dtos.Enums;
using UserService.Interfaces;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;

        public UserService(UserDbContext context, IConfiguration configuration, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<UserDto> GetUserById(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.DeleteDateTime.HasValue);

            if (user == null)
                throw new NotFoundException("Пользователь не найден.");

            return MapToUserDto(user);
        }

        public async Task<UserBrieflyPaginationListDto> GetAllUsers(UserBrieflyPaginationListQueryDto query)
        {
            var usersQuery = _context.Users.AsQueryable();

            if (query.userRole.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.Role == query.userRole.Value);
            }

            if (query.isBlocked.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsManuallyBlocked == query.isBlocked.Value);
            }

            var totalCount = await usersQuery.CountAsync();

            usersQuery = query.sortBy switch
            {
                UserBrieflyListSortByEnum.ByUserNameAsc => usersQuery.OrderBy(u => u.UserName),
                UserBrieflyListSortByEnum.ByUserNameDesc => usersQuery.OrderByDescending(u => u.UserName),
                UserBrieflyListSortByEnum.ByEmailAsc => usersQuery.OrderBy(u => u.Email),
                UserBrieflyListSortByEnum.ByEmailDesc => usersQuery.OrderByDescending(u => u.Email),
                UserBrieflyListSortByEnum.ByRoleAsc => usersQuery.OrderBy(u => u.Role),
                UserBrieflyListSortByEnum.ByRoleDesc => usersQuery.OrderByDescending(u => u.Role),
                UserBrieflyListSortByEnum.ByBlockedAsc => usersQuery.OrderBy(u=>u.IsManuallyBlocked),
                UserBrieflyListSortByEnum.ByBlockedDesc =>  usersQuery.OrderBy(u => u.IsManuallyBlocked),
                _ => usersQuery.OrderBy(u => u.UserName)
            };

            var users = await usersQuery
                    .Skip((query.pageIndex - 1) * query.pageSize)
                    .Take(query.pageSize)
                    .ToListAsync();
                var userDtos = users.Select(u => MapToUserBrieflyDto(u)).ToList();

            return new UserBrieflyPaginationListDto
            {
                users = userDtos,
                totalCount = totalCount,
                pageSize = query.pageSize,
                pageIndex = query.pageIndex
            };
        }

        public async Task<bool> UpdateUser(Guid userId, UserEditDto userDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.DeleteDateTime.HasValue);

            if (user == null)
                throw new NotFoundException("Пользователь не найден.");
            if (user.IsManuallyBlocked)
                throw new InvalidOperationException("Пользователь заблокирован и не может быть изменён");

            if (userDto.UserName != null)
                user.UserName = userDto.UserName;
            if (userDto.BirthDate != null) 
                user.BirthDate = userDto.BirthDate;
            if (userDto.PhoneNumber != null)
            {
                var isPhoneNumberOccupied = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userDto.PhoneNumber && !u.DeleteDateTime.HasValue);
                if (isPhoneNumberOccupied != null) 
                {

                }

                user.PhoneNumber = userDto.PhoneNumber;
            }
               
            if (userDto.Role != null)
                user.Role = (UserRole)userDto.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BlockUser(Guid userId, bool isBlocked)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.DeleteDateTime.HasValue);

            if (user == null)
                throw new NotFoundException("Пользователь не найден.");

            user.IsManuallyBlocked = isBlocked;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UserRole> CheckRole(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.DeleteDateTime.HasValue);

            if (user == null)
                throw new NotFoundException("Пользователь не найден.");

            return user.Role;
        }

        #region

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

        private UserBrieflyDto MapToUserBrieflyDto(User user)
        {
            return new UserBrieflyDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsManuallyBlocked = user.IsManuallyBlocked,
                Role = user.Role
            };
        }
        #endregion
    }
}
