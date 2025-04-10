using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class TariffBriefDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double InterestRate { get; set; }
        public string PaymentType { get; set; }
    }
}
