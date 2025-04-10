using Bank.DAL.Models;
using Core.DAL.Models.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Core.DAL.Models.Currency
{
    public sealed class CurrencyType : BaseEntity
    {
        /// <summary>
        /// Название
        /// </summary>
        [Required]
        public string Vname { get; set; }
        /// <summary>
        /// Номинал
        /// </summary>
        [Required]
        public int Vnom { get; set; }
        /// <summary>
        /// Курс
        /// </summary>
        [Required]
        public decimal Vcurs { get; set; }
        /// <summary>
        /// ISO Цифровой код валюты
        /// </summary>
        [Required]
        public int Vcode { get; set; }
        /// <summary>
        /// ISO Символьный код валюты
        /// </summary>
        [Required]
        public string VchCode { get; set; }
        /// <summary>
        /// Курс за 1 единицу валюты
        /// </summary>
        [Required]
        public decimal VunitRate { get; set; }

        public List<BaseBankAccount> BankAccounts { get; set; } = null!;
    }

    public class CurrencyConfiguration : IEntityTypeConfiguration<CurrencyType>
    {
        public void Configure(EntityTypeBuilder<CurrencyType> builder)
        {
            builder.HasIndex(x => x.Vcode).IsUnique();
            builder.HasIndex(x => x.Vname);

            builder.HasMany(x => x.BankAccounts)
                   .WithOne(b => b.Currency)
                   .HasForeignKey(x => x.CurrencyTypeId)
                   .IsRequired(false);
        }
    }
}
