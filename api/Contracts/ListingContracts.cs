namespace ListingsApi.Contracts;

// Поля, которые есть и при создании, и при изменении — по ним общий валидатор
public interface IListingFields
{
    string Title { get; }
    decimal Price { get; }
    int Rooms { get; }
    int DistrictId { get; }
    string? Address { get; }
}

public record CreateListingRequest(string Title, decimal Price, int Rooms, int DistrictId, string? Address)
    : IListingFields;

public record UpdateListingRequest(string Title, decimal Price, int Rooms, int DistrictId, string? Address)
    : IListingFields;

public record ListingResponse(
    int Id, string Title, decimal Price, int Rooms,
    int DistrictId, string DistrictName, string? Address, DateTime CreatedAt,
    int? OwnerId, string? PhotoUrl);

public record DistrictResponse(int Id, string Name, int ListingsCount);

// одна страница списка: объявления + сколько их всего
public record PagedResponse<T>(List<T> Items, int Total, int Page, int PageSize);

public record PhotoResponse(string PhotoUrl);
