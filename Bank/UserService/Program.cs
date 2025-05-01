using Bank.BL;
using Bank.BL.ExceptionHandler;
using Bank.BL.Redis.Middleware;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using UserService.Consumers;
using UserService.Db;
using UserService.Interfaces;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
    x.AddConsumer<IsUserBlockedConsumer>();
    x.AddConsumer<IsUserExistConsumer>();
    x.AddConsumer<UserCreatedConsumer>();
});
builder.Services.AddSwaggerSettings();
builder.Services.AddGlobalExceptionHandler();

builder.Services.AddAuth();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(action =>
{
    action.AddPolicy("UserCors", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddScoped<IUserService, UserService.Services.UserService>();
builder.Services.AddScoped<IUserRequestService, UserRequestService>();
builder.Services.AddIdempotencyService();
builder.Services.AddRedis(builder.Configuration);

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseRedisMessageMiddleware();

app.UseAuthentication();

app.UseCors("UserCors");

app.UseAuthorization();

app.UseIdempotencyMiddleware();

app.UseUnstableMiddleware();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
