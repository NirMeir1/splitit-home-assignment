var builder = WebApplication.CreateBuilder(args);

// Swagger (OpenAPI) for .NET 8
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Always expose Swagger (handy for the assignment)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Splitit Assignment API v1");
    c.RoutePrefix = string.Empty; // serve UI at "/"
});

// Simple health/ping
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
