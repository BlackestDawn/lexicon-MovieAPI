# MovieAPI

A RESTful Web API built with ASP.NET Core for browsing and managing movie data — think a small-scale IMDB clone. The API exposes information about movies, people (actors/directors), genres, and user ratings/reviews.

> **Status:** Early planning stage. The solution (`MovieAPI.slnx`) has been created, but no projects exist yet.

## Planned Features

- **Movies** — CRUD operations for movies (title, release date, plot summary, runtime, poster)
- **People** — Actors, directors, and crew members with filmographies
- **Genres** — Categorize movies by genre, with filtering/search support
- **Cast & Crew** — Associate people with movies and their roles (actor, director, writer, etc.)
- **Ratings & Reviews** — Users can rate and review movies; aggregate ratings per movie
- **Search & Filtering** — Search movies by title, genre, year, rating, cast, etc.
- **Pagination & Sorting** — Efficient browsing of large result sets

## Planned Tech Stack

- **ASP.NET Core Web API** (.NET 8+)
- **Entity Framework Core** for data access
- **SQL Server / PostgreSQL** (TBD) as the relational database
- **AutoMapper** for DTO mapping
- **FluentValidation** for request validation
- **Swagger / OpenAPI** for API documentation and testing
- **xUnit** for unit and integration testing

## Planned Project Structure

```
MovieAPI/
├── MovieAPI.slnx              # Solution file
├── src/
│   ├── MovieAPI.Api/           # API project (controllers, DTOs, configuration)
│   ├── MovieAPI.Domain/         # Entities and core business logic
│   ├── MovieAPI.Infrastructure/ # EF Core, repositories, external services
│   └── MovieAPI.Application/    # Services, validators, mapping profiles
└── tests/
    ├── MovieAPI.UnitTests/
    └── MovieAPI.IntegrationTests/
```

## Getting Started

> These steps will be fleshed out once the initial project scaffolding is in place.

1. Clone the repository
2. Open `MovieAPI.slnx` in your IDE of choice (Visual Studio, Rider, or VS Code)
3. Restore dependencies: `dotnet restore`
4. Apply database migrations: `dotnet ef database update`
5. Run the API: `dotnet run --project src/MovieAPI.Api`
6. Browse the API docs at `https://localhost:<port>/swagger`

## API Overview (Draft)

| Resource  | Endpoints                                              |
|-----------|---------------------------------------------------------|
| Movies    | `GET /api/movies`, `GET /api/movies/{id}`, `POST /api/movies`, `PUT /api/movies/{id}`, `DELETE /api/movies/{id}` |
| People    | `GET /api/people`, `GET /api/people/{id}`              |
| Genres    | `GET /api/genres`                                      |
| Reviews   | `GET /api/movies/{id}/reviews`, `POST /api/movies/{id}/reviews` |

## License

This project is released under the [CC0 1.0 Universal](LICENSE) license — public domain dedication.
