using Core.DAL.Models.BankAccounts.Tariff;
using Core.DAL.Models.Base;
using Core.DAL.Models.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DAL.Models.BankAccounts
{
    public class CreditBankAccount : BaseBankAccount
    {
        public string AccountNumber { get; set; }
        public decimal Debt { get; set; }
        public Guid TariffId { get; set; }
        public CreditTariff Tariff { get; set; }

        public Guid CreditCardId { get; set; }
        public CreditCard CreditCard { get; set; }

        public Guid PayingCardId { get; set; }
        public Card PayingCard { get; set; }
    }

    public class CreditBankAccountConfiguration : IEntityTypeConfiguration<CreditBankAccount>
    {
        public void Configure(EntityTypeBuilder<CreditBankAccount> builder)
        {
            builder.HasIndex(s => s.AccountNumber);

            builder.HasOne(s => s.CreditCard)
                   .WithOne(c => c.CreditBankAccount)
                   .HasForeignKey<CreditBankAccount>(s => s.CreditCardId)
                   .IsRequired(true);

            builder.HasOne(c => c.PayingCard)
                   .WithOne()
                   .HasForeignKey<CreditBankAccount>(s => s.PayingCardId)
                   .IsRequired(true);
        }
    }
}
