using Bank.DAL.Enums;

namespace CreditService.Db.Entities
{
    public class CreditTariff: BaseDeletableEntity
    {
        public string Name { get; set; }
        public double InterestRate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal MinimumPayment { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
