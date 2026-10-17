using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using Syner.Api.Configuration;
using Syner.Api.Data;
using Syner.Api.Data.Seed;
using Syner.Api.Services;
using QuestPDF.Infrastructure;
using Syner.Api.Services.Lotes;
using Syner.Api.Services.Notificaciones;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Syner.Api.Firebase;

const string corsPolicy = "SynerCors";

var builder = WebApplication.CreateBuilder(args);

var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? [];

var firebaseOptions = builder.Configuration
    .GetSection(FirebaseOptions.SectionName)
    .Get<FirebaseOptions>() ?? new FirebaseOptions();

FirebaseApp? firebaseApp = null;

if (!string.IsNullOrWhiteSpace(firebaseOptions.ServiceAccountPath))
{
    var configuredPath = firebaseOptions.ServiceAccountPath.Trim();

var credentialsPath = Path.IsPathRooted(configuredPath)
    ? configuredPath
    : Path.GetFullPath(
        Path.Combine(
            builder.Environment.ContentRootPath,
            configuredPath
        )
    );

    if (!File.Exists(credentialsPath))
    {
        throw new FileNotFoundException(
            "No se encontró la credencial de Firebase configurada.",
            credentialsPath
        );
    }

    firebaseApp = FirebaseApp.Create(
        new AppOptions
        {
            Credential = CredentialFactory
                .FromFile<ServiceAccountCredential>(credentialsPath)
                .ToGoogleCredential()
        }
    );
}

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                EsOrigenLocal(origin) ||
                configuredCorsOrigins.Contains(
                    origin,
                    StringComparer.OrdinalIgnoreCase
                ))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName)
);

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "No se configuró la sección JWT."
    );

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException(
        "No se configuró Jwt:Key."
    );
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)
            ),

            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<LotePdfService>();
builder.Services.AddScoped<LoteQrService>();
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddSingleton(sp => new FirebasePushService(
    firebaseApp,
    sp.GetRequiredService<ILogger<FirebasePushService>>()
));
builder.Services.AddScoped<DispositivoService>();
builder.Services.AddScoped<NotificacionPushService>();
builder.WebHost.UseWebRoot("wwwroot");

var app = builder.Build();

if (firebaseApp is null)
{
    app.Logger.LogWarning(
        "Firebase Push no está configurado. Configure Firebase:ServiceAccountPath fuera del repositorio para habilitarlo."
    );
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(corsPolicy);

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

QuestPDF.Settings.License =
    QuestPDF.Infrastructure.LicenseType.Community;

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SynerDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

    await SynerDbSeeder.SeedAsync(
        db,
        passwordService,
        builder.Configuration,
        app.Environment.IsDevelopment()
    );
}

app.Run();

static bool EsOrigenLocal(string origin)
{
    return origin == "http://localhost" ||
           origin.StartsWith("http://localhost:") ||
           origin == "http://127.0.0.1" ||
           origin.StartsWith("http://127.0.0.1:");
}
