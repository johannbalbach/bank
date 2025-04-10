using Core.DAL.Models.Base;
using Core.DAL.Models.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DAL.Models.BankAccounts
{
    public class CardBankAccount : BaseBankAccount
    {
        public Guid DebitCardId { get; set; }
        public DebitCard DebitCard { get; set; }
    }

    public class CardBankAccountConfiguration : IEntityTypeConfiguration<CardBankAccount>
    {
        public void Configure(EntityTypeBuilder<CardBankAccount> builder)
        {
        }
    }
}
