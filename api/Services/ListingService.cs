using System.Linq.Expressions;
using ListingsApi.Contracts;
using ListingsApi.Data;
using ListingsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Services;

public enum ChangeResult { Ok, NotFound, Forbidden }

// Result — как прошло; Url — адрес, если Ok; Error — текст, если файл не подошёл
public record PhotoResult(ChangeResult Result, string? Url = null, string? Error = null);

public class ListingService(AppDbContext db, IPhotoStorage photos, ILogger<ListingService> logger)
{
    // Проекция в DTO — одна на все методы чтения
    private static readonly Expression<Func<Listing, ListingResponse>> ToResponse =
        l => new ListingResponse(l.Id, l.Title, l.Price, l.Rooms,
                                 l.DistrictId, l.District!.Name, l.Address, l.CreatedAt,
                                 l.OwnerId, l.PhotoUrl);

    public async Task<PagedResponse<ListingResponse>> GetPageAsync(
        int? districtId, decimal? maxPrice, int page, int pageSize, string sort)
    {
        page = Math.Max(page, 1);                 // не меньше 1
        pageSize = Math.Clamp(pageSize, 1, 50);   // от 1 до 50

        var query = db.Listings.AsNoTracking().AsQueryable();

        // 1. фильтр
        if (districtId is not null) query = query.Where(l => l.DistrictId == districtId);
        if (maxPrice is not null)   query = query.Where(l => l.Price <= maxPrice);

        // 2. сортировка — до Skip/Take! Вторым ключом всегда Id, чтобы страницы не «прыгали»
        query = sort switch
        {
            "price_asc"  => query.OrderBy(l => l.Price).ThenBy(l => l.Id),
            "price_desc" => query.OrderByDescending(l => l.Price).ThenBy(l => l.Id),
            _            => query.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id),
        };

        // 3. сколько всего (до Skip/Take!) + одна страница
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToResponse)
            .ToListAsync();

        return new PagedResponse<ListingResponse>(items, total, page, pageSize);
    }

    // «Мои объявления»
    public async Task<List<ListingResponse>> GetMineAsync(int userId) =>
        await db.Listings.AsNoTracking()
            .Where(l => l.OwnerId == userId)
            .OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id)
            .Select(ToResponse)
            .ToListAsync();

    public async Task<ListingResponse?> GetAsync(int id) =>
        await db.Listings.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(ToResponse)
            .FirstOrDefaultAsync();

    // null = район не найден (endpoint вернёт 400)
    public async Task<ListingResponse?> CreateAsync(CreateListingRequest r, int ownerId)
    {
        if (!await db.Districts.AnyAsync(d => d.Id == r.DistrictId))
            return null;

        var listing = new Listing
        {
            Title = r.Title, Price = r.Price, Rooms = r.Rooms,
            DistrictId = r.DistrictId, Address = r.Address,
            CreatedAt = DateTime.UtcNow, OwnerId = ownerId,
        };
        db.Listings.Add(listing);
        await db.SaveChangesAsync();                 // INSERT; Id заполнен

        return (await GetAsync(listing.Id))!;        // перечитать с именем района
    }

    public async Task<ChangeResult> UpdateAsync(int id, UpdateListingRequest r, int userId)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return ChangeResult.NotFound;
        if (listing.OwnerId != userId) return ChangeResult.Forbidden;

        listing.Title = r.Title;
        listing.Price = r.Price;
        listing.Rooms = r.Rooms;
        listing.DistrictId = r.DistrictId;
        listing.Address = r.Address;
        await db.SaveChangesAsync();
        return ChangeResult.Ok;
    }

    public async Task<ChangeResult> DeleteAsync(int id, int userId)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return ChangeResult.NotFound;
        if (listing.OwnerId != userId) return ChangeResult.Forbidden;

        db.Listings.Remove(listing);
        await db.SaveChangesAsync();
        return ChangeResult.Ok;
    }

    public async Task<PhotoResult> SetPhotoAsync(int id, int userId, IFormFile file)
    {
        // 1. объявление есть и оно моё?
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return new(ChangeResult.NotFound);
        if (listing.OwnerId != userId)
        {
            logger.LogWarning("Пользователь {UserId} пытался сменить фото чужого объявления {ListingId}", userId, id);
            return new(ChangeResult.Forbidden);
        }

        // 2. файл подходит?
        var error = PhotoRules.Check(file);
        if (error is not null) return new(ChangeResult.Ok, Error: error);   // Ok + Error = 400

        // 3. сохранить (куда — решает хранилище) и записать адрес
        var url = await photos.SaveAsync(file);
        listing.PhotoUrl = url;
        await db.SaveChangesAsync();

        logger.LogInformation("Фото {Url} добавлено к объявлению {ListingId}", url, id);
        return new(ChangeResult.Ok, Url: url);
    }
}
