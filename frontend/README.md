# MovieAPI Frontend

Next.js client for [MovieAPI](../README.md): browse movies, people, and genres, read and write reviews, and sign in against the backend's OAuth2 endpoint. See the [root README](../README.md) for the project as a whole and the [backend README](../backend/README.md) for the API it talks to.

## Status

A full-featured catalog browser and review app, server-rendered against the backend's `v3` API. Movies, People, and Genres all have paginated/filterable list views, detail views, and create/edit/delete forms gated by role; Reviews can be created by any logged-in user and edited/deleted by their owner or a Moderator/Administrator. Authentication is a real OAuth2 password-grant flow against the backend, with tokens held in httpOnly cookies and transparent refresh. **Not yet built**: self-registration, password reset, and self-service account/profile pages, and any admin UI — the backend already supports all of these (see the [backend README](../backend/README.md)), but the frontend doesn't have screens for them yet; only login exists today.

## Implemented Features

### Catalog browsing

- **Movies** (`/movies`, `/movies/{id}`) — paginated list filterable by name, free-text search, genre, release year, and min/max rating; detail view with cast/crew, genres, average rating, and its reviews. Create/edit gated to PowerUser+, delete to Moderator+
- **People** (`/persons`, `/persons/{id}`) — paginated list filterable by name, genre (of movies they're credited on), and birth year; detail view with bio and movie roles. Create/edit gated to PowerUser+, delete to Moderator+
- **Genres** (`/genres`, `/genres/{id}`) — list of genre badges; detail view with a paginated list of movies in that genre. Create/edit gated to Moderator+, delete to Administrator only — the strictest gate in the app
- **Filters live in the URL** — every list's filter form submits as a GET to its own route, so filter state is just query params (shareable/bookmarkable links, no client-side filter state)
- Create/edit forms for Movies and People are modal dialogs with a cast/crew or movie-role builder (add/remove rows, submitted as JSON alongside the rest of the form)

### Reviews

- Shown on a movie's detail page, with their own filters (search text, min/max score) and a dedicated `/movies/{movieId}/{reviewId}` detail page
- Any logged-in user can create a review; **editing/deleting is restricted to the review's own author or a Moderator/Administrator** — the one place in the app that gates on *ownership* rather than pure role (see `RestrictedComponent` below)
- The author name shown on a review isn't a form field — it's whatever the backend derives from the poster's account, matching the backend's [3.1 versioning changes](../backend/README.md#api-versioning)

### Authentication

- **Login only** (`/login`) — no registration, password reset, or account-management screens exist in the UI yet, even though the backend has endpoints for all of them
- **OAuth2 password grant** — login exchanges email/password for an access + refresh token pair via the backend's `POST /connect/token` (the same OpenIddict endpoint Swagger UI uses), not a bespoke login route
- **httpOnly cookies, not localStorage** — `access_token`, `access_token_expires_at`, and `refresh_token` are set as httpOnly, `sameSite=lax` cookies (secure in production) by a server action, never exposed to client-side JS
- **Transparent refresh** — a request checks the token's expiry before firing and refreshes proactively if it's about to lapse; a `401` response triggers a reactive refresh-and-retry as a second line of defense, redirecting to `/login` only if that also fails
- **Logout** revokes the refresh token server-side (`POST /connect/token/revoke`) before clearing cookies
- The current user is fetched once server-side in the root layout (`GET /auth/me`) and seeded into a React Context, so the signed-in state is known on first paint with no client-side round trip

### Access control — `RestrictedComponent`

A single wrapper component (`src/components/auth/restrictedComponent.tsx`) gates UI on either role or ownership:

```tsx
<RestrictedComponent accessLevel="PowerUserAndAbove">...</RestrictedComponent>
<RestrictedComponent accessLevel="ModeratorAndAbove" id={review.userId}>...</RestrictedComponent>
```

`accessLevel` checks the signed-in user's role against an ordered hierarchy (`User < PowerUser < Moderator < Administrator`, plus the `LoggedIn`/`*AndAbove` shorthands); an optional `id` prop additionally passes if it matches the current user's own id, regardless of role — that's what lets a review's author edit/delete it without needing Moderator+.

### Data layer

- **Next.js Server Actions** (`"use server"`, `src/lib/actions/`) for every API call — Server Components call fetch actions directly, and client form components pass mutating actions straight into `<form action={...}>`, interpreting a `{ success, error?, issues? }` result to show inline validation errors
- **Zod schemas as the DTO layer** (`src/lib/data/models/`) — one schema per resource for reads, and a `*ForChangeDto` schema per resource for writes that mirrors the corresponding backend FluentValidation validator (documented in a comment on each). API responses are parsed and validated on the way in, not just trusted
- **`apiInteract.ts`** centralizes the fetch wrapper: injects the bearer token, defaults to `cache: "no-store"`, handles the 401-refresh-retry flow, and turns ASP.NET `ProblemDetails`/`ValidationProblemDetails` error responses into readable messages

## Tech Stack

- **Next.js 16** (App Router) + **React 19** + **TypeScript**, React Compiler enabled
- **Tailwind CSS v4** for styling (CSS-first `@theme`, no `tailwind.config.js`)
- **Zod v4** for schema validation of both API responses and form input
- **lucide-react** for icons
- **ESLint** (`eslint-config-next`, flat config) for linting
- **pnpm** as the package manager (`pnpm-lock.yaml`) — use `pnpm`, not `npm`/`yarn`, so the lockfile stays authoritative

## Project Structure

```
frontend/
├── Dockerfile                    # Production image build (see Running with Docker)
├── next.config.ts                # React Compiler, standalone output, /api/* rewrite to BACKEND_URL
├── .env                          # BACKEND_URL for local dev (committed - see Getting Started)
└── src/
    ├── app/                      # Routes: /, /movies(+[id]+[reviewId]), /persons(+[id]), /genres(+[id]),
    │                              #   /login, /cookie-policy, /licensing
    ├── components/                # auth/, movies/, persons/, genres/, reviews/, general/ (nav, pagination,
    │                              #   delete button, RestrictedComponent)
    ├── context/                   # commonContext.tsx - AuthContext/useAuth (user, hasAccess, login, logout)
    ├── hooks/                     # useDismissableMenu - shared outside-click/Escape-to-close hook
    └── lib/
        ├── actions/                # "use server" - apiInteract.ts (fetch/auth wrapper), auth.ts, and one
        │                          #   file per resource (movie/person/genre/review) with its CRUD actions
        └── data/
            ├── consts/             # BACKEND_URL/API_BASE_URL, shared Tailwind class strings, nav menu data
            ├── interfaces/         # AccessLevel, search-option shapes, shared error/API types
            └── models/             # Zod schemas + inferred types + validate*() helpers, one file per DTO family
```

## Getting Started

```bash
pnpm install
pnpm dev
```

Open [http://localhost:3000](http://localhost:3000) — it hot-reloads on changes.

The app needs the backend running to show any real data (see the [backend README](../backend/README.md)). It reads a single environment variable, `BACKEND_URL`, which a committed `.env` already sets to `http://localhost:5201` — matching the backend's default plain-HTTP dev port. Point it elsewhere (e.g. the backend's HTTPS port, or a Docker Compose service name) by editing `.env` or overriding the variable in your shell.

## Scripts

- `pnpm dev` — start the dev server
- `pnpm build` — production build
- `pnpm start` — serve the production build
- `pnpm lint` — run ESLint

## Running with Docker

The `Dockerfile` builds a production image: a pnpm-based multi-stage build (install → `next build` with `output: "standalone"` → a slim `bun` runtime image), running as the base image's non-root `bun` user on port 3000. `BACKEND_URL` must be supplied as a **build** argument, not a runtime environment variable — `next.config.ts`'s `rewrites()` is resolved once when `next build` runs, not re-read by the standalone server at request time:

```bash
docker build -t movieapi-frontend -f frontend/Dockerfile --build-arg BACKEND_URL=http://api:8080 .
docker run -p 3000:3000 movieapi-frontend
```

The repo-root `docker-compose.yml` builds and wires this up automatically alongside the API and its dependencies — see [Running with Docker](../backend/README.md#running-with-docker) in the backend README for the one-command demo stack.

## License

Released under the [CC0 1.0 Universal](../LICENSE) license — public domain dedication.
