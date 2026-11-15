namespace ListingsApi.Services;

public interface IPhotoStorage
{
    // сохраняет файл и возвращает адрес вида /uploads/3f2a….jpg
    Task<string> SaveAsync(IFormFile file, CancellationToken ct = default);
}
