using Core.DAL.Models;
using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.BankAccounts.Tariff;
using Core.DAL.Models.Base;
using Core.DAL.Models.Cards;
using Core.DAL.Models.Currency;
using Core.DAL.Models.History;
using Microsoft.EntityFrameworkCore;

namespace Core.DAL
{
    public class CoreDbContext(DbContextOptions<CoreDbContext> options) : DbContext(options)
    {

        #region Bank accounts

        public DbSet<BaseBankAccount> BankAccounts { get; set; }
        public DbSet<CardBankAccount> CardBankAccounts { get; set; }
        public DbSet<CreditBankAccount> CreditBankAccounts { get; set; }

        #endregion

        public DbSet<CreditTariff> CreditTariffs { get; set; }

        #region Cards

        public DbSet<Card> Cards { get; set; }
        public DbSet<DebitCard> DebitCards { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }

        #endregion

        public DbSet<BankAccountOperationsHistory> BankAccountOperationsHistory { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<CurrencyType> Currencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BaseBankAccountConfiguration());
            modelBuilder.ApplyConfiguration(new CreditBankAccountConfiguration());

            modelBuilder.ApplyConfiguration(new CreditTariffConfiguration());

            modelBuilder.ApplyConfiguration(new CardConfiguration());
            modelBuilder.ApplyConfiguration(new DebitCardConfiguration());
            //modelBuilder.ApplyConfiguration(new CreditCardConfiguration());

            modelBuilder.ApplyConfiguration(new BankAccountOperationsHistoryConfiguration());

            modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        }
    }
}
