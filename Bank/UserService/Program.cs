using Bank.BL;
using Bank.BL.ExceptionHandler;
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

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseCors("UserCors");

app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
