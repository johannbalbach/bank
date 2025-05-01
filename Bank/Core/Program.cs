using Bank.BL;
using Bank.BL.ExceptionHandler;
using Bank.BL.Redis.Middleware;
using Bank.DAL.Enums;
using Core.BL;
using Core.BL.CQRS.Base;
using Core.BL.CQRS.Queries.GetUserBankAccounts;
using Core.BL.Services.BankAccounts.BankAccountService;
using Core.BL.Services.BankAccounts.CreditBankAccountService;
using Core.BL.Services.Firebase;
using Core.BL.SignalR;
using Core.DAL;
using Core.DTO.DTOs.Responses.Aggregates;
using Core.Extensions;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Authorize",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
            new List<string> ()
        }
    });

    var documnetationFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, documnetationFile));

});
builder.Services.AddAuth();
builder.Services.AddQuartzCore(builder.Configuration);
builder.Services.AddOptionsBank();
builder.Services.AddSignalRCoreService();
builder.Services.AddMassTransit();
builder.Services.AddHttpClient();
//builder.Services.AddDbContextCoreDAL(builder.Configuration);
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.Load("Core.BL")));
builder.Services.AddScoped<ICreditBankAccountService, CreditBankAccountService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddIdempotencyService();
//builder.Services.AddScoped<IRequestHandler<MediatorRequest<GetUserBankAccountsRequest, UserBankAccountsResponseDTO>, UserBankAccountsResponseDTO>, GetUserBankAccountsRequestHandler>();
builder.Services.AddDbContext<CoreDbContext>(options => options.UseNpgsql("Host=core_db;Port=5432;Database=core_db;Username=postgres;Password=1"));
builder.Services.AddCors(action =>
{
    action.AddPolicy("CoreCors", builder =>
    {
        builder.WithOrigins(new string[] {"https://localhost:7207", "http://localhost:5126", "http://localhost:5175", "http://localhost:5173", "http://localhost:5174", "http://158.160.18.15:5174", "http://158.160.18.15:5175", "http://158.160.18.15:5173" }).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(Path.Combine(AppContext.BaseDirectory, "baza-a246a-firebase-adminsdk-fbsvc-be40c905d3.json"))
});

var app = builder.Build();

//app.ApplyDALCore();

app.UseSwagger();
app.UseSwaggerUI();
app.AddCurrencyDefault();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseRedisMessageMiddleware();

app.UseAuthentication();

app.UseCors("CoreCors");

app.UseAuthorization();

app.MapHub<CoreSignalRHub>("core-hub");

app.UseIdempotencyMiddleware();

app.UseUnstableMiddleware();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
