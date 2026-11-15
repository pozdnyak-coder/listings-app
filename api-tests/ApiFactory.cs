using ListingsApi.Data;
using ListingsApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ListingsApi.Tests;

// Поднимает весь сервер в памяти, но подменяет базу, ключ и хранилище фото
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conn = new("Data Source=:memory:");

    public ApiFactory()
    {
        // У тестов нет user-secrets. Program.cs читает Jwt:Key сразу при старте,
        // поэтому кладём ключ в переменную окружения — её видно с самого начала.
        Environment.SetEnvironmentVariable("Jwt__Key", new string('k', 40));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();     // убрать настоящую базу
            _conn.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conn));

            services.RemoveAll<IPhotoStorage>();
            services.AddSingleton<IPhotoStorage, FakePhotoStorage>();
        });
        // таблицы и начальные данные создаёт сам Program.cs (DbInitializer, провайдер SQLite)
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _conn.Dispose();
    }
}
