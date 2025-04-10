using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DAL.Models.Cards
{
    public class CreditCard : Card
    {
        public Guid CreditCardId { get; set; }
        public bool IsActive { get; set; }

        public Guid CreditBankAccountId { get; set; }
        public CreditBankAccount CreditBankAccount { get; set; } = null!;
    }

    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            builder.HasAlternateKey(x => x.CreditCardId);
        }
    }
}
