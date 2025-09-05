using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SplititAssignment.Api;
using SplititAssignment.Infrastructure.Persistence;
using SplititAssignment.Infrastructure.Seeding;
using SplititAssignment.Domain.Entities;

namespace SplititAssignment.IntegrationTests.Support;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove hosted seeding to avoid network calls and non-determinism
            var hosted = services.FirstOrDefault(d =>
                d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                d.ImplementationType == typeof(ActorSeedingHostedService));
            if (hosted is not null) services.Remove(hosted);

            // Replace DbContext with isolated InMemory DB
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<ActorsDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<ActorsDbContext>(opt => opt.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

            // Build provider & seed known data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ActorsDbContext>();

            db.Actors.AddRange(
                new Actor { Id = Guid.NewGuid(), Name = "Alice Actor", Rank = 1 },
                new Actor { Id = Guid.NewGuid(), Name = "Bob Actor", Rank = 2 }
            );
            db.SaveChanges();
        });
    }
}
