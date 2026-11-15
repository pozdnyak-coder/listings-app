namespace ListingsApi.Data.Entities;

public class Listing
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public int Rooms { get; set; }
    public string? Address { get; set; }
    public string? PhotoUrl { get; set; }          // например "/uploads/3f2a….jpg"
    public DateTime CreatedAt { get; set; }

    public int DistrictId { get; set; }            // внешний ключ — столбец в таблице
    public District? District { get; set; }        // навигационное свойство — объект

    public int? OwnerId { get; set; }              // у объявлений из HasData автора нет
    public User? Owner { get; set; }
}
