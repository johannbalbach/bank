using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DAL.Models.Cards
{
    public class DebitCard : Card
    {
        public Guid CardBankAccountId { get; set; }
        public CardBankAccount CardBankAccount { get; set; }
    }

    public class DebitCardConfiguration : IEntityTypeConfiguration<DebitCard>
    {
        public void Configure(EntityTypeBuilder<DebitCard> builder)
        {
            builder.HasOne(s => s.CardBankAccount)
                   .WithOne(c => c.DebitCard)
                   .HasForeignKey<DebitCard>(s => s.CardBankAccountId)
                   .IsRequired();
        }
    }
}
