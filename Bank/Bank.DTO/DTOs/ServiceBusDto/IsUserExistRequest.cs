
namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class IsUserExistRequest: baseMessage
    {
        public Guid UserId { get; set; }
    }
}
