using Microsoft.EntityFrameworkCore;
using StorageService.Dto;
using StorageService.StorageDbContextB;
using StorageService.StorageDbContextB.Models;

namespace StorageService.Services
{
    public class ConfigService : IConfigService
    {
        private readonly StorageDbContext _context;

        public ConfigService(StorageDbContext context)
        {
            _context = context;
        }

        public async Task<CreateConfigResponse> CreateConfig(CreateConfigRequest request, Guid userId)
        {
            var configFromDatabase = await _context.Configs.FirstOrDefaultAsync(x => x.UserId == userId && x.Device == request.Device);

            DateTime now = DateTime.UtcNow;
            if(configFromDatabase != null)
            {
                configFromDatabase.ModifyDateTime = now;
                configFromDatabase.Device = request.Device;
                configFromDatabase.Configuration = request.Config;
            }
            else
            {
                Config config = new()
                {
                    Id = Guid.NewGuid(),
                    CreateDateTime = now,
                    UserId = userId,
                    Device = request.Device,
                    Configuration = request.Config
                };
                await _context.Configs.AddAsync(config);
            }

            await _context.SaveChangesAsync();

            return new CreateConfigResponse { Config = request.Config, Device = request.Device, UserId = userId };
        }

        public async Task<GetConfigResponse> GetConfig(GetConfigRequest request, Guid userId)
        {
            var config = await _context.Configs.FirstOrDefaultAsync(x => x.UserId == userId && x.Device == request.Device)
                         ?? throw new KeyNotFoundException($"Config for user id {userId} was not found");

            return new GetConfigResponse { UserId = userId, Device = request.Device, Config = config.Configuration };
        }

        public async Task RemoveConfig(DeleteConfigRequest request, Guid userId)
        {
            var config = await _context.Configs.FirstOrDefaultAsync(x => x.UserId == userId && x.Device == request.Device)
                         ?? throw new KeyNotFoundException($"Config for user id {userId} was not found");

            _context.Configs.Remove(config);
            await _context.SaveChangesAsync();
        }
    }
}
