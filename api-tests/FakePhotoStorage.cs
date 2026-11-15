using ListingsApi.Services;
using Microsoft.AspNetCore.Http;

namespace ListingsApi.Tests;

// Вторая реализация договора IPhotoStorage: диск не трогает, запоминает имена
public class FakePhotoStorage : IPhotoStorage
{
    public List<string> Saved { get; } = [];

    public Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        Saved.Add(file.FileName);
        return Task.FromResult($"/uploads/{file.FileName}");
    }
}
