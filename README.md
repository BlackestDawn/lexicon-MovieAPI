# MovieAPI

[![PR Checks](https://github.com/BlackestDawn/lexicon-MovieAPI/actions/workflows/pr-checks.yml/badge.svg)](https://github.com/BlackestDawn/lexicon-MovieAPI/actions/workflows/pr-checks.yml)
[![Backend Coverage](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/BlackestDawn/lexicon-MovieAPI/badges/.badges/backend-coverage.json)](https://github.com/BlackestDawn/lexicon-MovieAPI/actions/workflows/coverage.yml)
[![Frontend Coverage](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/BlackestDawn/lexicon-MovieAPI/badges/.badges/frontend-coverage.json)](https://github.com/BlackestDawn/lexicon-MovieAPI/actions/workflows/coverage.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Next.js](https://img.shields.io/badge/Next.js-16-black?logo=next.js&logoColor=white)
[![License: CC0](https://img.shields.io/badge/License-CC0%201.0-lightgrey.svg)](LICENSE)

A small-scale IMDB clone: an ASP.NET Core Web API for browsing and managing movies, people, genres, and reviews, paired with a Next.js frontend. The API is secured behind OAuth2-based authentication and four-tier role authorization (User / PowerUser / Moderator / Administrator).

**Live:** [movieapi.alexstauch.app](https://movieapi.alexstauch.app) — browse the catalog anonymously; signing in (existing accounts only, no self-registration in the UI yet) unlocks writing/editing reviews and, by role, managing the catalog itself.

## Status

Both halves are functional end to end. The backend has full CRUD for all four catalog resources, OAuth2 authentication (OpenIddict) with refresh-token rotation and per-request revocation, admin user management, and a versioned API currently at `v3.1`. The frontend covers browsing/filtering/pagination for all four resources plus review create/edit/delete (gated by ownership and role) and login — it doesn't yet have registration, password reset, self-service account, or admin screens, though the backend already supports all of those.

- **[Backend README](backend/README.md)** — features, auth model, API versioning history, full endpoint reference, running the API and its test suite, Docker
- **[Frontend README](frontend/README.md)** — features, architecture (Server Actions, Zod-validated DTOs, role/ownership-gated UI), running the dev server, Docker

## Tech Stack

- **Backend**: ASP.NET Core (.NET 10), Entity Framework Core + PostgreSQL, ASP.NET Core Identity + OpenIddict (OAuth2), AutoMapper, FluentValidation, Swagger/OpenAPI, Serilog, output caching (config-driven: in-memory or Redis)
- **Frontend**: Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, Zod
- **Testing**: xUnit + Moq (unit) and xUnit + Testcontainers + Respawn (integration) on the backend; Vitest + React Testing Library on the frontend, including async Server Components
- **Infra**: Docker / Docker Compose for local dev; GCP Cloud Run + Neon Postgres for deployment (see [Deployment](#deployment))

## Project Structure

```
MovieAPI/
├── docker-compose.yml   # Local demo stack: frontend + API + Postgres + Redis + Elasticsearch
├── deploy/               # Terraform (GCP + Neon) and the deployment bootstrap runbook
├── backend/              # ASP.NET Core Web API - see backend/README.md
└── frontend/             # Next.js client - see frontend/README.md
```

## Quick Start

The fastest way to see the whole thing running is Docker Compose:

```bash
docker compose up -d --build
```

This builds and runs both the API (`http://localhost:8080`) and the frontend (`http://localhost:3000`), plus Postgres, Redis, and Elasticsearch, seeding a demo admin account (`admin@movieapi.local` / `Admin123!`) on first boot. See [Running with Docker](backend/README.md#running-with-docker) in the backend README for configuration details and caveats (it's a local demo stack, not a deployment template).

To run either half directly against your own tooling instead:

- Backend: see [Getting Started](backend/README.md#getting-started) in the backend README (needs a Postgres instance and the .NET 10 SDK)
- Frontend: see [Getting Started](frontend/README.md#getting-started) in the frontend README (needs `pnpm` and a running backend to talk to)

A `Makefile` at the repo root wraps the common commands across both stacks (`make help` lists them).

## Deployment

Deploys to Google Cloud Run (backend + frontend) backed by [Neon](https://neon.tech) serverless Postgres, via GitHub Actions + Workload Identity Federation. See [deploy/README.md](deploy/README.md) for the one-time bootstrap runbook and an overview of the pipeline.

## License

This project is released under the [CC0 1.0 Universal](LICENSE) license — public domain dedication.
