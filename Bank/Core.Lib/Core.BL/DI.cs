using Core.BL.Jobs;
using Core.DAL;
using Core.DAL.Migrations;
using Core.DAL.Models.Currency;
using Core.DAL.Options;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Reflection;

namespace Core.BL
{
    public static class DI
    {
        public static void AddSignalRCoreService(this IServiceCollection services)
        {
            services.AddSignalR();
        }

        public static void AddMediatr(this IServiceCollection services)
        {
            services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        }

        public static void AddMassTransit(this IServiceCollection services)
        {
            services.AddMassTransit(configurator =>
            {
                configurator.SetKebabCaseEndpointNameFormatter();

                configurator.AddConsumers(Assembly.GetExecutingAssembly());

                configurator.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", cred =>
                    {
                        cred.Username("guest");
                        cred.Password("guest");
                    });

                    cfg.ConfigureEndpoints(ctx);
                });
            });
        }

        public static void AddQuartzCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureOptions<CoreJobsConfigurer>();
            services.Configure<CurrencyOptions>(configuration.GetSection("CurrencyOptions"));

            DateTime now = DateTime.UtcNow.AddSeconds(10);

            services.AddQuartz(config =>
            {
                config.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = JobKey.Create(nameof(CreditPaymentJob));
                config.AddJob<CreditPaymentJob>(jobKey).AddTrigger(trigger =>
                {
                    trigger.ForJob(jobKey).StartAt(now).WithSimpleSchedule(action =>
                    {
                        action.WithIntervalInMinutes(60).RepeatForever();
                    });
                });

                config.AddJob<CurrencyJob>(opt => opt.WithIdentity(nameof(CurrencyJob)));
                config.AddTrigger(opts => opts
                    .ForJob(nameof(CurrencyJob))
                    .WithIdentity($"{nameof(CurrencyJob)}_Trigger")
                    .StartAt(now.AddSeconds(3))
                    .WithSimpleSchedule(opts => opts
                        .WithIntervalInSeconds(60).RepeatForever()));
            });

            services.AddQuartzHostedService(action =>
            {
                action.WaitForJobsToComplete = true;
            });



        }

        public static void AddCurrencyDefault(this WebApplication application)
        {
            using var scope = application.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();

            var rubleCurrency = new DAL.Models.Currency.CurrencyType
            {
                Id = Guid.Parse("05438e40-5990-47ca-8a69-1e49feefac57"),
                Vname = "Российский рубль",
                Vnom = 1,
                Vcurs = 1,
                VchCode = "RUB",
                VunitRate = 1,
                CreateDateTime = DateTime.UtcNow
            };

            var rubbleCurrencyInDb = db.Currencies.FirstOrDefault(x => x.VchCode == "RUB");
            if(rubbleCurrencyInDb == null)
            {
                db.Currencies.Add(rubleCurrency);
                db.SaveChanges();
            }
        }
    }
}
