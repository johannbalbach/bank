using Bank.DAL.Enums;

namespace Core.DTO.DTOs.Responses.BankAccounts.Tariff
{
    public class TariffDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double InterestRate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal MinimumPayment { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
