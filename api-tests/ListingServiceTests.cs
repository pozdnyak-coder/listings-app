using ListingsApi.Contracts;
using ListingsApi.Data;
using ListingsApi.Data.Entities;
using ListingsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ListingsApi.Tests;

// Сервис + настоящий EF Core на SQLite в памяти. xUnit создаёт новый экземпляр класса на каждый тест —
// значит, у каждого теста своя чистая база. Районы 1–8 и 12 объявлений приходят из HasData.
public class ListingServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly FakePhotoStorage _photos = new();
    private readonly ListingService _service;

    public ListingServiceTests()
    {
        _conn = new SqliteConnection("Data Source=:memory:");
        _conn.Open();                                         // база живёт, пока открыто подключение

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();                         // таблицы по модели + данные из HasData

        _service = new ListingService(_db, _photos, NullLogger<ListingService>.Instance);
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    static IFormFile File(string name) => new FormFile(Stream.Null, 0, 100, "file", name);

    // в базе должен существовать пользователь — иначе сработает внешний ключ OwnerId
    private async Task<int> AddUserAsync(string email)
    {
        var user = new User { Email = email, PasswordHash = "x", CreatedAt = DateTime.UtcNow };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task Создатель_становится_автором()
    {
        var owner = await AddUserAsync("o1@test.uz");
        var created = await _service.CreateAsync(new("Квартира у метро", 100, 2, 1, null), owner);

        Assert.NotNull(created);
        Assert.Equal(owner, created.OwnerId);
    }

    [Fact]
    public async Task Несуществующий_район_даёт_null()
    {
        var owner = await AddUserAsync("o2@test.uz");
        var created = await _service.CreateAsync(new("Квартира у метро", 100, 2, 999, null), owner);

        Assert.Null(created);
    }

    [Fact]
    public async Task Чужое_объявление_нельзя_изменить()
    {
        var owner = await AddUserAsync("o3@test.uz");
        var stranger = await AddUserAsync("s3@test.uz");
        var created = await _service.CreateAsync(new("Квартира у метро", 100, 2, 1, null), owner);

        var result = await _service.UpdateAsync(created!.Id, new("Другая квартира", 100, 2, 1, null), stranger);

        Assert.Equal(ChangeResult.Forbidden, result);
    }

    [Fact]
    public async Task Объявление_из_HasData_без_автора_нельзя_удалить()
    {
        var user = await AddUserAsync("o4@test.uz");
        var result = await _service.DeleteAsync(1, user);     // Id = 1 — начальные данные, OwnerId = null

        Assert.Equal(ChangeResult.Forbidden, result);
    }

    [Fact]
    public async Task Вторая_страница_по_3_из_8_по_цене()
    {
        // район 8 («Алмазар») в начальных данных пуст — считаем только свои 8 объявлений
        var owner = await AddUserAsync("o5@test.uz");
        for (var i = 1; i <= 8; i++)
            await _service.CreateAsync(new($"Квартира номер {i}", i * 100, 1, 8, null), owner);

        var page = await _service.GetPageAsync(8, null, page: 2, pageSize: 3, sort: "price_asc");

        Assert.Equal(8, page.Total);
        Assert.Equal(3, page.Items.Count);
        Assert.Equal(400, page.Items[0].Price);                // 4-я по цене — первая на 2-й странице
    }

    [Fact]
    public async Task Фильтр_по_району_возвращает_только_этот_район()
    {
        var owner = await AddUserAsync("o6@test.uz");
        await _service.CreateAsync(new("Квартира в Алмазаре 1", 100, 1, 8, null), owner);
        await _service.CreateAsync(new("Квартира в Алмазаре 2", 200, 1, 8, null), owner);
        await _service.CreateAsync(new("Квартира в Мирабаде", 300, 1, 7, null), owner);

        var page = await _service.GetPageAsync(8, null, page: 1, pageSize: 10, sort: "new");

        Assert.Equal(2, page.Total);
        Assert.All(page.Items, i => Assert.Equal(8, i.DistrictId));
    }

    [Fact]
    public async Task Фильтр_по_максимальной_цене()
    {
        var owner = await AddUserAsync("o7@test.uz");
        await _service.CreateAsync(new("Дешёвая квартира", 100, 1, 8, null), owner);
        await _service.CreateAsync(new("Дорогая квартира", 900, 1, 8, null), owner);

        var page = await _service.GetPageAsync(8, 500, page: 1, pageSize: 10, sort: "new");

        Assert.Single(page.Items);
        Assert.Equal(100, page.Items[0].Price);
    }

    [Fact]
    public async Task PageSize_не_больше_50()
    {
        var page = await _service.GetPageAsync(null, null, page: 1, pageSize: 99999, sort: "new");

        Assert.Equal(50, page.PageSize);
    }

    [Fact]
    public async Task Мои_объявления_только_мои()
    {
        var me = await AddUserAsync("me@test.uz");
        var other = await AddUserAsync("other@test.uz");
        await _service.CreateAsync(new("Моя квартира", 100, 1, 8, null), me);
        await _service.CreateAsync(new("Чужая квартира", 100, 1, 8, null), other);

        var mine = await _service.GetMineAsync(me);

        Assert.Single(mine);
        Assert.Equal("Моя квартира", mine[0].Title);
    }

    [Fact]
    public async Task Фото_автора_сохраняется_через_хранилище()
    {
        var owner = await AddUserAsync("o8@test.uz");
        var created = await _service.CreateAsync(new("Квартира с фото", 1, 1, 1, null), owner);

        var result = await _service.SetPhotoAsync(created!.Id, owner, File("a.jpg"));

        Assert.Equal(ChangeResult.Ok, result.Result);
        Assert.Equal("/uploads/a.jpg", result.Url);
        Assert.Single(_photos.Saved);                         // хранилище вызвано один раз
        Assert.Equal("a.jpg", _photos.Saved[0]);
    }

    [Fact]
    public async Task Фото_чужого_объявления_запрещено_и_не_сохраняется()
    {
        var owner = await AddUserAsync("o9@test.uz");
        var stranger = await AddUserAsync("s9@test.uz");
        var created = await _service.CreateAsync(new("Квартира с фото", 1, 1, 1, null), owner);

        var result = await _service.SetPhotoAsync(created!.Id, stranger, File("a.jpg"));

        Assert.Equal(ChangeResult.Forbidden, result.Result);
        Assert.Empty(_photos.Saved);
    }

    [Fact]
    public async Task Текстовый_файл_даёт_ошибку_и_не_сохраняется()
    {
        var owner = await AddUserAsync("o10@test.uz");
        var created = await _service.CreateAsync(new("Квартира с фото", 1, 1, 1, null), owner);

        var result = await _service.SetPhotoAsync(created!.Id, owner, File("notes.txt"));

        Assert.Equal("Только JPG, PNG или WEBP", result.Error);
        Assert.Empty(_photos.Saved);
    }
}
