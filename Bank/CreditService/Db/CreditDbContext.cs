using CreditService.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditService.Db
{
    public class CreditDbContext : DbContext
    {
        public DbSet<CreditBankAccount> CreditBankAccounts { get; set; }
        public DbSet<CreditTariff> CreditTariffs { get; set; }
        public DbSet<BankAccountOperationsHistory> OperationsHistory { get; set; }

        public CreditDbContext(DbContextOptions<CreditDbContext> options) : base(options)
        {
        }

    }
}
