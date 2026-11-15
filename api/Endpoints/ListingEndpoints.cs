using System.Security.Claims;
using ListingsApi.Auth;
using ListingsApi.Contracts;
using ListingsApi.Services;
using ListingsApi.Validators;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ListingsApi.Endpoints;

public static class ListingEndpoints
{
    public static void MapListingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/listings").WithTags("Listings");

        // GET /api/listings?page=1&pageSize=6&sort=price_asc&districtId=1&maxPrice=50000
        group.MapGet("/", async (ListingService service, int? districtId, decimal? maxPrice,
                                 int page = 1, int pageSize = 10, string sort = "new") =>
            TypedResults.Ok(await service.GetPageAsync(districtId, maxPrice, page, pageSize, sort)))
        .WithName("GetListings")
        .WithSummary("Одна страница объявлений с фильтром и сортировкой");

        // GET /api/listings/mine — объявления вошедшего пользователя
        group.MapGet("/mine", async (ClaimsPrincipal user, ListingService service) =>
            TypedResults.Ok(await service.GetMineAsync(user.GetUserId())))
        .RequireAuthorization()
        .WithName("GetMyListings")
        .WithSummary("Мои объявления");

        group.MapGet("/{id:int}", async Task<Results<Ok<ListingResponse>, NotFound>>
            (int id, ListingService service) =>
            await service.GetAsync(id) is { } dto
                ? TypedResults.Ok(dto)
                : TypedResults.NotFound())
        .WithName("GetListing")
        .WithSummary("Одно объявление по id");

        group.MapPost("/", async Task<Results<Created<ListingResponse>, ValidationProblem>>
            (CreateListingRequest request, ClaimsPrincipal user, ListingService service) =>
        {
            var created = await service.CreateAsync(request, user.GetUserId());
            if (created is null)    // район удалили между проверкой и вставкой — редко, но возможно
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["districtId"] = ["Район не найден"]
                });
            return TypedResults.Created($"/api/listings/{created.Id}", created);
        })
        .RequireAuthorization()
        .AddEndpointFilter<ValidationFilter<CreateListingRequest>>()
        .WithName("CreateListing")
        .WithSummary("Создать объявление (нужен вход)");

        group.MapPut("/{id:int}", async Task<Results<NoContent, NotFound, ForbidHttpResult>>
            (int id, UpdateListingRequest request, ClaimsPrincipal user, ListingService service) =>
            await service.UpdateAsync(id, request, user.GetUserId()) switch
            {
                ChangeResult.Ok       => TypedResults.NoContent(),
                ChangeResult.NotFound => TypedResults.NotFound(),
                _                     => TypedResults.Forbid(),
            })
        .RequireAuthorization()
        .AddEndpointFilter<ValidationFilter<UpdateListingRequest>>()
        .ProducesValidationProblem()
        .WithName("UpdateListing")
        .WithSummary("Изменить объявление (только автор)");

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, ForbidHttpResult>>
            (int id, ClaimsPrincipal user, ListingService service) =>
            await service.DeleteAsync(id, user.GetUserId()) switch
            {
                ChangeResult.Ok       => TypedResults.NoContent(),
                ChangeResult.NotFound => TypedResults.NotFound(),
                _                     => TypedResults.Forbid(),
            })
        .RequireAuthorization()
        .WithName("DeleteListing")
        .WithSummary("Удалить объявление (только автор)");

        // Endpoint — «переводчик»: результат сервиса → код ответа
        group.MapPost("/{id:int}/photo", async Task<Results<Ok<PhotoResponse>,
                NotFound, ForbidHttpResult, ValidationProblem>>
            (int id, IFormFile file, ClaimsPrincipal user, ListingService service) =>
            await service.SetPhotoAsync(id, user.GetUserId(), file) switch
            {
                { Result: ChangeResult.NotFound }  => TypedResults.NotFound(),
                { Result: ChangeResult.Forbidden } => TypedResults.Forbid(),
                { Error: { } e }                   => TypedResults.ValidationProblem(
                                                          new Dictionary<string, string[]> { ["file"] = [e] }),
                { Url: var url }                   => TypedResults.Ok(new PhotoResponse(url!)),
            })
        .RequireAuthorization()
        .DisableAntiforgery()           // форма с файлом + вход по JWT, а не по cookie
        .WithName("UploadListingPhoto")
        .WithSummary("Загрузить фото к своему объявлению");
    }
}
