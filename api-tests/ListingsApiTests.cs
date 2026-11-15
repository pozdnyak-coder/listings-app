using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ListingsApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ListingsApi.Tests;

// IClassFixture<ApiFactory> — один сервер на весь класс. Базу делят все тесты,
// поэтому у каждого теста свои пользователи (уникальные email), а проверки — на форму ответа.
public class ListingsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string NewEmail() => $"{Guid.NewGuid():N}@test.uz";

    private async Task<string> RegisterAsync(string email)
    {
        var res = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "secret123"));
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<AuthResponse>())!.Token;
    }

    // отдельный клиент с токеном конкретного пользователя
    private HttpClient ClientFor(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<ListingResponse> CreateAsync(HttpClient client, string title = "Квартира у метро")
    {
        var res = await client.PostAsJsonAsync("/api/listings", new CreateListingRequest(title, 100, 2, 1, null));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<ListingResponse>())!;
    }

    [Fact]
    public async Task Список_отдаёт_200_и_страницу()
    {
        var page = await _client.GetFromJsonAsync<PagedResponse<ListingResponse>>("/api/listings?pageSize=2");

        Assert.NotNull(page);
        Assert.True(page.Items.Count <= 2);
        Assert.Equal(2, page.PageSize);
        Assert.True(page.Total >= 12);                         // начальные данные
    }

    [Fact]
    public async Task Справочник_районов_отдаёт_8_районов()
    {
        var districts = await _client.GetFromJsonAsync<List<DistrictResponse>>("/api/districts");
        Assert.Equal(8, districts!.Count);
    }

    [Fact]
    public async Task Создание_без_токена_даёт_401()
    {
        var res = await _client.PostAsJsonAsync("/api/listings",
            new CreateListingRequest("Квартира у метро", 100, 2, 1, null));

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Пустой_title_даёт_400_с_errors_title()
    {
        var client = ClientFor(await RegisterAsync(NewEmail()));

        var res = await client.PostAsJsonAsync("/api/listings", new CreateListingRequest("", 100, 2, 1, null));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var problem = await res.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("title", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Заголовок_Дом_короче_5_символов_даёт_400()
    {
        var client = ClientFor(await RegisterAsync(NewEmail()));

        var res = await client.PostAsJsonAsync("/api/listings", new CreateListingRequest("Дом", 100, 2, 1, null));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task С_токеном_создаётся_201_и_автор_я()
    {
        var client = ClientFor(await RegisterAsync(NewEmail()));

        var res = await client.PostAsJsonAsync("/api/listings",
            new CreateListingRequest("Квартира у метро", 100, 2, 1, null));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var created = await res.Content.ReadFromJsonAsync<ListingResponse>();
        Assert.NotNull(created!.OwnerId);
    }

    [Fact]
    public async Task Несуществующее_объявление_404()
    {
        var res = await _client.GetAsync("/api/listings/999999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Чужое_объявление_нельзя_изменить_403()
    {
        var author = ClientFor(await RegisterAsync(NewEmail()));
        var stranger = ClientFor(await RegisterAsync(NewEmail()));
        var created = await CreateAsync(author);

        var res = await stranger.PutAsJsonAsync($"/api/listings/{created.Id}",
            new UpdateListingRequest("Взлом чужого объявления", 1, 2, 1, null));

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Своё_объявление_удаляется_204_и_потом_404()
    {
        var author = ClientFor(await RegisterAsync(NewEmail()));
        var created = await CreateAsync(author);

        var delete = await author.DeleteAsync($"/api/listings/{created.Id}");
        var get = await _client.GetAsync($"/api/listings/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Мои_объявления_требуют_вход_и_содержат_только_мои()
    {
        var anonymous = await _client.GetAsync("/api/listings/mine");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var me = ClientFor(await RegisterAsync(NewEmail()));
        var created = await CreateAsync(me, "Моя единственная квартира");

        var mine = await me.GetFromJsonAsync<List<ListingResponse>>("/api/listings/mine");

        Assert.Single(mine!);
        Assert.Equal(created.Id, mine![0].Id);
    }

    [Fact]
    public async Task Регистрация_того_же_email_в_другом_регистре_даёт_400()
    {
        var email = NewEmail();
        await RegisterAsync(email);

        var res = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email.ToUpperInvariant(), "secret123"));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Вход_с_неверным_паролем_даёт_401()
    {
        var email = NewEmail();
        await RegisterAsync(email);

        var res = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "wrong-pass1"));

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Health_отвечает_200()
    {
        var res = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
