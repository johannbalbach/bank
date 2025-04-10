using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum PaymentType
    {
        [Description("Ежедневно")]
        Daily,
        [Description("Еженедельно")]
        Weekly,
        [Description("Ежемесячно")]
        Monthly
    }
}
