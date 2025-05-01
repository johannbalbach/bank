using Bank.DAL.Enums;

namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class GetUserRoleCommand: baseResponse
    {
        public UserRole Role { get; set; }
    }
}
