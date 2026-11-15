using System.Text;
using System.Text.Json;
using FluentValidation;
using ListingsApi.Auth;
using ListingsApi.Data;
using ListingsApi.Data.Entities;
using ListingsApi.Endpoints;
using ListingsApi.Errors;
using ListingsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// На Render порт приходит в переменной PORT. Локально её нет — работает адрес из launchSettings.json.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------- настройки JWT: проверяются при старте (урок 9) ----------
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration("Jwt")
    .ValidateDataAnnotations()
    .ValidateOnStart();
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

// ---------- база данных ----------
// Database:Provider = Sqlite (по умолчанию, локально) или Postgres (на Render)
var usePostgres = string.Equals(builder.Configuration["Database:Provider"], "Postgres",
                                StringComparison.OrdinalIgnoreCase);
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=listings.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (usePostgres) options.UseNpgsql(ConnectionStrings.NormalizePostgres(connectionString));
    else options.UseSqlite(connectionString);
});

// ---------- CORS: адреса фронтенда берём из настроек ----------
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

// ---------- ошибки, OpenAPI, валидация ----------
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// ключи ошибок валидации — как в JSON: title, а не Title
ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) =>
    member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

// ---------- сервисы ----------
builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<DistrictService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<IPhotoStorage, LocalPhotoStorage>();

// ---------- аутентификация по JWT ----------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;            // claim "sub" остаётся "sub"
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- конвейер: порядок важен ----------
app.UseStaticFiles();          // раздаёт файлы из wwwroot (загруженные фото)
app.UseExceptionHandler();     // необработанное исключение → 500 problem+json
app.UseStatusCodePages();      // пустой 404 → 404 problem+json
app.UseCors();
app.UseAuthentication();       // кто ты (читает токен)
app.UseAuthorization();        // что тебе можно

app.MapOpenApi();              // документ: /openapi/v1.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Listings API v1");
    options.RoutePrefix = "swagger";           // страница: /swagger
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription();   // для Render

app.MapAuthEndpoints();
app.MapListingEndpoints();
app.MapDistrictEndpoints();

// ---------- база: миграции / создание таблиц + начальные данные ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitAsync(db, usePostgres, app.Logger);
}

app.Run();

// нужно интеграционным тестам (WebApplicationFactory<Program>)
public partial class Program { }
