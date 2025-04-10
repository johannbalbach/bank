using Bank.DAL.Enums;
using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;

namespace UserService.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterUser(UserRegisterDto userDto);
        Task<TokenResponseDto> Login(LoginRequestDto loginRequest);
        Task<UserDto> GetUserById(Guid userId);
        Task<UserBrieflyPaginationListDto> GetAllUsers(UserBrieflyPaginationListQueryDto query);
        Task<bool> UpdateUser(Guid userId, UserEditDto userDto);
        Task<bool> BlockUser(Guid userId, bool isBlocked);
       
        Task<UserRole> CheckRole(Guid userId);
    }
}
