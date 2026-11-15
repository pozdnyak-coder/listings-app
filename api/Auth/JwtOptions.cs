using System.ComponentModel.DataAnnotations;

namespace ListingsApi.Auth;

public class JwtOptions
{
    [Required] public string Issuer { get; init; } = "";
    [Required] public string Audience { get; init; } = "";

    [Required, MinLength(32, ErrorMessage =
        "Jwt:Key должен быть не короче 32 символов: dotnet user-secrets set \"Jwt:Key\" \"...\"")]
    public string Key { get; init; } = "";                 // из user-secrets / переменной Jwt__Key, не из appsettings!

    [Range(1, 24 * 60)] public int ExpiresMinutes { get; init; } = 60;
}
