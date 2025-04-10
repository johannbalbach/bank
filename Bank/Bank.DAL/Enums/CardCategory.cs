using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum CardCategory
    {
        [Description("Золотая карта")]
        Gold,
        [Description("Платиновая карта")]
        Platinum,
        [Description("Черная карта")]
        Black
    }
}
