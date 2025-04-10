using Bank.DAL.Enums;

namespace CreditService.Dtos
{
    public class CreditBalanceLeftDto
    {
        public Guid Id { get; set; }
        public string CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public decimal DebitedAmount { get; set; }
    }
}
