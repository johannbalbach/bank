namespace Bank.DTO.DTOs.ServiceBusDto
{
    public class IsUserBlockedCommand: baseResponse
    {
        public bool IsBlocked { get; set; }
    }
}
