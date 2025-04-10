using Microsoft.EntityFrameworkCore;
using UserService.Db.Entities;

namespace UserService.Db
{
    public class UserDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

    }
}
