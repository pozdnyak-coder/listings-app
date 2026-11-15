namespace ListingsApi.Data;

public static class ConnectionStrings
{
    // Render отдаёт адрес базы в виде postgresql://user:password@host:port/dbname,
    // а Npgsql ждёт строку вида Host=…;Database=…. Эта функция превращает первое во второе.
    // Строку, которая уже в нужном формате, возвращает без изменений.
    public static string NormalizePostgres(string value)
    {
        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return value;

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={uri.Host};Port={port};Database={database};Username={user};Password={password};" +
               "SSL Mode=Prefer;Trust Server Certificate=true";
    }
}
