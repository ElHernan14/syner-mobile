using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Seed;

const string corsPolicy = "SynerCors";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                origin == "http://localhost" ||
                origin.StartsWith("http://localhost:") ||
                origin == "http://127.0.0.1" ||
                origin.StartsWith("http://127.0.0.1:"))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "No se configuró la cadena de conexión 'Default'."
    );

builder.Services.AddDbContext<SynerDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(corsPolicy);

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SynerDbContext>();

    await SynerDbSeeder.SeedAsync(db);
}

app.Run();