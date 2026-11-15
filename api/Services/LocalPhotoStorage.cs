namespace ListingsApi.Services;

public class LocalPhotoStorage(IWebHostEnvironment env) : IPhotoStorage
{
    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(root, "uploads");
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var path = Path.Combine(dir, name);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream, ct);
        return $"/uploads/{name}";
    }
}
