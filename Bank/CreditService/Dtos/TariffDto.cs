using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class TariffDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double InterestRate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal MinimumPayment { get; set; }
        public string PaymentType { get; set; }
    }
}
