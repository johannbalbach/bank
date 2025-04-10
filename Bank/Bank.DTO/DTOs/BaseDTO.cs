namespace Bank.DTO.DTOs
{
    public class BaseDTO
    {
        public Guid Id { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? ModifyDateTime { get; set; }
    }
}
