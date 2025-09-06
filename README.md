# SplititAssignment – Actors API

A minimal Actors API with filtering, pagination, and a unique-rank constraint. Includes unit and integration tests over Entity Framework Core In‑Memory.

## Tech

- .NET 8
- ASP.NET Core Minimal API
- EF Core In‑Memory
- xUnit + FluentAssertions
- Swagger (OpenAPI)

---

## Quick Start

### Prerequisites

- .NET SDK 8.x

### Build

```bash
dotnet --info
dotnet restore
dotnet build -c Release
```

### Run API (Development)

- Windows (PowerShell)

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/Api --urls "http://localhost:5176"
```

- Windows (cmd)

```cmd
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src\Api --urls "http://localhost:5176"
```

- macOS/Linux (bash)

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/Api --urls "http://localhost:5176"
```

Open Swagger UI: `http://localhost:5176`

Notes:
- The Infrastructure layer registers a hosted seeder that fetches data from IMDb (HTML scrape) and a stub provider. This runs on app start; it’s removed during integration tests.
- The sample uses EF Core In‑Memory for simplicity.

---

## API Overview

- Base: `/actors`
- Filters: `name`, `rankMin`, `rankMax`
- Paging: `page` (>=1, default 1), `pageSize` (1..100, default 20)
 

Endpoints:
- `GET /actors`: Returns paged list of items `{ id, name }` with metadata `{ page, pageSize, total }`.
- `GET /actors/{id}`: Returns full actor details.
- `POST /actors`: Creates an actor (201 + Location). Enforces unique `rank`.
- `PUT /actors/{id}`: Updates an actor. Enforces unique `rank`.
- `DELETE /actors/{id}`: Deletes an actor (204 or 404).
- `GET /health`: Basic health endpoint.

Error shape (examples):
- 404: `{ "code": "not_found", "message": "Actor not found." }`
- 409: `{ "code": "conflict", "message": "Conflict.", "details": { "rank": ["Duplicate rank"] } }`
- 400 validation: `{ "code": "validation_error", "message": "One or more validation errors occurred.", "details": { ... } }`

Sample list response:

```json
{
  "page": 1,
  "pageSize": 2,
  "total": 4,
  "items": [
    { "id": "00000000-0000-0000-0000-000000000000", "name": "Alice Actor" },
    { "id": "00000000-0000-0000-0000-000000000000", "name": "Bob Actor" }
  ]
}
```

---

## Data Model (Key Types)

- Domain entity `Actor` (Id, Name, Rank, ImageUrl?, KnownFor?, PrimaryProfession?, TopMovies[], Source, ExternalId?).
- DTOs:
  - `ActorListItemDto` (Id, Name)
  - `ActorDetailsDto` (Id, Name, Rank, ImageUrl?, KnownFor?, PrimaryProfession?, TopMovies[], Source, ExternalId?)
  - `ActorCreateUpdateDto` (Name, Rank, ImageUrl?, KnownFor?, PrimaryProfession?, TopMovies[]?)

Validation:
- Name required (<=256), Rank > 0, TopMovies cannot contain empty strings.

Persistence notes:
- EF Core configuration maps `TopMovies` to a string column and uses a `ValueComparer` for change tracking.
- `Source` enum is stored as string.

---

## Architecture

- `src/Api`: Minimal API endpoints and Swagger.
- `src/Application`: DTOs, mappings, validation, pagination model.
- `src/Domain`: Entity and provider abstraction.
- `src/Infrastructure`: EF Core DbContext/repository, data providers (IMDb scrape + stub), startup seeding.
- `tests/UnitTests`: Validator and repository tests.
- `tests/IntegrationTests`: End‑to‑end API tests with custom factory.

---

## Running Tests

```bash
dotnet test -c Release
```

Integration tests:
- Use `CustomWebApplicationFactory` that swaps the DbContext to a fresh EF In‑Memory database per test host.
- A shared `InMemoryDatabaseRoot` ensures all scopes in a test share the same store; the hosted seeder is removed.
- The factory seeds two actors: `Alice Actor` (rank 1) and `Bob Actor` (rank 2).

---

## Project Layout

```
src/
  Api/
  Application/
  Domain/
  Infrastructure/
tests/
  IntegrationTests/
    ActorsApiTests.cs
    Support/CustomWebApplicationFactory.cs
  UnitTests/
    Actors/
    Infrastructure/
```

---

## Troubleshooting

- Change `--urls` if the port is in use.
- Ensure `ASPNETCORE_ENVIRONMENT=Development` to view Swagger.
- For flaky tests, verify the test factory uses a single in‑memory DB name and removes the hosted seeder (already configured here).

