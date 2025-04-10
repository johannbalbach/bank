using Bank.DAL;
using Core.DAL;
using Microsoft.EntityFrameworkCore;

namespace Core.Extensions
{
    public static class ServicesExtension
    {
        public static void AddDbContextCoreDAL(this IServiceCollection services, IConfiguration configuration)
        {
            DIExtensionBankDAL.AddDbContextBankDAL<CoreDbContext>(services, configuration);
            }

        public static void ApplyDALCore(this WebApplication webApplication)
                {
            DIExtensionBankDAL.ApplyDALBank<CoreDbContext>(webApplication.Services);
        }
    }
}
