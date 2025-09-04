using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SplititAssignment.Domain.Abstractions;
using SplititAssignment.Domain.Entities;
using SplititAssignment.Infrastructure.Persistence;

namespace SplititAssignment.Infrastructure.Seeding;

public sealed class ActorSeedingHostedService : IHostedService
{
    private readonly ILogger<ActorSeedingHostedService> _logger;
    private readonly IServiceProvider _services;
    private readonly IEnumerable<IActorProvider> _providers;

    public ActorSeedingHostedService(
        ILogger<ActorSeedingHostedService> logger,
        IServiceProvider services,
        IEnumerable<IActorProvider> providers)
    {
        _logger = logger;
        _services = services;
        _providers = providers;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IActorRepository>();

        if (await repo.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Actors already seeded. Skipping.");
            return;
        }

        _logger.LogInformation("Seeding actors from providers...");
        var combined = new List<Actor>();

        foreach (var provider in _providers)
        {
            try
            {
                var batch = await provider.FetchAsync(cancellationToken);
                combined.AddRange(batch);
                _logger.LogInformation("Provider {Provider} returned {Count} actors.",
                    provider.GetType().Name, batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Provider {Provider} failed; continuing.", provider.GetType().Name);
            }
        }

        // Normalize ranks sequentially & ensure uniqueness
        combined = combined
            .OrderBy(a => a.Rank)
            .Select((a, i) => { a.Rank = i + 1; return a; })
            .ToList();

        var seen = new HashSet<int>();
        foreach (var a in combined)
        {
            while (!seen.Add(a.Rank))
                a.Rank++;
        }

        foreach (var a in combined)
            await repo.AddAsync(a, cancellationToken);

        _logger.LogInformation("Seeding complete. Inserted {Count} actors.", combined.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
