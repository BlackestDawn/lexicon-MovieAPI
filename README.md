# MovieAPI

A RESTful Web API built with ASP.NET Core for browsing and managing movie data — think a small-scale IMDB clone. The API exposes information about movies, people (actors/directors), genres, and user ratings/reviews.

> **Status:** All four core resources — Movies, People, Genres, and Reviews — are implemented end-to-end with full CRUD (controller, service, validation, DTO mapping, unit tests). Repositories expose tracked and read-only (`AsNoTracking`) query paths, with GET endpoints wired to the read-only versions. Integration tests project is currently empty (no tests yet).

## Implemented Features

- **Movies** — Full CRUD: list with filtering/pagination, get by ID, create, full update (PUT), partial update (PATCH via JSON Patch), delete
- **People** — Full CRUD for actors/directors/crew, with filtering by name/genre/year and optional filmography inclusion
- **Reviews** — Full CRUD scoped to a movie (`/api/movies/{movieId}/reviews`), with filtering by search text and score range
- **Genres** — Full CRUD: list, get by ID (optional `includeMovies`), create, full update (PUT), partial update (PATCH via JSON Patch), delete
- **Filtering** — Movies by name, free-text search (title + plot), genre, release year, minimum rating; People by name, genre, year; Reviews by search text and min/max score
- **Pagination** — Configurable page/pageSize with metadata returned in `X-Pagination` response header (Movies, People, Reviews)
- **Validation** — FluentValidation on create/update requests for Movies, People, Reviews, and Genres, using a single shared "change" DTO/validator per resource for create and update
- **Read-only query paths** — Each repository exposes both a tracked and a `AsNoTracking` read-only variant of its GET methods; read (GET) endpoints use the read-only versions, write flows use the tracked versions
- **DTO Mapping** — AutoMapper profiles for movies, people, genres, and reviews
- **Domain Tracking** — `CreatedAt` / `UpdatedAt` auto-managed via an EF Core save interceptor
- **Dev Seeding** — Database is seeded with sample data in the Development environment
- **Unit Tests** — xUnit + Moq tests covering all four services (including Genre's create/update/remove flows) and the Movie/Person validators

## Planned Features

- **Integration Tests** — End-to-end tests against a real database (project scaffolded, no tests written yet)
- **Genre/Review validator tests** — Dedicated validator unit tests currently only exist for Movie and Person

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
    ├── MovieAPI.UnitTests/      # Service tests for all four resources, validator tests for Movie/Person
    └── MovieAPI.IntegrationTests/ # Scaffolded project, no tests yet
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

| Method   | Route                                   | Description                                      |
|----------|------------------------------------------|--------------------------------------------------|
| `GET`    | `/api/movies`                           | List movies (filter by `name`, `search`, `genre`, `year`, `minRating`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/movies/{id}`                      | Get a single movie by ID (optional `includePeople` query param) |
| `POST`   | `/api/movies`                           | Create a new movie                               |
| `PUT`    | `/api/movies/{id}`                      | Full update of a movie                           |
| `PATCH`  | `/api/movies/{id}`                      | Partial update via JSON Patch                    |
| `DELETE` | `/api/movies/{id}`                      | Delete a movie                                   |
| `GET`    | `/api/people`                           | List people (filter by `name`, `genre`, `year`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/people/{id}`                      | Get a single person by ID (optional `includeMovies` query param) |
| `POST`   | `/api/people`                           | Create a new person                              |
| `PUT`    | `/api/people/{id}`                      | Full update of a person                          |
| `PATCH`  | `/api/people/{id}`                      | Partial update via JSON Patch                    |
| `DELETE` | `/api/people/{id}`                      | Delete a person                                  |
| `GET`    | `/api/genres`                           | List genres                                      |
| `GET`    | `/api/genres/{id}`                      | Get a single genre by ID (optional `includeMovies` query param) |
| `POST`   | `/api/genres`                           | Create a new genre                               |
| `PUT`    | `/api/genres/{id}`                      | Full update of a genre                           |
| `PATCH`  | `/api/genres/{id}`                      | Partial update via JSON Patch                    |
| `DELETE` | `/api/genres/{id}`                      | Delete a genre                                   |
| `GET`    | `/api/movies/{movieId}/reviews`         | List reviews for a movie (filter by `search`, `minScore`, `maxScore`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/movies/{movieId}/reviews/{id}`    | Get a single review                              |
| `POST`   | `/api/movies/{movieId}/reviews`         | Create a review for a movie                      |
| `PUT`    | `/api/movies/{movieId}/reviews/{id}`    | Full update of a review                          |
| `PATCH`  | `/api/movies/{movieId}/reviews/{id}`    | Partial update via JSON Patch                    |
| `DELETE` | `/api/movies/{movieId}/reviews/{id}`    | Delete a review                                  |

## License

This project is released under the [CC0 1.0 Universal](LICENSE) license — public domain dedication.
