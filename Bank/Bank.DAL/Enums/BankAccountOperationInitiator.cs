using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum BankAccountOperationInitiator
    {
        [Description("Пользователь")]
        User,
        [Description("Система")]
        System,
        [Description("Другое")]
        Other
    }
}
