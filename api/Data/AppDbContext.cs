using ListingsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<District> Districts => Set<District>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // --- Districts ---
        b.Entity<District>(e =>
        {
            e.Property(d => d.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(d => d.Name).IsUnique();
        });

        // --- Users ---
        b.Entity<User>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
        });

        // --- Listings ---
        b.Entity<Listing>(e =>
        {
            e.Property(l => l.Title).HasMaxLength(200).IsRequired();
            e.Property(l => l.Address).HasMaxLength(300);
            e.Property(l => l.PhotoUrl).HasMaxLength(300);

            // SQLite не умеет decimal «по-настоящему» (сортировка и сравнение ломаются),
            // поэтому только для SQLite храним как double. PostgreSQL умеет numeric.
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                e.Property(l => l.Price).HasConversion<double>();

            e.HasOne(l => l.District)
             .WithMany(d => d.Listings)
             .HasForeignKey(l => l.DistrictId)
             .OnDelete(DeleteBehavior.Restrict);      // район с объявлениями удалить нельзя

            e.HasOne(l => l.Owner)
             .WithMany(u => u.Listings)
             .HasForeignKey(l => l.OwnerId)
             .OnDelete(DeleteBehavior.SetNull);       // удалили пользователя — объявление остаётся
        });

        // --- начальные данные: только фиксированные значения! ---
        b.Entity<District>().HasData(
            new District { Id = 1, Name = "Чиланзар" },
            new District { Id = 2, Name = "Юнусабад" },
            new District { Id = 3, Name = "Мирзо-Улугбек" },
            new District { Id = 4, Name = "Яккасарай" },
            new District { Id = 5, Name = "Сергели" },
            new District { Id = 6, Name = "Шайхантахур" },
            new District { Id = 7, Name = "Мирабад" },
            new District { Id = 8, Name = "Алмазар" });

        static DateTime D(int day, int hour) => new(2026, 9, day, hour, 0, 0, DateTimeKind.Utc);

        b.Entity<Listing>().HasData(
            new Listing { Id = 1,  Title = "2-комн. рядом с метро Чиланзар",    Price = 48000, Rooms = 2, DistrictId = 1, Address = "Чиланзар, 12 кв., д. 4", CreatedAt = D(1, 9) },
            new Listing { Id = 2,  Title = "3-комн. с ремонтом",               Price = 71000, Rooms = 3, DistrictId = 2, CreatedAt = D(5, 10) },
            new Listing { Id = 3,  Title = "1-комн. студия",                   Price = 32000, Rooms = 1, DistrictId = 3, CreatedAt = D(8, 14) },
            new Listing { Id = 4,  Title = "4-комн. с видом на парк",          Price = 95000, Rooms = 4, DistrictId = 2, Address = "Юнусабад, 4 кв.", CreatedAt = D(12, 8) },
            new Listing { Id = 5,  Title = "2-комн. после ремонта, тихий двор", Price = 52000, Rooms = 2, DistrictId = 1, CreatedAt = D(15, 11) },
            new Listing { Id = 6,  Title = "1-комн. у ТРЦ",                    Price = 36000, Rooms = 1, DistrictId = 2, CreatedAt = D(18, 16) },
            new Listing { Id = 7,  Title = "3-комн. семейная",                 Price = 66000, Rooms = 3, DistrictId = 3, CreatedAt = D(20, 9) },
            new Listing { Id = 8,  Title = "2-комн. срочно, торг",             Price = 39000, Rooms = 2, DistrictId = 1, CreatedAt = D(24, 12) },
            new Listing { Id = 9,  Title = "2-комн. Яккасарай, центр",         Price = 58000, Rooms = 2, DistrictId = 4, CreatedAt = D(25, 10) },
            new Listing { Id = 10, Title = "1-комн. Сергели, новостройка",     Price = 29000, Rooms = 1, DistrictId = 5, CreatedAt = D(26, 13) },
            new Listing { Id = 11, Title = "3-комн. Шайхантахур у метро",      Price = 74000, Rooms = 3, DistrictId = 6, CreatedAt = D(27, 15) },
            new Listing { Id = 12, Title = "2-комн. Мирабад, евроремонт",      Price = 61000, Rooms = 2, DistrictId = 7, CreatedAt = D(28, 9) });
    }
}
