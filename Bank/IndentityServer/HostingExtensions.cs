using Bank;
using Duende.IdentityServer.Services;
using IdentityServer.Interfaces;
using IdentityServer.Services;
using IndentityServer.Db;
using IndentityServer.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IndentityServer
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri("rabbitmq://rabbitmq"), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IProfileService, ProfileService>();

            builder.Services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddProfileService<ProfileService>()
                .AddDeveloperSigningCredential();

            //builder.Services.ConfigureExternalCookie(options => {
            //    options.Cookie.IsEssential = true;
            //    options.Cookie.SameSite = (SameSiteMode)(-1);
            //});

            //builder.Services.ConfigureApplicationCookie(options => {
            //    options.Cookie.IsEssential = true;
            //    options.Cookie.SameSite = (SameSiteMode)(-1);
            //});

            builder.Services.AddAuthentication();

            builder.Services.AddCors(action =>
            {
                action.AddPolicy("IdentityCors", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });


            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                dbContext.Database.Migrate();
            }

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors("IdentityCors");

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapRazorPages().RequireAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
