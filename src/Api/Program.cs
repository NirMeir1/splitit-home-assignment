using SplititAssignment.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SplititAssignment.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (DbContext, repo, providers, seeding)
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Splitit Assignment API v1");
    c.RoutePrefix = string.Empty;
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/debug/actors", async (ActorsDbContext db) =>
{
    var list = await db.Actors.AsNoTracking().Select(a => new { a.Id, a.Name, a.Rank }).ToListAsync();
    return Results.Ok(list);
});

app.Run();
