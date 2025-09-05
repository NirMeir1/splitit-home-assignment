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
        var create = new ActorCreateUpdateDto { Name = "Charlie", Rank = 3 };
        var res = await _client.PostAsJsonAsync("/actors", create);
        await AssertStatusAsync(res, HttpStatusCode.Created);

        var createdText = await res.Content.ReadAsStringAsync();
        var created = await res.Content.ReadFromJsonAsync<ActorDetailsDto>();
        created.Should().NotBeNull($"Raw: {createdText}");

        var get = await _client.GetAsync($"/actors/{created!.Id}");
        await AssertStatusAsync(get, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_Update_DuplicateRank_409()
    {
        // Create a new actor first (so we have an ID to update)
        var initial = new ActorCreateUpdateDto { Name = "Zed", Rank = 3 };
        var create = await _client.PostAsJsonAsync("/actors", initial);
        await AssertStatusAsync(create, HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<ActorDetailsDto>();

        // Attempt to set duplicate rank = 1 (Alice already seeded with rank 1)
        var update = new ActorCreateUpdateDto { Name = "Zed", Rank = 1 };
        var put = await _client.PutAsJsonAsync($"/actors/{created!.Id}", update);
        await AssertStatusAsync(put, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_Then_404_On_Get()
    {
        // Create a new actor then delete it
        var create = new ActorCreateUpdateDto { Name = "Temp", Rank = 99 };
        var res = await _client.PostAsJsonAsync("/actors", create);
        await AssertStatusAsync(res, HttpStatusCode.Created);
        var created = await res.Content.ReadFromJsonAsync<ActorDetailsDto>();

        var del = await _client.DeleteAsync($"/actors/{created!.Id}");
        await AssertStatusAsync(del, HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/actors/{created.Id}");
        await AssertStatusAsync(get, HttpStatusCode.NotFound);
    }

    // Minimal types to parse list response cleanly
    private sealed record PagedList(Item[] items, int page, int pageSize, int total);
    private sealed record Item(Guid id, string name);
}