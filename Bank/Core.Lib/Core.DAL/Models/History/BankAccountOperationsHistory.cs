using Bank.DAL.Enums;
using Core.DAL.Models.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Core.DAL.Models.History
{
    public class BankAccountOperationsHistory
    {
        public BankAccountOperationType BankAccountOperationType { get; set; }
        public decimal OperatingMoney { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PreviousBalance { get; set; }
        public DateTime OperationDateTime { get; set; }
        public BankAccountOperationInitiator BankAccountOperationInitiator { get; set; }
        public BankAccountOperationStatus BankAccountOperationStatus { get; set; }

        public Guid BankAccountId { get; set; }
        public BaseBankAccount BankAccount { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; } = null!;
    }

    public class BankAccountOperationsHistoryConfiguration : IEntityTypeConfiguration<BankAccountOperationsHistory>
    {
        public void Configure(EntityTypeBuilder<BankAccountOperationsHistory> builder)
        {
            builder.Property(s => s.OperatingMoney).HasConversion(new ValueConverter<decimal, decimal>(
                convertToProviderExpression: (v) => decimal.Round(v, 2),
                convertFromProviderExpression: (v) => decimal.Round(v, 2)
            ));

            builder.Property(s => s.CurrentBalance).HasConversion(new ValueConverter<decimal, decimal>(
                convertToProviderExpression: (v) => decimal.Round(v, 2),
                convertFromProviderExpression: (v) => decimal.Round(v, 2)
            ));

            builder.Property(s => s.PreviousBalance).HasConversion(new ValueConverter<decimal, decimal>(
                convertToProviderExpression: (v) => decimal.Round(v, 2),
                convertFromProviderExpression: (v) => decimal.Round(v, 2)
            ));

            builder.HasKey(s => new { s.BankAccountId, s.UserId, s.OperationDateTime });

            builder.HasOne(s => s.User)
                   .WithMany()
                   .HasForeignKey(s => s.UserId);

            builder.HasOne(s => s.BankAccount)
                   .WithMany()
                   .HasForeignKey(s => s.BankAccountId)
                   .IsRequired();
        }
    }
}
