using System.Net.Http.Headers;
using System.Text.RegularExpressions;
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
                Details = TryGetDetails(node),
                Type = "Actor",
                Rank = rank,
                Source = ProviderSource.Imdb
            });
        }

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
        // 1) New IMDb layout: <h3 class="ipc-title__text ipc-title__text--reduced">
        var h3 = node.SelectSingleNode(".//h3[contains(@class,'ipc-title__text')]");
        
        // 2) Fallback: <div class="ipc-title"> ... <a><h3>NAME</h3></a>
        h3 ??= node.SelectSingleNode(".//div[contains(@class,'ipc-title')]//a//h3");
        
        // 3) Legacy list layout: <h3><a>NAME</a></h3>
        h3 ??= node.SelectSingleNode(".//h3//a");
        
        // 4) Last resort: any anchor linking to /name/nm... (use text)
        var textNode = h3 ?? node.SelectSingleNode(".//a[contains(@href,'/name/nm')]");

        var raw = textNode?.InnerText;
        if (string.IsNullOrWhiteSpace(raw)) return null;

        // Normalize: HTML-decode, collapse whitespace, strip any leading rank like "1. "
        var decoded = HtmlEntity.DeEntitize(raw);
        decoded = Regex.Replace(decoded, @"\s+", " ").Trim();
        decoded = Regex.Replace(decoded, @"^\d+\s*[\.\)]\s*", ""); 

        return string.IsNullOrWhiteSpace(decoded) ? null : decoded;
    }

    private static string? TryGetDetails(HtmlNode itemNode)
    {
        var candidate = itemNode.SelectSingleNode(".//*[@data-testid='dli-bio']")
            ?? itemNode.SelectSingleNode(".//div[contains(@class,'ipc-html-content-inner-div')]")
            ?? itemNode.SelectSingleNode(".//div[contains(@class,'ipc-html-content')]");

        var raw = candidate?.InnerText;
        return CleanText(raw);
    }

    private static string? CleanText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var decoded = HtmlEntity.DeEntitize(text);
        decoded = Regex.Replace(decoded, @"\s+", " ").Trim();

        return string.IsNullOrWhiteSpace(decoded) ? null : decoded;
    }
    
}
