using Microsoft.EntityFrameworkCore;
using StorageService.StorageDbContextB.Models;

namespace StorageService.StorageDbContextB
{
    public class StorageDbContext : DbContext
    {
        public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options)
        {

        }

        public DbSet<Config> Configs { get; set; }
    }
}
