namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class IsUserBlockedRequest: baseMessage
    {
        public Guid UserId { get; set; }
    }
}
