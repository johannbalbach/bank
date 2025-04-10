using Bank.DAL.Enums;
using Bank.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Core.DAL.Models.BankAccounts.Tariff
{
    public class CreditTariff : BaseDeletableEntity
    {
        public string Name { get; set; }
        public double InterestRate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal MinimumPayment { get; set; }
        public PaymentType PaymentType { get; set; }

        public ICollection<CreditBankAccount> CreditBankAccounts { get; set; }
    }

    public class CreditTariffConfiguration : IEntityTypeConfiguration<CreditTariff>
    {
        public void Configure(EntityTypeBuilder<CreditTariff> builder)
        {
            builder.Property(s => s.MinimumPayment).HasConversion(new ValueConverter<decimal, decimal>(
                convertToProviderExpression: (v) => decimal.Round(v, 2),
                convertFromProviderExpression: (v) => decimal.Round(v, 2)
            ));

            builder.Property(s => s.CreditLimit).HasConversion(new ValueConverter<decimal, decimal>(
                convertToProviderExpression: (v) => decimal.Round(v, 2),
                convertFromProviderExpression: (v) => decimal.Round(v, 2)
            ));

            builder.HasMany(x => x.CreditBankAccounts)
                   .WithOne(c => c.Tariff)
                   .HasForeignKey(c => c.TariffId)
                   .IsRequired();
        }
    }
}
