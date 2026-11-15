using ListingsApi.Services;
using Microsoft.AspNetCore.Http;

namespace ListingsApi.Tests;

public class PhotoRulesTests
{
    // поддельный файл нужной длины и имени
    static IFormFile File(string name, long bytes) => new FormFile(Stream.Null, 0, bytes, "file", name);

    [Fact]
    public void Jpg_до_5_МБ_проходит()
    {
        var error = PhotoRules.Check(File("photo.jpg", 1024));   // Act
        Assert.Null(error);                                      // Assert
    }

    [Fact]
    public void Расширение_в_верхнем_регистре_проходит()
        => Assert.Null(PhotoRules.Check(File("PHOTO.JPG", 10)));

    [Fact]
    public void Пустой_файл_отклоняется()
        => Assert.Equal("Файл пустой", PhotoRules.Check(File("a.jpg", 0)));

    [Fact]
    public void Файл_больше_5_МБ_отклоняется()
        => Assert.Equal("Файл больше 5 МБ", PhotoRules.Check(File("a.jpg", 6 * 1024 * 1024)));

    [Theory]                       // один тест — много наборов данных
    [InlineData("notes.txt")]
    [InlineData("archive.zip")]
    [InlineData("script.exe")]
    [InlineData("photo")]          // без расширения
    public void Не_картинка_отклоняется(string name)
        => Assert.Equal("Только JPG, PNG или WEBP", PhotoRules.Check(File(name, 1024)));
}
