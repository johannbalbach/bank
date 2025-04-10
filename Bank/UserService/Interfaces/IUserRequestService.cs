using Bank.DTO.DTOs.ServiceBusDto;

namespace UserService.Interfaces
{
    public interface IUserRequestService
    {
        public Task<IsUserBlockedCommand> GetIsUserBlocked(Guid UserId);
        public Task<IsUserExistCommand> GetIsUserExist(Guid UserId);
        public Task<GetUserRoleCommand> GetUserRole(Guid UserId);
    }
}
