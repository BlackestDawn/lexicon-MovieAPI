# MovieAPI

A RESTful Web API built with ASP.NET Core for browsing and managing movie data — think a small-scale IMDB clone. The API exposes information about movies, people (actors/directors), genres, and user ratings/reviews.

> **Status:** Movies resource fully implemented (CRUD + filtering + pagination). People, Genres, and Reviews endpoints are repository-ready but controllers are not yet built.

## Implemented Features

- **Movies** — Full CRUD: list with filtering/pagination, get by ID, create, full update (PUT), partial update (PATCH via JSON Patch), delete
- **Filtering** — Filter movies by name, free-text search (title + plot), genre, release year, and minimum rating
- **Pagination** — Configurable page/pageSize with metadata returned in `X-Pagination` response header
- **Validation** — FluentValidation on create and update requests (required fields, date range, positive runtime/budget, at least one genre and cast/crew member)
- **DTO Mapping** — AutoMapper profiles for movies, people, genres, and reviews
- **Domain Tracking** — `CreatedAt` / `UpdatedAt` auto-managed via an EF Core save interceptor
- **Dev Seeding** — Database is seeded with sample data in the Development environment
- **Unit Tests** — xUnit + Moq tests covering `MovieService` and both validators

## Planned Features

- **People** — Actors, directors, and crew members with filmographies (repository layer exists)
- **Genres** — Read endpoints for genre listings (repository layer exists)
- **Reviews** — Get and post reviews per movie (repository layer exists)
- **Integration Tests** — End-to-end tests against a real database

## Tech Stack

- **ASP.NET Core Web API** (.NET 10)
- **Entity Framework Core** with SQL Server
- **AutoMapper** for DTO mapping
- **FluentValidation** for request validation
- **Swagger / OpenAPI** (Swashbuckle + `Microsoft.AspNetCore.OpenApi`) for API docs
- **xUnit + Moq** for unit testing

## Project Structure

```
MovieAPI/
├── MovieAPI.slnx
├── src/
│   ├── MovieAPI.Api/           # Controllers, program entry point, appsettings
│   ├── MovieAPI.Application/   # Services, validators, DTOs, AutoMapper profiles
│   ├── MovieAPI.Domain/        # Entities (Movie, Person, Genre, Review, CastCrew, MovieDetail)
│   └── MovieAPI.Infrastructure/ # AppDbContext, EF Fluent configs, migrations, repository, seeder
└── tests/
    ├── MovieAPI.UnitTests/      # MovieService and validator tests
    └── MovieAPI.IntegrationTests/ # Placeholder
```

## Getting Started

1. Clone the repository
2. Set the SQL Server connection string in `src/MovieAPI.Api/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "sqlserver": "Server=...;Database=MovieAPI;..."
     }
   }
   ```
3. Apply database migrations:
   ```
   dotnet ef database update --project src/MovieAPI.Infrastructure --startup-project src/MovieAPI.Api
   ```
4. Run the API:
   ```
   dotnet run --project src/MovieAPI.Api
   ```
5. Browse the API docs at `https://localhost:<port>/swagger`

The database is automatically seeded with sample data when running in the Development environment.

## API Overview

| Method   | Route                   | Description                                      |
|----------|-------------------------|--------------------------------------------------|
| `GET`    | `/api/movies`           | List movies (filter by `name`, `search`, `genre`, `year`, `minRating`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/movies/{id}`      | Get a single movie by ID (optional `includePeople` query param) |
| `POST`   | `/api/movies`           | Create a new movie                               |
| `PUT`    | `/api/movies/{id}`      | Full update of a movie                           |
| `PATCH`  | `/api/movies/{id}`      | Partial update via JSON Patch                    |
| `DELETE` | `/api/movies/{id}`      | Delete a movie                                   |

## License

This project is released under the [CC0 1.0 Universal](LICENSE) license — public domain dedication.
