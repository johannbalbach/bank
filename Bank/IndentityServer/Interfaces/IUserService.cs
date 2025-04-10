using IdentityServer.Dtos;

namespace IdentityServer.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterUser(UserRegisterDto userDto);
        Task<UserDto> Login(LoginRequestDto loginRequest);
        Task<UserDto> GetUserByIdAsync(Guid id);
    }
}
