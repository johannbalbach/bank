using Microsoft.EntityFrameworkCore;
using StorageService.Services;
using StorageService.StorageDbContextB;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var documnetationFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, documnetationFile));
});
builder.Services.AddDbContext<StorageDbContext>(x => x.UseNpgsql("Host=storage_bank_db;Port=5432;Database=storage_bank_db;Username=postgres;Password=1"));
builder.Services.AddScoped<IConfigService, ConfigService>();
builder.Services.AddCors(action =>
{
    action.AddPolicy("StorageCors", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("StorageCors");

app.UseAuthorization();

app.MapControllers();

app.Run();
