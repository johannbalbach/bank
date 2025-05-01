using Bank.BL.Configuration;
using Bank.BL.ExceptionHandler;
using Bank.BL.Middlewares;
using Bank.BL.OperationFilters;
using Bank.BL.Options;
using Bank.BL.Redis;
using Bank.BL.Redis.Patterns;
using Bank.BL.Services;
using Bank.DAL.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Bank.BL
{
    public static class DI
    {
        public static void AddOptionsBank(this IServiceCollection services)
        {
            services.ConfigureOptions<TariffOptionsConfigurer>();
        }
        public static void AddAuth(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = "http://158.160.18.15:5004/";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "http://158.160.18.15:5004/",
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Auth failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var token = context.HttpContext.Request.Query["access_token"];
                        var requestPath = context.HttpContext.Request.Path;

                        if(!string.IsNullOrEmpty(token) && requestPath.StartsWithSegments("/core-hub"))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(UserRole.Employee.ToString(), policy => policy.RequireClaim(ClaimTypes.Role, UserRole.Employee.ToString()));
                options.AddPolicy(UserRole.Client.ToString(), policy => policy.RequireClaim(ClaimTypes.Role, UserRole.Client.ToString()));
            });
        }
        public static void AddSwaggerSettings(this IServiceCollection services)
        {
            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressMapClientErrors = true;
            });
            services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Authorize",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.Http
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
                c.OperationFilter<AddIdempotencyHeaderOperationFilter>();
            });
        }
        public static void AddGlobalExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
        }

        public static void AddIdempotencyService(this IServiceCollection services)
        {
            services.AddSingleton<IIdempotencyService, IdempotencyService>();
        }
        public static IApplicationBuilder UseUnstableMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnstableMiddleware>();
        }
        public static IApplicationBuilder UseIdempotencyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IdempotencyMiddleware>();
        }

        public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection redisOptions = configuration.GetSection("RedisOptions");

            services.Configure<RedisConfiguration>(configuration.GetSection(RedisConfiguration.Redis));
            services.AddStackExchangeRedisCache(setupOptions =>
            {
                configuration.GetSection("RedisOptions").Bind(setupOptions);
            });
            services.AddSingleton(provider =>
            {
                var connection = ConnectionMultiplexer.Connect(redisOptions["Configuration"] ?? throw new Exception());
                return connection;
            });
            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.AddScoped<IRedisMessagingService, RedisMessagingService>();
            services.AddScoped<IRedisMessagingFacade, RedisMessagingFacade>();
        }
    }
}
