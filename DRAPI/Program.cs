using DRBD;
using DRBD.Interfaces;
using DRCore;
using DRCore.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add User Secrets configuration in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddControllers();

// Register dependency injection services
builder.Services.AddScoped<IDBHelper, DBHelper>();
builder.Services.AddScoped<ICFDI, DRCFDI>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
