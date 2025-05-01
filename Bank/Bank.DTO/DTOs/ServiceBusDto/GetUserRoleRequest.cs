namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class GetUserRoleRequest: baseMessage
    {
        public Guid UserId { get; set; }
    }
}
