using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SplititAssignment.IntegrationTests.Support;
using SplititAssignment.Application.Actors.Dtos;

namespace SplititAssignment.IntegrationTests;

public class ActorsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ActorsApiTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    // ---------- Helper: assert status and include body on failure ----------
    private static async Task AssertStatusAsync(HttpResponseMessage res, HttpStatusCode expected)
    {
        var body = res.Content == null ? "" : await res.Content.ReadAsStringAsync();
        res.StatusCode.Should().Be(expected, $"Body: {body}");
    }

    [Fact]
    public async Task Get_List_Defaults()
    {
        var res = await _client.GetAsync("/actors");
        await AssertStatusAsync(res, HttpStatusCode.OK);

        var bodyText = await res.Content.ReadAsStringAsync(); // helpful to keep during debugging
        var body = await res.Content.ReadFromJsonAsync<PagedList>();
        body.Should().NotBeNull($"Raw: {bodyText}");
        body!.total.Should().BeGreaterThanOrEqualTo(0);
        body.items.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_FilterByName()
    {
        var res = await _client.GetAsync("/actors?name=Alice");
        await AssertStatusAsync(res, HttpStatusCode.OK);

        var bodyText = await res.Content.ReadAsStringAsync();
        var body = await res.Content.ReadFromJsonAsync<PagedList>();
        body.Should().NotBeNull($"Raw: {bodyText}");
        body!.items.Should().Contain(i => i.name.Contains("Alice"));
    }

    [Fact]
    public async Task Post_Create_Then_GetById()
    {
        // Use a rank value that is unique across tests to avoid collisions
        var id = Guid.NewGuid();
        var create = new ActorUpsertRequestDto { Name = "Charlie", Details = "", Type = "Actor", Rank = 100, Source = "Imdb" };
        var res = await _client.PostAsJsonAsync($"/actors/{id}", create);
        await AssertStatusAsync(res, HttpStatusCode.Created);

        var get = await _client.GetAsync($"/actors/{id}");
        await AssertStatusAsync(get, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_Update_DuplicateRank_409()
    {
        // Create a new actor first (so we have an ID to update)
        var id = Guid.NewGuid();
        var initial = new ActorUpsertRequestDto { Name = "Zed", Details = "", Type = "Actor", Rank = 3, Source = "Imdb" };
        var create = await _client.PostAsJsonAsync($"/actors/{id}", initial);
        await AssertStatusAsync(create, HttpStatusCode.Created);

        // Attempt to set duplicate rank = 1 (Alice already seeded with rank 1)
        var update = new ActorUpsertRequestDto { Name = "Zed", Details = "", Type = "Actor", Rank = 1, Source = "Imdb" };
        var put = await _client.PutAsJsonAsync($"/actors/{id}", update);
        await AssertStatusAsync(put, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_Then_404_On_Get()
    {
        // Create a new actor then delete it
        var id = Guid.NewGuid();
        var create = new ActorUpsertRequestDto { Name = "Temp", Details = "", Type = "Actor", Rank = 99, Source = "Imdb" };
        var res = await _client.PostAsJsonAsync($"/actors/{id}", create);
        await AssertStatusAsync(res, HttpStatusCode.Created);

        var del = await _client.DeleteAsync($"/actors/{id}");
        await AssertStatusAsync(del, HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/actors/{id}");
        await AssertStatusAsync(get, HttpStatusCode.NotFound);
    }

    // Minimal types to parse list response cleanly
    private sealed record PagedList(Item[] items, int page, int pageSize, int total);
    private sealed record Item(Guid id, string name);
}
