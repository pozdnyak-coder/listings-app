using ListingsApi.Contracts;
using ListingsApi.Data;
using ListingsApi.Validators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Tests;

// Валидатор принимает AppDbContext (проверка района по базе) — создаём его так же, как для сервиса
public class ValidatorTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly CreateListingRequestValidator _validator;

    public ValidatorTests()
    {
        _conn = new SqliteConnection("Data Source=:memory:");
        _conn.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options);
        _db.Database.EnsureCreated();
        _validator = new CreateListingRequestValidator(_db);
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    [Fact]
    public async Task Верный_запрос_без_ошибок()
    {
        var result = await _validator.ValidateAsync(new CreateListingRequest("Квартира у метро", 100, 2, 1, null));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Пустой_заголовок_даёт_одну_ошибку()
    {
        var result = await _validator.ValidateAsync(new CreateListingRequest("", 100, 2, 1, null));

        // имя свойства — "Title" или "title" (зависит от глобального резолвера, который ставит Program.cs)
        var errors = result.Errors
            .Where(e => string.Equals(e.PropertyName, "title", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.Single(errors);
        Assert.Equal("Укажите заголовок", errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Короткий_заголовок_отклоняется()
    {
        var result = await _validator.ValidateAsync(new CreateListingRequest("Дом", 100, 2, 1, null));
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Не короче 5 символов");
    }

    [Fact]
    public async Task Ноль_комнат_и_11_комнат_отклоняются()
    {
        var zero = await _validator.ValidateAsync(new CreateListingRequest("Квартира у метро", 100, 0, 1, null));
        var eleven = await _validator.ValidateAsync(new CreateListingRequest("Квартира у метро", 100, 11, 1, null));

        Assert.Contains(zero.Errors, e => e.ErrorMessage == "Комнат — от 1 до 10");
        Assert.Contains(eleven.Errors, e => e.ErrorMessage == "Комнат — от 1 до 10");
    }

    [Fact]
    public async Task Несуществующий_район_отклоняется()
    {
        var result = await _validator.ValidateAsync(new CreateListingRequest("Квартира у метро", 100, 2, 999, null));
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Район не найден");
    }
}
