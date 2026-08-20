.PHONY: help install build test lint format \
	dev dev-backend dev-frontend \
	migrate migration check-name check-db-url \
	docker-up docker-down docker-logs

help:
	@echo "Available targets:"
	@echo "  install         Install backend + frontend dependencies"
	@echo "  build           Build backend and frontend"
	@echo "  test            Run backend unit + integration tests, and frontend unit tests"
	@echo "  lint            Check formatting/lint for both stacks (no changes)"
	@echo "  format          Apply backend formatting fixes"
	@echo "  dev             Run backend and frontend dev servers together"
	@echo "  dev-backend     Run only the backend dev server"
	@echo "  dev-frontend    Run only the frontend dev server"
	@echo "  migrate         Apply EF Core migrations (needs ConnectionStrings__postgres)"
	@echo "  migration NAME= Add a new EF Core migration"
	@echo "  docker-up       Start the full demo stack via docker compose"
	@echo "  docker-down     Stop the demo stack"
	@echo "  docker-logs     Follow logs from the demo stack"

install:
	cd backend && dotnet restore MovieAPI.slnx
	cd frontend && pnpm install --frozen-lockfile

build:
	cd backend && dotnet build MovieAPI.slnx
	cd frontend && pnpm run build

# Backend covers both MovieAPI.UnitTests and MovieAPI.IntegrationTests - the
# latter manages its own Postgres Testcontainer, no separate DB setup needed.
# Frontend runs Vitest in single-pass mode (no watch).
test:
	cd backend && dotnet test MovieAPI.slnx
	cd frontend && pnpm run test

lint:
	cd backend && dotnet format MovieAPI.slnx --verify-no-changes
	cd frontend && pnpm run lint

format:
	cd backend && dotnet format MovieAPI.slnx

dev:
	@trap 'kill 0' EXIT INT TERM; \
	(cd backend/src/MovieAPI.Api && dotnet run) & \
	(cd frontend && pnpm run dev) & \
	wait

dev-backend:
	cd backend/src/MovieAPI.Api && dotnet run

dev-frontend:
	cd frontend && pnpm run dev

check-db-url:
	@if [ -z "$$ConnectionStrings__postgres" ]; then \
		echo "Error: ConnectionStrings__postgres is not set. Example:" >&2; \
		echo '  export ConnectionStrings__postgres="Host=localhost;Port=5432;Database=MovieAPI;Username=movieapi;Password=..."' >&2; \
		exit 1; \
	fi

check-name:
	@if [ -z "$(NAME)" ]; then \
		echo "Error: NAME is required, e.g. make migration NAME=AddThing" >&2; exit 1; \
	fi

migrate: check-db-url
	cd backend && dotnet tool run dotnet-ef database update \
		--project src/MovieAPI.Infrastructure --startup-project src/MovieAPI.Api

migration: check-name check-db-url
	cd backend && dotnet tool run dotnet-ef migrations add $(NAME) \
		--project src/MovieAPI.Infrastructure --startup-project src/MovieAPI.Api --output-dir Migrations

docker-up:
	docker compose up -d --build

docker-down:
	docker compose down

docker-logs:
	docker compose logs -f
