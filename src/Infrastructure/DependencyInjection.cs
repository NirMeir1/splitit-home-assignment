using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SplititAssignment.Domain.Abstractions;
using SplititAssignment.Infrastructure.Persistence;
using SplititAssignment.Infrastructure.Providers;
using SplititAssignment.Infrastructure.Seeding;

namespace SplititAssignment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ActorsDbContext>(opt => opt.UseInMemoryDatabase("ActorsDb"));

        services.AddScoped<IActorRepository, ActorRepository>();

        services.AddHttpClient<ImdbTopActorsProvider>();
        services.AddSingleton<IActorProvider>(sp => sp.GetRequiredService<ImdbTopActorsProvider>());
        services.AddSingleton<IActorProvider, StubRottenTomatoesProvider>();

        services.AddHostedService<ActorSeedingHostedService>();

        return services;
    }
}
