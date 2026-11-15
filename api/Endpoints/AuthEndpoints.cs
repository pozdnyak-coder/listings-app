using System.Security.Claims;
using ListingsApi.Auth;
using ListingsApi.Contracts;
using ListingsApi.Services;
using ListingsApi.Validators;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ListingsApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest r, AuthService auth) =>
            TypedResults.Ok(await auth.RegisterAsync(r)))
        .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
        .ProducesValidationProblem()
        .WithSummary("Регистрация: сразу возвращает токен");

        group.MapPost("/login", async Task<Results<Ok<AuthResponse>, ProblemHttpResult>>
            (LoginRequest r, AuthService auth) =>
            await auth.LoginAsync(r) is { } res
                ? TypedResults.Ok(res)
                : TypedResults.Problem("Неверный email или пароль", statusCode: 401))
        .WithSummary("Вход: возвращает токен");

        group.MapGet("/me", (ClaimsPrincipal user) =>
            TypedResults.Ok(new MeResponse(
                user.GetUserId(),
                user.FindFirstValue(JwtRegisteredClaimNames.Email)!)))
        .RequireAuthorization()
        .WithSummary("Кто я (по токену)");
    }
}
