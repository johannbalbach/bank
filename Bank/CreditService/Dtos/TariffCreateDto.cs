using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class TariffCreateDto
    {
        public string Name { get; set; }
        public double InterestRate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal MinimumPayment { get; set; }
        public PaymentType PaymentType { get; set; } = PaymentType.Daily;
    }
}
