using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum CardType
    {
        [Description("Дебетовая карта")]
        DebitCard,
        [Description("Кредитная карта")]
        CreditCard
    }
}
