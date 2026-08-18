# MovieAPI Backend

ASP.NET Core Web API (.NET 10) for the [MovieAPI](../README.md) catalog: movies, people, genres, and reviews, behind OAuth2-based authentication and four-tier role authorization. See the [root README](../README.md) for the project as a whole and the [frontend README](../frontend/README.md) for the Next.js client.

## Status

All four catalog resources — Movies, People, Genres, and Reviews — are implemented end-to-end with full CRUD (controller, service, validation, DTO mapping, unit tests, integration tests). The API is versioned via URL segment (`/api/v1/...` through `/api/v3.1/...`); requests with no version specified default to v1 — see [API Versioning](#api-versioning) for what changed at each step. Authentication is OAuth2 (OpenIddict) backed by ASP.NET Core Identity: a password grant issues short-lived access tokens plus rotating refresh tokens, a four-tier role hierarchy (User/PowerUser/Moderator/Administrator) gates every write endpoint, and Administrators get a full CRUD set for managing other accounts. Password recovery, self-service profile updates, and per-request access-token revocation (via a security-stamp check, not just expiry) round out the auth story. Error handling is centralized: every service throws (`NotFoundException`, `ForbiddenException`, `AuthenticationException`, FluentValidation's `ValidationException`) instead of returning result wrapper objects or tuples, and a global `IExceptionHandler` middleware maps those to `ProblemDetails` HTTP responses. GET endpoints across the catalog resources are response-cached via ASP.NET Core's output caching middleware, backed by Redis in Production and an in-memory store elsewhere.

## Implemented Features

### Catalog (Movies, People, Genres, Reviews)

- **Movies** — Full CRUD: list with filtering/pagination, get by ID, create, full update (PUT), partial update (PATCH via JSON Patch), delete
- **People** — Full CRUD for actors/directors/crew, with filtering by name/genre/birth year and optional filmography inclusion. `v2` renamed the person's name field to `givenName` and added an optional `middleName`; `v3` renamed the route from `/people` to `/persons` (`v1`/`v2` still expose `/people` — see [API Versioning](#api-versioning))
- **Reviews** — Full CRUD scoped to a movie (`/api/v{version}/movies/{movieId}/reviews`), with filtering by search text and score range. Each review is tied to the authenticated user who created it (`ReviewDto.userId`); `authorName` isn't client-supplied — it's derived from that user's `DisplayName`, refreshed on every edit (from the review's *owner*, not necessarily whoever is editing it, so a Moderator correcting someone else's review doesn't reattribute it). Updating or deleting someone else's review requires Moderator/Administrator privileges
- **Genres** — Full CRUD: list, get by ID (optional `includeMovies`, paginated with `page`/`pageSize`), create, full update (PUT), partial update (PATCH via JSON Patch), delete
- **Filtering** — Movies by name, free-text search (title + plot), genre, release year, minimum/maximum rating; People by name, genre, birth year; Reviews by search text and min/max score
- **Pagination** — Configurable page/pageSize with metadata returned in `X-Pagination` response header (Movies, People, Reviews, the admin user list, and the movie list embedded in a single Genre)
- **Validation** — FluentValidation on create/update requests for every resource, using a single shared "change" DTO/validator per resource for create and update
- **Read-only query paths** — Each repository exposes both a tracked and an `AsNoTracking` read-only variant of its GET methods; read (GET) endpoints use the read-only versions, write flows use the tracked versions
- **DTO Mapping** — AutoMapper profiles for movies, people, genres, reviews, and auth-related user DTOs
- **Domain Tracking** — `CreatedAt` / `UpdatedAt` auto-managed via an EF Core save interceptor
- **Dev Seeding** — Database is seeded with sample movie/genre/person/review data in the Development environment
- **Response Caching** — GET endpoints for the catalog resources are output-cached under a single shared cache tag (`catalog`), since their "extended"/detail DTOs embed each other's data (e.g. movies embed genres/cast/reviews, genres embed movies). Any write through any of the catalog controllers evicts the shared tag. The store is in-memory in Development/Testing and Redis-backed in Production (`AddStackExchangeRedisOutputCache`, configured via the `redis` connection string). Authenticated requests are never cached — ASP.NET Core's output cache middleware refuses to cache (or read from cache) any request carrying an `Authorization` header, by design

### Authentication & Authorization

- **Identity-backed accounts** — Built on `Microsoft.AspNetCore.Identity` (`AddIdentityCore`, EF Core stores) rather than a hand-rolled user table. `ApplicationUser`/`ApplicationRole` extend Identity's base types with `CreatedAt`/`UpdatedAt` tracking and a `DisplayName`
- **Roles** — Four tiers, each cumulative with the one before it:
  | Role | Can do |
  |---|---|
  | `User` | Create reviews; update/delete their own reviews |
  | `PowerUser` | Everything User can, plus create/update Movies and People |
  | `Moderator` | Everything PowerUser can, plus delete Movies/People, create/update Genres, and update/delete *any* user's review (not just their own) |
  | `Administrator` | Everything Moderator can, plus delete Genres and full user management (see below) |

  Every new registration is assigned `User` automatically. GET endpoints on the catalog resources remain anonymous-accessible regardless of role.
- **OAuth2 via OpenIddict** — Login and token refresh go through a standard OAuth2 token endpoint (`POST /connect/token`) rather than a hand-rolled JWT login route: `grant_type=password` exchanges email/password for an access + refresh token pair, `grant_type=refresh_token` exchanges a refresh token for a new pair. `POST /connect/token/revoke` revokes a token (used for logout). All three require `client_id=movieapi-client` (a public OAuth client seeded at startup — see `OpenIddictClientSeeder`); access tokens carry the user's id, email, role, and a security-stamp claim (see below)
- **Refresh tokens** — Rolling/single-use by default (OpenIddict's built-in behavior, not custom code): redeeming one immediately revokes it and issues a new one (`Jwt:RefreshTokenLifetimeDays`-equivalent is `OpenIddict:RefreshTokenLifetimeDays`, default 7 days). Replaying an already-redeemed refresh token is treated as theft and automatically revokes every other token tied to that authorization — `SetRefreshTokenReuseLeeway(TimeSpan.Zero)` removes OpenIddict's default 30-second grace window so this is zero-tolerance
- **Per-request access-token revocation** — Each access token embeds the user's current Identity `SecurityStamp` as a claim. A custom `ValidateSecurityStampHandler` re-fetches the user on every authenticated request and compares stamps; a mismatch (e.g. because the password changed since the token was issued) rejects the request immediately, even though the token's own signature/expiry are still otherwise valid. This is a real per-request database check, not a cache, by design — closing a gap that pure refresh-token revocation can't: a stolen or stale *access* token would otherwise keep working until it naturally expired
- **Self-service account management** (`/api/v{version}/auth`, authenticated user acting on their own account):
  - `GET /me` — fetch own profile
  - `PUT /me` — update own email and/or display name
  - `PUT /me/password` — change password (requires current password); revokes all of that user's refresh tokens on success
- **Password recovery** (`/api/v{version}/auth`, anonymous):
  - `POST /forgot-password` — always returns 204 whether or not the email exists, so it can't be used to enumerate accounts. Generates an Identity password-reset token and hands it to `IEmailSender`
  - `POST /reset-password` — takes email + token + new password; revokes all of that user's refresh tokens on success
  - `IEmailSender`'s only implementation right now (`LoggingEmailSender`) just logs the reset token instead of emailing it — there's no SMTP/SendGrid integration configured. The abstraction exists so a real provider can be dropped in later without touching `AuthService`
- **Display names** — `RegisterDto`/`UserForUpdateDto`/`AdminUserForCreationDto`/`AdminUserForUpdateDto` all take an optional `displayName`. Left unset on create, it falls back to the local part of the email (e.g. `ada@example.com` → `ada`); left unset on update, the existing value is untouched. Reviews use this as their `authorName` (see Catalog above)
- **Admin user management** (`/api/v{version}/admin/users`, Administrator-only — including GET, since it exposes emails and role assignments) — full CRUD: list (paginated), get by id, create (with a chosen role), update (email/role/display name), delete. An Administrator cannot delete or demote their own account through these endpoints, to avoid a state with zero administrators left
- **Seeded admin account** — `AdminUserSeeder` creates a default Administrator account at startup, but only if `Seed:AdminEmail`/`Seed:AdminPassword` are configured (set in `appsettings.Development.json`; intentionally unset by default elsewhere, so Production doesn't get a default admin with a known password unless someone opts in). Idempotent — never overwrites an existing account
- **Swagger "Authorize" support** — Swagger UI's Authorize dialog does a real password-grant login against `/connect/token` itself, so you can authenticate from the docs page without minting a token by hand first

### API Versioning

- **URL-segment versioning** (`Asp.Versioning.Mvc`) — every route is shaped `/api/v{version}/...` (e.g. `/api/v1/people`, `/api/v3/persons`)
- **Unversioned requests default to v1** — `/api/people` (no version in the URL, query string, or header) is rewritten to `/api/v1/people` by middleware before routing runs, so clients that never adopt versioning keep working unchanged. See the path-rewrite middleware in `Program.cs` for why this couldn't be a second route on the v1 controllers (ASP.NET Core requires every attribute route sharing a `Name` to resolve to one identical template, which collides with `CreatedAtRoute`'s use of route names)
- **v1 → v2** — scoped to People: `firstName` became `givenName`, plus a new optional `middleName`. The two versions share the same domain entity, persistence, and validation; v1's controller and DTOs (`Models/V1`) are a thin translation layer over the v2/canonical shape, not a parallel implementation. Movies inherits the split for its embedded cast/crew (a movie's detail view embeds People data), so v1's response still shows `firstName` per cast member while v2 shows `givenName`/`middleName`
- **v2 → v3** — People's route (and the query param naming around it) changed from `/people` to `/persons`, and Movies' `includePeople` query param became `includePersons` to match; otherwise identical to v2
- **v3 → 3.1 — a non-breaking marker, not a fork** — 3.1 exists to signal "something changed here, but nothing broke" without the cost of a parallel implementation. Concretely: `[ApiVersion("3.1")]` is stacked onto the *same* controller class as `[ApiVersion("3.0")]` for every resource that gained something additive since 3.0 shipped (Persons, Movies, Reviews, Auth, AdminUsers) — one implementation answers both version numbers identically. `GenresController` was left off 3.1 since nothing about Genres changed. What actually landed under 3.1: Reviews gained `userId` and account-linked `authorName` (see Catalog above), Movies gained a `maxRating` filter, and Auth/AdminUsers gained `displayName`. An earlier attempt at this used a forked `V4/PersonsController` instead — abandoned once it became clear every change so far was safe to apply directly to the shared v1–v3 code path, making a full parallel version pure maintenance overhead with no behavioral payoff
- **Genres, Reviews, Admin, and Auth are version-neutral controllers** — one controller class and one set of DTOs answers every version it's tagged with (`[ApiVersion]` stacked, no forked implementation), since nothing about those resources' *shape* changed between the versions they support. This is the same mechanism 3.1 uses everywhere, just also used historically for 1.0–3.0 on these four
- **Swagger UI** exposes one OpenAPI document per version — use the dropdown in the top-right of the docs page to switch between them

### Cross-cutting

- **Centralized error handling** — Services signal failure by throwing (`NotFoundException`, `ForbiddenException`, `AuthenticationException`, FluentValidation's `ValidationException`) instead of returning result-wrapper objects or `(bool, string?)` tuples. A global `IExceptionHandler` middleware (`GlobalExceptionHandler`) catches these and returns RFC 7807 `ProblemDetails` responses with the appropriate status code (401/403/404/400/500); unexpected exceptions are logged server-side and returned without leaking internal details
- **Structured Logging** — Serilog replaces the default logging provider, with per-request logging (`UseSerilogRequestLogging`) and startup failures captured by a bootstrap logger. Development logs at `Debug` to the console and a rolling daily file under `logs/`; Production logs at `Information` to the console and ships structured events to an external Elasticsearch cluster (configured via the `Elasticsearch:Uri` setting, required and fail-fast in Production, matching the Redis connection-string pattern)
- **Unit Tests** — xUnit + Moq tests covering every service (Movie, Person, Genre, Review, Auth, AdminUser) and every validator
- **Integration Tests** — xUnit + `WebApplicationFactory` tests covering full CRUD for every controller against a real SQL Server instance spun up via Testcontainers, with Respawn resetting the database between tests (except seeded reference data like roles), plus dedicated tests for output cache hits/invalidation, role-boundary enforcement, refresh-token rotation/theft-detection, password recovery, access-token revocation via the security-stamp check, version-specific People/Movies/Reviews behavior at each step above, the 3.1 marker version resolving where expected (and not where it shouldn't), and unversioned requests defaulting to v1

## Tech Stack

- **ASP.NET Core Web API** (.NET 10)
- **API Versioning** (`Asp.Versioning.Mvc`, `Asp.Versioning.Mvc.ApiExplorer`) for URL-segment versioning, integrated with Swagger's per-version documents
- **Entity Framework Core** with SQL Server
- **ASP.NET Core Identity** (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`) for user/role storage
- **OpenIddict** (`OpenIddict.AspNetCore`, `OpenIddict.EntityFrameworkCore`) for the OAuth2 token/revocation endpoints (password + refresh_token grants) backing authentication
- **AutoMapper** for DTO mapping
- **FluentValidation** for request validation
- **Swagger / OpenAPI** (Swashbuckle + `Microsoft.AspNetCore.OpenApi`) for API docs, with an OAuth2 password-flow Authorize dialog wired into the UI. Every controller action has an XML doc comment (`<summary>`/`<param>`/`<returns>`) describing what it does, its parameters, and its response, surfaced in Swagger UI via `GenerateDocumentationFile` + `IncludeXmlComments`
- **ASP.NET Core Output Caching**, with `Microsoft.AspNetCore.OutputCaching.StackExchangeRedis` for the Production store
- **Serilog** (`Serilog.AspNetCore`) for structured logging, with `Serilog.Sinks.Elasticsearch` as the external log store in Production
- **xUnit + Moq** for unit testing
- **xUnit + Testcontainers + Respawn** for integration testing
- **Docker / Docker Compose** — a production `Dockerfile` plus a repo-root `docker-compose.yml` demo stack (API + frontend + SQL Server + Redis + Elasticsearch), see [Running with Docker](#running-with-docker)

## Project Structure

```
backend/
├── MovieAPI.slnx
├── Dockerfile                      # Production image build (see Running with Docker)
├── src/
│   ├── MovieAPI.Api/                # Controllers (V1/, V2/, V3/ subfolders for Movies/Persons, which have
│   │                                #   version-specific route/DTO shapes; version-neutral resources - Genres,
│   │                                #   Reviews, Auth, AdminUsers - live directly under Controllers/ with
│   │                                #   multiple [ApiVersion] attributes stacked on one class), program entry
│   │                                #   point (incl. the unversioned-request path-rewrite middleware), appsettings,
│   │                                #   global exception handler middleware, OAuth2 AuthorizationController
│   ├── MovieAPI.Application/         # Services, validators, DTOs (Models/V1/ holds the legacy v1-only Person/Movie
│   │                                #   shapes), AutoMapper profiles, custom exceptions
│   ├── MovieAPI.Domain/              # Entities (Movie, Person, Genre, Review, CastCrew, MovieDetail, ApplicationUser,
│   │                                #   ApplicationRole), Constants (Roles, CustomClaimTypes)
│   └── MovieAPI.Infrastructure/      # AppDbContext, EF Fluent configs, migrations, repositories, seeders
│                                    #   (DbSeeder, RoleSeeder, AdminUserSeeder, OpenIddictClientSeeder), LoggingEmailSender
└── tests/
    ├── MovieAPI.UnitTests/          # Service tests for every resource + Auth/AdminUser, validator tests
    └── MovieAPI.IntegrationTests/   # WebApplicationFactory tests for every controller (Testcontainers + Respawn)
```

## Getting Started

1. Clone the repository
2. Set the SQL Server connection string in `backend/src/MovieAPI.Api/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "sqlserver": "Server=...;Database=MovieAPI;..."
     }
   }
   ```
   The same file already has dev-only defaults for the `OpenIddict` section (access/refresh token lifetimes) and a `Seed` section (`AdminEmail`/`AdminPassword`) that bootstraps a default Administrator account on first run — no extra setup needed locally.
3. Apply database migrations:
   ```
   dotnet ef database update --project backend/src/MovieAPI.Infrastructure --startup-project backend/src/MovieAPI.Api
   ```
4. Run the API:
   ```
   dotnet run --project backend/src/MovieAPI.Api
   ```
5. Browse the API docs at `https://localhost:<port>/swagger` — use the dropdown in the top-right to switch between OpenAPI documents per version. Register a user (or log in with the seeded admin account from `appsettings.Development.json`) via `POST /api/v1/auth/register` then `POST /connect/token` (`grant_type=password`), or click **Authorize** in Swagger UI and let it do that exchange for you.

The database is automatically seeded with sample catalog data when running in the Development environment. Roles (`User`, `PowerUser`, `Moderator`, `Administrator`), the default admin account, and the `movieapi-client` OAuth client are seeded in every environment except `Testing`, where the integration test factory seeds them itself after migrations run.

Output caching uses an in-memory store in Development, so no extra setup is needed locally. Running with `ASPNETCORE_ENVIRONMENT=Production` requires a `redis` connection string (e.g. `ConnectionStrings__redis=localhost:6379`) — the app fails fast at startup if it's missing. Production also needs its own `OpenIddict:SigningKey`/`OpenIddict:EncryptionKey` (base64-encoded symmetric keys; the ephemeral keys used outside Production don't survive a restart, which is fine there but not in Production) and, if a seeded admin account is wanted there too, its own `Seed:AdminEmail`/`Seed:AdminPassword` — both unset by default outside Development.

Logging similarly needs no extra setup in Development (console + a rolling file under `logs/`). Production additionally requires an `Elasticsearch:Uri` setting (e.g. `Elasticsearch__Uri=http://localhost:9200`) pointing at an Elasticsearch cluster — the app fails fast at startup if it's missing. Logs are shipped to a `movieapi-logs-{yyyy.MM}` index, viewable in Kibana.

### Running Tests

```
dotnet test backend/MovieAPI.slnx
```

Unit tests run with no external dependencies. Integration tests require a running Docker daemon — they spin up a disposable SQL Server container via Testcontainers for each test run.

## Running with Docker

The `Dockerfile` builds a production image: a multi-stage build (SDK → `aspnet` runtime) that publishes only `MovieAPI.Api`, runs as the image's non-root `app` user, and listens on port 8080. It bakes in no secrets or service endpoints — everything the app already reads from configuration (`ConnectionStrings:sqlserver`, `ConnectionStrings:redis`, `Elasticsearch:Uri`, `OpenIddict:*`, `Seed:*`) is supplied at container-start time via ASP.NET Core's double-underscore environment variable convention, e.g.:

```
docker build -t movieapi -f backend/Dockerfile .
docker run -p 8080:8080 \
  -e ConnectionStrings__sqlserver="Server=tcp:...;Database=MovieAPI;..." \
  -e ConnectionStrings__redis="redis-host:6379" \
  -e Elasticsearch__Uri="http://elasticsearch-host:9200" \
  -e OpenIddict__SigningKey="..." -e OpenIddict__EncryptionKey="..." \
  movieapi
```

Since the image carries no migration step of its own, set `ApplyMigrationsOnStartup=true` if you want the container to apply pending EF Core migrations itself on boot (off by default — a real deployment normally applies migrations through its release pipeline instead, so multiple replicas don't race each other against the same database at startup).

### Demo environment (`docker-compose.yml`)

The repo-root `docker-compose.yml` runs the API and frontend images alongside real instances of every external service the API needs — SQL Server, Redis, and Elasticsearch — for a self-contained local demo:

```
docker compose up -d --build
```

The API becomes available at `http://localhost:8080` and the frontend at `http://localhost:3000` once dependencies report healthy; the API applies migrations and seeds roles/the default admin account (`admin@movieapi.local` / `Admin123!` by default) on startup. Elasticsearch runs with security disabled and everything uses throwaway demo credentials (overridable via a `.env` file — see `MSSQL_SA_PASSWORD`, `OPENIDDICT_SIGNING_KEY`, `OPENIDDICT_ENCRYPTION_KEY`, `SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD` in `docker-compose.yml`) — **this stack is for local demo/testing only**, not a template for a real deployment's secrets handling.

If port `1433`, `6379`, `9200`, `8080`, or `3000` is already in use on your machine, either stop that service first or remap the conflicting port(s) in `docker-compose.yml` — the containers talk to each other over the internal compose network regardless of what's published to the host.

## API Overview

Every route below is versioned (`/api/v1/...`, shown here — see [API Versioning](#api-versioning) for what's different in later versions). Requests with no version specified (e.g. `/api/auth/register`) default to `v1`.

### Auth (`/api/v1/auth`)

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/me` | Authenticated | Fetch own profile |
| `POST` | `/register` | Anonymous | Create an account (assigned the `User` role); log in separately afterwards |
| `PUT` | `/me` | Authenticated | Update own email and/or display name |
| `PUT` | `/me/password` | Authenticated | Change own password (requires current password) |
| `POST` | `/forgot-password` | Anonymous | Request a password reset token (logged server-side; see `LoggingEmailSender`) |
| `POST` | `/reset-password` | Anonymous | Reset password using email + token from `forgot-password` |

### OAuth2 token endpoint (unversioned — not under `/api`)

| Method | Route | Description |
|---|---|---|
| `POST` | `/connect/token` (`grant_type=password`) | Log in: exchange email/password for an access + refresh token pair |
| `POST` | `/connect/token` (`grant_type=refresh_token`) | Refresh: exchange a refresh token for a new pair (rotates the old one) |
| `POST` | `/connect/token/revoke` | Log out: revoke a specific refresh token (that session only) |

All three require `client_id=movieapi-client` in the request body.

### Admin (`/api/v1/admin/users`) — Administrator only, including GET

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | List users, paginated (`page`, `pageSize`) |
| `GET` | `/{id}` | Get a single user |
| `POST` | `/` | Create a user with a chosen role |
| `PUT` | `/{id}` | Update a user's email/role/display name |
| `DELETE` | `/{id}` | Delete a user |

### Catalog

Shown at their v1 routes; see [API Versioning](#api-versioning) for the `/people` → `/persons` rename in v3 and other version-specific differences.

| Method   | Route                                   | Auth | Description                                      |
|----------|------------------------------------------|------|--------------------------------------------------|
| `GET`    | `/api/v1/movies`                        | Anonymous | List movies (filter by `name`, `search`, `genre`, `year`, `minRating`, `maxRating`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/v1/movies/{id}`                   | Anonymous | Get a single movie by ID (optional `includePeople` query param) |
| `POST`   | `/api/v1/movies`                        | PowerUser+ | Create a new movie                               |
| `PUT`    | `/api/v1/movies/{id}`                   | PowerUser+ | Full update of a movie                           |
| `PATCH`  | `/api/v1/movies/{id}`                   | PowerUser+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/movies/{id}`                   | Moderator+ | Delete a movie                                   |
| `GET`    | `/api/v1/people`                        | Anonymous | List people (filter by `name`, `genre`, `year` — birth year; paginate with `page`, `pageSize`) |
| `GET`    | `/api/v1/people/{id}`                   | Anonymous | Get a single person by ID (optional `includeMovies` query param) |
| `POST`   | `/api/v1/people`                        | PowerUser+ | Create a new person                              |
| `PUT`    | `/api/v1/people/{id}`                   | PowerUser+ | Full update of a person                          |
| `PATCH`  | `/api/v1/people/{id}`                   | PowerUser+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/people/{id}`                   | Moderator+ | Delete a person                                  |
| `GET`    | `/api/v1/genres`                        | Anonymous | List genres                                      |
| `GET`    | `/api/v1/genres/{id}`                   | Anonymous | Get a single genre by ID, with its movies (optional `includeMovies`, defaults to true; paginate the embedded movie list with `page`, `pageSize`) |
| `POST`   | `/api/v1/genres`                        | Moderator+ | Create a new genre                               |
| `PUT`    | `/api/v1/genres/{id}`                   | Moderator+ | Full update of a genre                           |
| `PATCH`  | `/api/v1/genres/{id}`                   | Moderator+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/genres/{id}`                   | Administrator | Delete a genre                                   |
| `GET`    | `/api/v1/movies/{movieId}/reviews`      | Anonymous | List reviews for a movie (filter by `search`, `minScore`, `maxScore`; paginate with `page`, `pageSize`) |
| `GET`    | `/api/v1/movies/{movieId}/reviews/{id}` | Anonymous | Get a single review                              |
| `POST`   | `/api/v1/movies/{movieId}/reviews`      | Authenticated | Create a review for a movie (`authorName` is derived from your account, not part of the request body) |
| `PUT`    | `/api/v1/movies/{movieId}/reviews/{id}` | Owner or Moderator+ | Full update of a review                          |
| `PATCH`  | `/api/v1/movies/{movieId}/reviews/{id}` | Owner or Moderator+ | Partial update via JSON Patch                    |
| `DELETE` | `/api/v1/movies/{movieId}/reviews/{id}` | Owner or Moderator+ | Delete a review                                  |

## License

Released under the [CC0 1.0 Universal](../LICENSE) license — public domain dedication.
