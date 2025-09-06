using System.Net.Http.Headers;
using HtmlAgilityPack;
using SplititAssignment.Domain.Abstractions;
using SplititAssignment.Domain.Entities;
using SplititAssignment.Domain.Enums;

namespace SplititAssignment.Infrastructure.Providers;

public sealed class ImdbTopActorsProvider : IActorProvider
{
    private readonly HttpClient _http;
    private const string ListUrl = "https://www.imdb.com/list/ls054840033/";

    public ImdbTopActorsProvider(HttpClient httpClient)
    {
        _http = httpClient;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; SplititAssignment/1.0)");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
    }

    public async Task<IReadOnlyList<Actor>> FetchAsync(CancellationToken cancellationToken = default)
    {
        using var resp = await _http.GetAsync(ListUrl, cancellationToken);
        resp.EnsureSuccessStatusCode();
        var html = await resp.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var items = doc.DocumentNode
            .SelectNodes("//div[contains(@class,'lister-item')]")
            ?? doc.DocumentNode.SelectNodes("//li[contains(@class,'ipc-metadata-list-summary-item')]")
            ?? new HtmlNodeCollection(null);

        var results = new List<Actor>(capacity: items.Count);
        int rankCounter = 0;

        foreach (var node in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rank = TryParseRank(node) ?? ++rankCounter;
            var name = TryGetName(node);
            if (string.IsNullOrWhiteSpace(name)) continue;

            results.Add(new Actor
            {
                Name = name.Trim(),
                Rank = rank,
                Source = ProviderSource.Imdb,
                ExternalId = TryGetExternalId(node)
            });
        }

        // Normalize ranks sequentially (defensive)
        results = results
            .OrderBy(a => a.Rank)
            .Select((a, i) => { a.Rank = i + 1; return a; })
            .ToList();

        return results;
    }

    private static int? TryParseRank(HtmlNode node)
    {
        var text = node.SelectSingleNode(".//span[contains(@class,'lister-item-index')]")?.InnerText
                ?? node.SelectSingleNode(".//div[contains(@class,'ipc-title__text')]")?.InnerText;
        if (string.IsNullOrWhiteSpace(text)) return null;
        var digits = new string(text.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var r) ? r : null;
    }

    private static string? TryGetName(HtmlNode node)
    {
        var a = node.SelectSingleNode(".//h3//a")
             ?? node.SelectSingleNode(".//a[contains(@href,'/name/nm')]");
        return a?.InnerText?.Trim();
    }

    private static string? TryGetExternalId(HtmlNode node)
    {
        var href = node.SelectSingleNode(".//a[contains(@href,'/name/nm')]")?.GetAttributeValue("href", null);
        if (string.IsNullOrWhiteSpace(href)) return null;
        var parts = href.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.FirstOrDefault(p => p.StartsWith("nm", StringComparison.OrdinalIgnoreCase));
    }

    
}
