using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.DAL
{
    public static class DIExtensionBankDAL
    {
        public static void AddDbContextBankDAL<TDbContext>(IServiceCollection services, IConfiguration configuration)
            where TDbContext : DbContext
        { 
            string? connectionString = configuration.GetConnectionString("DatabaseConnection");

            if (connectionString == null)
            {
                throw new KeyNotFoundException("Connection string for database was not found");
            }

            bool success = int.TryParse(configuration["DatabaseOptions:PoolSize"], out int connectionPoolSize);

            services.AddDbContextPool<TDbContext>(options =>
            {
                options.UseNpgsql(connectionString);

            }, poolSize: success ? connectionPoolSize : 1024);
        }

        public static void ApplyDALBank<TDbContext>(IServiceProvider serviceProvider)
            where TDbContext : DbContext
        {
            using(var scope = serviceProvider.CreateScope())
            {
                IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                if (bool.TryParse(configuration["DatabaseOptions:NeedMigration"], out bool needMigration) && needMigration)
                {
                    TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                    dbContext.Database.Migrate();
                }
            }
        }
    }
}
