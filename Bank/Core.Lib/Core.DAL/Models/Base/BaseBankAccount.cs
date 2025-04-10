using Bank.DAL;
using Bank.DAL.Enums;
using Bank.DAL.Models;
using Core.DAL.Models.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;

namespace Core.DAL.Models.Base
{
    public abstract class BaseBankAccount : BaseEntity
    {
        public DateTime? CloseDateTime { get; set; }

        [Required]
        public string CurrencyType { get; set; }
        public Guid? CurrencyTypeId { get; set; }
        public CurrencyType Currency { get; set; }

        public decimal Balance { get; set; }
        public BankAccountType BankAccountType { get; set; }
        public bool IsFrozen { get; set; }
        [Required]
        public string AccountName { get; set; }

        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
    }

    public class BaseBankAccountConfiguration : IEntityTypeConfiguration<BaseBankAccount>
    {
        public void Configure(EntityTypeBuilder<BaseBankAccount> builder)
        {
            builder.UseTptMappingStrategy();

            builder.Property(s => s.CurrencyType).HasMaxLength(5).IsFixedLength(false);

            builder.Property(s => s.AccountName).HasMaxLength(200).IsFixedLength(false);

            builder.Property(s => s.Balance).HasConversion(new ValueConverter<decimal, decimal>(
                convertToProviderExpression: (v) => decimal.Round(v, 2),
                convertFromProviderExpression: (v) => decimal.Round(v, 2)
            ));

            builder.HasOne(s => s.Owner)
                   .WithMany(o => o.BankAccounts)
                   .HasForeignKey(s => s.OwnerId)
                   .IsRequired();
        }
    }
}
