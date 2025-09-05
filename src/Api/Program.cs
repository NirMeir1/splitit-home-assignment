using SplititAssignment.Infrastructure;
using SplititAssignment.Api.Endpoints;

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

app.MapActorsEndpoints();

app.Run();
