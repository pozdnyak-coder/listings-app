using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Data;

public static class DbInitializer
{
    // PostgreSQL + есть папка Migrations  → применяем миграции (как в уроке 3)
    // PostgreSQL, миграций ещё нет        → создаём таблицы по модели (чтобы деплой не падал)
    // SQLite (локально и в тестах)        → создаём таблицы по модели
    public static async Task InitAsync(AppDbContext db, bool postgres, ILogger logger)
    {
        if (postgres && db.Database.GetMigrations().Any())
        {
            logger.LogInformation("Применяю миграции базы данных");
            await db.Database.MigrateAsync();
        }
        else
        {
            logger.LogInformation("Создаю таблицы по модели (EnsureCreated)");
            await db.Database.EnsureCreatedAsync();
        }

        if (postgres) await ResetSequencesAsync(db);
    }

    // Данные из HasData вставлены с готовыми Id, а счётчик Id в PostgreSQL об этом не знает —
    // без этой правки первое же создание объявления упало бы с «duplicate key».
    private static async Task ResetSequencesAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            """SELECT setval(pg_get_serial_sequence('"Districts"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Districts"));""");
        await db.Database.ExecuteSqlRawAsync(
            """SELECT setval(pg_get_serial_sequence('"Listings"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Listings"));""");
    }
}
