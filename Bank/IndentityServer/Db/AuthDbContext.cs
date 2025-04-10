using Microsoft.EntityFrameworkCore;

namespace IndentityServer.Db
{
    public class AuthDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
    }
}
