using Bank.BL.Configuration;
using Bank.BL.Redis;
using Bank.BL.Redis.Messages;
using MassTransit.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace LoggingService.Services
{
    public class BackgroundLoggingService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RedisConfiguration _redisConfiguration;

        public BackgroundLoggingService(IServiceScopeFactory serviceScopeFactory, IOptions<RedisConfiguration> redisConfiguration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _redisConfiguration = redisConfiguration.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using(var timer = new PeriodicTimer(TimeSpan.FromSeconds(5)))
            {
                while(await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ProcessRedisQueue();
                }
            }
        }

        private async Task ProcessRedisQueue()
        {
            using(var scope = _serviceScopeFactory.CreateScope())
            {
                IRedisMessagingService redisMessagingService = scope.ServiceProvider.GetRequiredService<IRedisMessagingService>();

                int count = 0;
                while (true)
                {
                    if(count >= _redisConfiguration.BatchSize)
                    {
                        break;
                    }

                    var redisMessage = await redisMessagingService.PopMessageAsync();

                    if(redisMessage != null)
                    {
                        Log.Information(redisMessage);
                    }
                    else
                    {
                        break;
                    }

                    count++;
                }
            }
        }
    }
}
