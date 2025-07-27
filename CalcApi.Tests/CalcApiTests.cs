using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace CalcApi.Tests;

public class CalcApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CalcApiTests(TestWebApplicationFactory factory) => _factory = factory;

    private static async Task<string> GetDevJwtAsync(HttpClient client)
    {
        // Call your dev token endpoint
        using var resp = await client.PostAsync("/auth/token", content: null);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.TryGetProperty("token", out var t)
            ? t.GetString()
            : doc.RootElement.GetProperty("access_token").GetString();

        token.Should().NotBeNullOrWhiteSpace("auth/token should return a JWT for dev");
        return token!;
    }

    [Fact]
    public async Task Calc_Add_Returns_200_And_3_When_Authorized()
    {
        var client = _factory.CreateClient();

        var token = await GetDevJwtAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Op", "add");

        var body = new { number1 = 1, number2 = 2 };
        using var resp = await client.PostAsJsonAsync("/calc", body);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await resp.Content.ReadFromJsonAsync<JsonElement>();
        payload.TryGetProperty("result", out var resultProp).Should().BeTrue();
        resultProp.GetDecimal().Should().Be(3m);
    }

    [Fact]
    public async Task Calc_Returns_401_When_Missing_Token()
    {
        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add("X-Op", "add");
        var body = new { number1 = 1, number2 = 2 };

        using var resp = await client.PostAsJsonAsync("/calc", body);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Calc_Returns_400_When_Invalid_XOp()
    {
        var client = _factory.CreateClient();

        var token = await GetDevJwtAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Op", "nope"); // invalid

        var body = new { number1 = 1, number2 = 2 };
        using var resp = await client.PostAsJsonAsync("/calc", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resp.Content.ReadAsStringAsync()).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Calc_Returns_400_On_DivideByZero()
    {
        var client = _factory.CreateClient();

        var token = await GetDevJwtAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Op", "div");

        var body = new { number1 = 10, number2 = 0 };
        using var resp = await client.PostAsJsonAsync("/calc", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}