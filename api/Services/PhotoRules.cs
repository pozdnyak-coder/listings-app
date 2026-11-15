namespace ListingsApi.Services;

public static class PhotoRules
{
    public const long MaxBytes = 5 * 1024 * 1024;     // 5 МБ
    private static readonly string[] Allowed = [".jpg", ".jpeg", ".png", ".webp"];

    // null — файл подходит; иначе текст ошибки
    public static string? Check(IFormFile file)
    {
        if (file.Length == 0) return "Файл пустой";
        if (file.Length > MaxBytes) return "Файл больше 5 МБ";

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return "Только JPG, PNG или WEBP";

        return null;
    }
}
