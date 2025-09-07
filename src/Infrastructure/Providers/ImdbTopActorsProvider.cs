using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq; 
using SplititAssignment.Domain.Abstractions;
using SplititAssignment.Domain.Entities;
using SplititAssignment.Domain.Enums;

namespace SplititAssignment.Infrastructure.Providers;

public sealed class ImdbTopActorsProvider : IActorProvider
{
    private readonly HttpClient _http;
    private const string ListUrl = "https://www.imdb.com/list/ls054840033/";
    private static readonly Dictionary<string, string> _bioCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly SemaphoreSlim _bioGate = new(10); 


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

        var jsonScripts = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']")
                         ?? new HtmlNodeCollection(null);

        JArray? itemList = null;
        int totalExpected = 0;

        foreach (var script in jsonScripts)
        {
            var text = script.InnerText?.Trim();
            if (string.IsNullOrEmpty(text)) continue;

            JToken root;
            try { root = JToken.Parse(text); } catch { continue; }

            if (root is JArray arr)
            {
                foreach (var obj in arr.OfType<JObject>())
                {
                    var items = obj["itemListElement"] as JArray;
                    if (items != null && items.Count > 0)
                    {
                        itemList = items;
                        totalExpected = (int?)obj["numberOfItems"] ?? 0;
                        break;
                    }
                }
            }
            else if (root is JObject obj)
            {
                var items = obj["itemListElement"] as JArray;
                if (items != null && items.Count > 0)
                {
                    itemList = items;
                    totalExpected = (int?)obj["numberOfItems"] ?? 0;
                }
            }

            if (itemList != null) break;
        }

        var results = new List<Actor>();
        if (itemList == null || itemList.Count == 0)
            return results;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var tasks = new List<Task>();

        foreach (var entry in itemList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rank = (int?)entry["position"] ?? 0;
            var item = entry["item"];
            if (item == null) continue;

            var name = item["name"]?.ToString()?.Trim();
            var url = item["url"]?.ToString();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                continue;

            var nmId = ExtractNmId(url);
            if (string.IsNullOrEmpty(nmId)) continue;
            if (!seen.Add(nmId)) continue;

            var actor = new Actor
            {
                Name = name,
                Details = string.Empty, // fill async later
                Type = "Actor",
                Rank = rank > 0 ? rank : (results.Count + 1),
                Source = ProviderSource.Imdb
            };

            results.Add(actor);

            tasks.Add(FillDetailsAsync(actor, entry, item, nmId, cancellationToken));
        }

        await Task.WhenAll(tasks);

        // Normalize ranks 1..N (stable sort: by provided rank, then name)
        return results
            .OrderBy(a => a.Rank == 0 ? int.MaxValue : a.Rank)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Select((a, i) => { a.Rank = i + 1; return a; })
            .ToList();
    }

    private static string? ExtractNmId(string urlOrPath)
    {
        var m = Regex.Match(urlOrPath, @"/name/(nm\d{1,9})", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value : null;
    }

    private async Task FillDetailsAsync(Actor actor, JToken entry, JToken item, string nmId, CancellationToken ct)
    {
        var desc = entry["description"]?.ToString()?.Trim()
            ?? item["description"]?.ToString()?.Trim()
            ?? item["disambiguatingDescription"]?.ToString()?.Trim()
            ?? await TryFetchMiniBioAsync(_http, nmId, ct)
            ?? string.Empty;

        actor.Details = desc;
    }
    
    private static async Task<string?> TryFetchMiniBioAsync(HttpClient http, string nmId, CancellationToken ct)
    {
        if (_bioCache.TryGetValue(nmId, out var cached))
            return cached;

        await _bioGate.WaitAsync(ct);
        try
        {
            if (_bioCache.TryGetValue(nmId, out cached))
                return cached;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(6));

            var url = $"https://www.imdb.com/name/{nmId}/";
            using var resp = await http.GetAsync(url, cts.Token);
            if (!resp.IsSuccessStatusCode)
            {
                _bioCache[nmId] = string.Empty;
                return string.Empty;
            }

            var html = await resp.Content.ReadAsStringAsync(cts.Token);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.DocumentNode.SelectSingleNode(@".//*[@data-testid='name-bio-text'
                                               or @data-testid='mini-bio'
                                               or contains(@class,'ipc-html-content--base')
                                               or contains(@class,'ipc-html-content-inner-div')
                                               or contains(@class,'ipc-html-content')]");


            var raw = node?.InnerText;
            if (string.IsNullOrWhiteSpace(raw) && node != null)
            {
                var parts = node.SelectNodes(".//p|.//span");
                if (parts is { Count: > 0 })
                    raw = string.Join(" ", parts.Select(n => n.InnerText));
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                _bioCache[nmId] = string.Empty;
                return string.Empty;
            }

            var text = HtmlEntity.DeEntitize(Regex.Replace(raw, @"\s+", " ").Trim());
            _bioCache[nmId] = text;
            return text;
        }
        catch
        {
            _bioCache[nmId] = string.Empty;
            return string.Empty;
        }
        finally
        {
            _bioGate.Release();
        }
    }


}