using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum UserRole
    {
        [Description("Сотрудник")]
        Employee,
        [Description("Клиент")]
        Client
    }
}