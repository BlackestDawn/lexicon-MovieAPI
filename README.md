# MovieAPI

A RESTful Web API built with ASP.NET Core for browsing and managing movie data — think a small-scale IMDB clone. The API exposes information about movies, people (actors/directors), genres, and user ratings/reviews, secured behind a JWT-based authentication and role-based authorization system.

> **Status:** All four catalog resources — Movies, People, Genres, and Reviews — are implemented end-to-end with full CRUD (controller, service, validation, DTO mapping, unit tests, integration tests). The API is versioned via URL segment (`/api/v1/...`, `/api/v2/...`); v2 currently differs from v1 only in the People resource, which renamed `firstName` to `givenName` and added an optional `middleName` — requests with no version specified default to v1. Authentication and authorization are built on ASP.NET Core Identity: registration/login issue short-lived JWT access tokens plus rotating refresh tokens, a four-tier role hierarchy (User/PowerUser/Moderator/Administrator) gates every write endpoint, and Administrators get a full CRUD set for managing other accounts. Password recovery, self-service password change, and per-request access-token revocation (via a security-stamp check, not just expiry) round out the auth story. Error handling is centralized: every service throws (`NotFoundException`, `ForbiddenException`, `AuthenticationException`, FluentValidation's `ValidationException`) instead of returning result wrapper objects or tuples, and a global `IExceptionHandler` middleware maps those to `ProblemDetails` HTTP responses. GET endpoints across the catalog resources are response-cached via ASP.NET Core's output caching middleware, backed by Redis in Production and an in-memory store elsewhere.

## Implemented Features

### Catalog (Movies, People, Genres, Reviews)

- **Movies** — Full CRUD: list with filtering/pagination, get by ID, create, full update (PUT), partial update (PATCH via JSON Patch), delete
- **People** — Full CRUD for actors/directors/crew, with filtering by name/genre/year and optional filmography inclusion. `v2` named the person a `givenName` and added an optional `middleName`; `v1` still exposes the original `firstName` field (see [API Versioning](#api-versioning))
- **Reviews** — Full CRUD scoped to a movie (`/api/v1/movies/{movieId}/reviews`), with filtering by search text and score range. Each review is tied to the authenticated user who created it; updating or deleting someone else's review requires Moderator/Administrator privileges
- **Genres** — Full CRUD: list, get by ID (optional `includeMovies`), create, full update (PUT), partial update (PATCH via JSON Patch), delete
- **Filtering** — Movies by name, free-text search (title + plot), genre, release year, minimum rating; People by name, genre, year; Reviews by search text and min/max score
- **Pagination** — Configurable page/pageSize with metadata returned in `X-Pagination` response header (Movies, People, Reviews, and the admin user list)
- **Validation** — FluentValidation on create/update requests for every resource, using a single shared "change" DTO/validator per resource for create and update
- **Read-only query paths** — Each repository exposes both a tracked and a `AsNoTracking` read-only variant of its GET methods; read (GET) endpoints use the read-only versions, write flows use the tracked versions
- **DTO Mapping** — AutoMapper profiles for movies, people, genres, reviews, and auth-related user DTOs
- **Domain Tracking** — `CreatedAt` / `UpdatedAt` auto-managed via an EF Core save interceptor
- **Dev Seeding** — Database is seeded with sample movie/genre/person/review data in the Development environment
- **Response Caching** — GET endpoints for the catalog resources are output-cached under a single shared cache tag (`catalog`), since their "extended"/detail DTOs embed each other's data (e.g. movies embed genres/cast/reviews, genres embed movies). Any write through any of the catalog controllers evicts the shared tag. The store is in-memory in Development/Testing and Redis-backed in Production (`AddStackExchangeRedisOutputCache`, configured via the `redis` connection string). Authenticated requests are never cached — ASP.NET Core's output cache middleware refuses to cache (or read from cache) any request carrying an `Authorization` header, by design

### Authentication & Authorization

- **Identity-backed accounts** — Built on `Microsoft.AspNetCore.Identity` (`AddIdentityCore`, EF Core stores) rather than a hand-rolled user table. `ApplicationUser`/`ApplicationRole` extend Identity's base types with `CreatedAt`/`UpdatedAt` tracking
- **Roles** — Four tiers, each cumulative with the one before it:
  | Role | Can do |
  |---|---|
  | `User` | Create reviews; update/delete their own reviews |
  | `PowerUser` | Everything User can, plus create/update Movies and People |
  | `Moderator` | Everything PowerUser can, plus delete Movies/People, create/update Genres, and update/delete *any* user's review (not just their own) |
  | `Administrator` | Everything Moderator can, plus delete Genres and full user management (see below) |

  Every new registration is assigned `User` automatically. GET endpoints on the catalog resources remain anonymous-accessible regardless of role.
- **JWT access tokens** — Short-lived (`Jwt:ExpiryMinutes`, default 60 min), signed with `Jwt:Key`, carrying the user's id, email, role, and a security-stamp claim (see below)
- **Refresh tokens** — Long-lived (`Jwt:RefreshTokenExpiryDays`, default 7 days), stored hashed (SHA-256) in the database, never the raw value. `POST /api/v1/auth/refresh` rotates the token on every use — the one just used is revoked and a new one issued. Presenting an already-rotated (i.e. revoked) token is treated as a sign of theft: every active refresh token for that user is revoked, not just the one being replayed
- **Per-request access-token revocation** — Each access token embeds the user's current Identity `SecurityStamp` as a claim. Every authenticated request re-fetches the user and compares stamps; a mismatch (e.g. because the password changed since the token was issued) rejects the request immediately, even though the token's own signature/expiry are still otherwise valid. This is a real per-request database check, not a cache, by design — closing a gap that pure refresh-token revocation can't: a stolen or stale *access* token would otherwise keep working until it naturally expired
- **Self-service account management** (`/api/v1/auth`, authenticated user acting on their own account):
  - `PUT /me` — update own email
  - `PUT /me/password` — change password (requires current password); revokes all of that user's refresh tokens on success
  - `POST /logout` — revoke a specific refresh token (the one supplied in the request body), logging out that session only — other devices/sessions stay logged in
- **Password recovery** (`/api/v1/auth`, anonymous):
  - `POST /forgot-password` — always returns 204 whether or not the email exists, so it can't be used to enumerate accounts. Generates an Identity password-reset token and hands it to `IEmailSender`
  - `POST /reset-password` — takes email + token + new password; revokes all of that user's refresh tokens on success
  - `IEmailSender`'s only implementation right now (`LoggingEmailSender`) just logs the reset token instead of emailing it — there's no SMTP/SendGrid integration configured. The abstraction exists so a real provider can be dropped in later without touching `AuthService`
- **Admin user management** (`/api/v1/admin/users`, Administrator-only — including GET, since it exposes emails and role assignments) — full CRUD: list (paginated), get by id, create (with a chosen role), update (email + role), delete. An Administrator cannot delete or demote their own account through these endpoints, to avoid a state with zero administrators left
- **Seeded admin account** — `AdminUserSeeder` creates a default Administrator account at startup, but only if `Seed:AdminEmail`/`Seed:AdminPassword` are configured (set in `appsettings.Development.json`; intentionally unset by default elsewhere, so Production doesn't get a default admin with a known password unless someone opts in). Idempotent — never overwrites an existing account
- **Swagger "Authorize" support** — paste an access token into Swagger UI's Authorize button to exercise protected endpoints from the docs page directly

### API Versioning

- **URL-segment versioning** (`Asp.Versioning.Mvc`) — every route is shaped `/api/v{version}/...` (e.g. `/api/v1/people`, `/api/v2/people`)
- **Unversioned requests default to v1** — `/api/people` (no version in the URL, query string, or header) is rewritten to `/api/v1/people` by middleware before routing runs, so clients that never adopt versioning keep working unchanged. See the path-rewrite middleware in `Program.cs` for why this couldn't be a second route on the v1 controllers (ASP.NET Core requires every attribute route sharing a `Name` to resolve to one identical template, which collides with `CreatedAtRoute`'s use of route names)
- **v1 → v2 changes are scoped to People** — `firstName` became `givenName`, plus a new optional `middleName`. The two versions share the same domain entity, persistence, and validation; v1's controller and DTOs (`Models/V1`) are a thin translation layer over the v2/canonical shape, not a parallel implementation
- **Movies inherits the split for its embedded cast/crew** — a single movie's detail view embeds People data, so `v1`'s response still shows `firstName` per cast member while `v2` shows `givenName`/`middleName`; the movie list view carries no person data and is identical between versions
- **Genres, Reviews, Admin, and Auth are version-neutral** — one controller and one set of DTOs answer both `v1` and `v2` requests, since nothing about those resources changed between versions
- **Swagger UI** exposes one OpenAPI document per version — use the dropdown in the top-right of the docs page to switch between `v1` and `v2`

### Cross-cutting

- **Centralized error handling** — Services signal failure by throwing (`NotFoundException`, `ForbiddenException`, `AuthenticationException`, FluentValidation's `ValidationException`) instead of returning result-wrapper objects or `(bool, string?)` tuples. A global `IExceptionHandler` middleware (`GlobalExceptionHandler`) catches these and returns RFC 7807 `ProblemDetails` responses with the appropriate status code (401/403/404/400/500); unexpected exceptions are logged server-side and returned without leaking internal details
- **Structured Logging** — Serilog replaces the default logging provider, with per-request logging (`UseSerilogRequestLogging`) and startup failures captured by a bootstrap logger. Development logs at `Debug` to the console and a rolling daily file under `logs/`; Production logs at `Information` to the console and ships structured events to an external Elasticsearch cluster (configured via the `Elasticsearch:Uri` setting, required and fail-fast in Production, matching the Redis connection-string pattern)
- **Unit Tests** — xUnit + Moq tests covering every service (Movie, Person, Genre, Review, Auth, AdminUser) and every validator
- **Integration Tests** — xUnit + `WebApplicationFactory` tests covering full CRUD for every controller against a real SQL Server instance spun up via Testcontainers, with Respawn resetting the database between tests (except seeded reference data like roles), plus dedicated tests for output cache hits/invalidation, role-boundary enforcement, refresh-token rotation/theft-detection, password recovery, access-token revocation via the security-stamp check, the v2-specific People/Movies behavior, and unversioned requests defaulting to v1

## Tech Stack

- **ASP.NET Core Web API** (.NET 10)
- **API Versioning** (`Asp.Versioning.Mvc`, `Asp.Versioning.Mvc.ApiExplorer`) for URL-segment versioning, integrated with Swagger's per-version documents
- **Entity Framework Core** with SQL Server
- **ASP.NET Core Identity** (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`) for user/role storage
- **JWT Bearer authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`)
- **AutoMapper** for DTO mapping
- **FluentValidation** for request validation
- **Swagger / OpenAPI** (Swashbuckle + `Microsoft.AspNetCore.OpenApi`) for API docs, with JWT bearer auth wired into the UI
- **ASP.NET Core Output Caching**, with `Microsoft.AspNetCore.OutputCaching.StackExchangeRedis` for the Production store
- **Serilog** (`Serilog.AspNetCore`) for structured logging, with `Serilog.Sinks.Elasticsearch` as the external log store in Production
- **xUnit + Moq** for unit testing
- **xUnit + Testcontainers + Respawn** for integration testing

## Project Structure

```
MovieAPI/
├── MovieAPI.slnx
├── src/
│   ├── MovieAPI.Api/              # Controllers (V1/, V2/ subfolders for resources with version-specific shapes;
│   │                              #   version-neutral resources live directly under Controllers/), program entry
│   │                              #   point (incl. the unversioned-request path-rewrite middleware), appsettings,
│   │                              #   global exception handler middleware
│   ├── MovieAPI.Application/      # Services, validators, DTOs (Models/V1/ holds the legacy v1-only Person/Movie
│   │                              #   shapes), AutoMapper profiles, custom exceptions
│   ├── MovieAPI.Domain/           # Entities (Movie, Person, Genre, Review, CastCrew, MovieDetail, ApplicationUser,
│   │                              #   ApplicationRole, RefreshToken), Constants (Roles, CustomClaimTypes)
│   └── MovieAPI.Infrastructure/   # AppDbContext, EF Fluent configs, migrations, repositories, seeders
│                                 #   (DbSeeder, RoleSeeder, AdminUserSeeder), TokenService, LoggingEmailSender
└── tests/
    ├── MovieAPI.UnitTests/        # Service tests for every resource + Auth/AdminUser, validator tests
    └── MovieAPI.IntegrationTests/ # WebApplicationFactory tests for every controller (Testcontainers + Respawn)
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
   The same file already has dev-only defaults for the `Jwt` section (signing key, issuer/audience, token lifetimes) and a `Seed` section (`AdminEmail`/`AdminPassword`) that bootstraps a default Administrator account on first run — no extra setup needed locally.
3. Apply database migrations:
   ```
   dotnet ef database update --project src/MovieAPI.Infrastructure --startup-project src/MovieAPI.Api
   ```
4. Run the API:
   ```
   dotnet run --project src/MovieAPI.Api
   ```
5. Browse the API docs at `https://localhost:<port>/swagger` — use the dropdown in the top-right to switch between the `v1` and `v2` OpenAPI documents. Register a user (or log in with the seeded admin account from `appsettings.Development.json`) via `/api/v1/auth/register` or `/api/v1/auth/login`, then paste the returned `accessToken` into Swagger's **Authorize** button to call protected endpoints from the docs page.

The database is automatically seeded with sample catalog data when running in the Development environment. Roles (`User`, `PowerUser`, `Moderator`, `Administrator`) and the default admin account are seeded in every environment except `Testing`, where the integration test factory seeds them itself after migrations run.

Output caching uses an in-memory store in Development, so no extra setup is needed locally. Running with `ASPNETCORE_ENVIRONMENT=Production` requires a `redis` connection string (e.g. `ConnectionStrings__redis=localhost:6379`) — the app fails fast at startup if it's missing. Production also needs its own `Jwt:Key`/`Jwt:Issuer`/`Jwt:Audience` (the committed Development values are not safe to reuse) and, if a seeded admin account is wanted there too, its own `Seed:AdminEmail`/`Seed:AdminPassword` — both unset by default outside Development.

Logging similarly needs no extra setup in Development (console + a rolling file under `logs/`). Production additionally requires an `Elasticsearch:Uri` setting (e.g. `Elasticsearch__Uri=http://localhost:9200`) pointing at an Elasticsearch cluster — the app fails fast at startup if it's missing. Logs are shipped to a `movieapi-logs-{yyyy.MM}` index, viewable in Kibana.

### Running Tests

```
dotnet test
```

Unit tests run with no external dependencies. Integration tests require a running Docker daemon — they spin up a disposable SQL Server container via Testcontainers for each test run.

## API Overview

Every route below is versioned (`/api/v1/...`, shown here; `/api/v2/...` also exists — see [API Versioning](#api-versioning)). Requests with no version specified (e.g. `/api/auth/login`) default to `v1`.

### Auth (`/api/v1/auth`) — anonymous unless noted

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/register` | Anonymous | Create an account (assigned the `User` role) and return access + refresh tokens |
| `POST` | `/login` | Anonymous | Authenticate with email/password, return access + refresh tokens |
| `POST` | `/refresh` | Anonymous | Exchange a refresh token for a new access + refresh token pair (rotates the old one) |
| `POST` | `/logout` | Authenticated | Revoke the refresh token supplied in the body (that session only) |
| `PUT` | `/me` | Authenticated | Update own email |
| `PUT` | `/me/password` | Authenticated | Change own password (requires current password) |
| `POST` | `/forgot-password` | Anonymous | Request a password reset token (logged server-side; see `LoggingEmailSender`) |
| `POST` | `/reset-password` | Anonymous | Reset password using email + token from `forgot-password` |

### Admin (`/api/v1/admin/users`) — Administrator only, including GET

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | List users, paginated (`page`, `pageSize`) |
| `GET` | `/{id}` | Get a single user |
| `POST` | `/` | Create a user with a chosen role |
| `PUT` | `/{id}` | Update a user's email/role |
| `DELETE` | `/{id}` | Delete a user |

### Catalog

People routes also exist under `/api/v2/...` with `givenName`/`middleName` replacing `firstName` (see [API Versioning](#api-versioning)); every other route below is identical between `v1` and `v2`.

| Method   | Route                                   | Auth | Description                                      |
|----------|------------------------------------------|------|--------------------------------------------------|
| `GET`    | `/api/v1/movies`                        | Anonymous | List movies (filter by `name`, `search`, `genre`, `year`, `minRating`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/v1/movies/{id}`                   | Anonymous | Get a single movie by ID (optional `includePeople` query param) |
| `POST`   | `/api/v1/movies`                        | PowerUser+ | Create a new movie                               |
| `PUT`    | `/api/v1/movies/{id}`                   | PowerUser+ | Full update of a movie                           |
| `PATCH`  | `/api/v1/movies/{id}`                   | PowerUser+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/movies/{id}`                   | Moderator+ | Delete a movie                                   |
| `GET`    | `/api/v1/people`                        | Anonymous | List people (filter by `name`, `genre`, `year`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/v1/people/{id}`                   | Anonymous | Get a single person by ID (optional `includeMovies` query param) |
| `POST`   | `/api/v1/people`                        | PowerUser+ | Create a new person                              |
| `PUT`    | `/api/v1/people/{id}`                   | PowerUser+ | Full update of a person                          |
| `PATCH`  | `/api/v1/people/{id}`                   | PowerUser+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/people/{id}`                   | Moderator+ | Delete a person                                  |
| `GET`    | `/api/v1/genres`                        | Anonymous | List genres                                      |
| `GET`    | `/api/v1/genres/{id}`                   | Anonymous | Get a single genre by ID (optional `includeMovies` query param) |
| `POST`   | `/api/v1/genres`                        | Moderator+ | Create a new genre                               |
| `PUT`    | `/api/v1/genres/{id}`                   | Moderator+ | Full update of a genre                           |
| `PATCH`  | `/api/v1/genres/{id}`                   | Moderator+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/genres/{id}`                   | Administrator | Delete a genre                                   |
| `GET`    | `/api/v1/movies/{movieId}/reviews`      | Anonymous | List reviews for a movie (filter by `search`, `minScore`, `maxScore`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/v1/movies/{movieId}/reviews/{id}` | Anonymous | Get a single review                              |
| `POST`   | `/api/v1/movies/{movieId}/reviews`      | Authenticated | Create a review for a movie                      |
| `PUT`    | `/api/v1/movies/{movieId}/reviews/{id}` | Owner or Moderator+ | Full update of a review                          |
| `PATCH`  | `/api/v1/movies/{movieId}/reviews/{id}` | Owner or Moderator+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/movies/{movieId}/reviews/{id}` | Owner or Moderator+ | Delete a review                                  |

## License

This project is released under the [CC0 1.0 Universal](LICENSE) license — public domain dedication.
