using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class CreditAccountCloseDto
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime CloseDate { get; set; }
        public bool IsFrozen { get; set; }
        public string OwnerId { get; set; }
    }
}
