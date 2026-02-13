# Makefile for UrlShortener

.PHONY: up dev down build logs clean test

# Production mode (default)
up:
	docker compose up -d

# Development mode with hot reload
dev:
	docker compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Stop all services
down:
	docker compose down
	docker compose -f docker-compose.yml -f docker-compose.override.yml down 2>/dev/null || true

# Build/rebuild all services
build:
	docker compose build

# Build/rebuild in dev mode
build-dev:
	docker compose -f docker-compose.yml -f docker-compose.override.yml build

# View logs
logs:
	docker compose logs -f

# View logs for dev mode
logs-dev:
	docker compose -f docker-compose.yml -f docker-compose.override.yml logs -f

# Clean up volumes and images
clean:
	docker compose down -v --rmi local
	docker compose -f docker-compose.yml -f docker-compose.override.yml down -v --rmi local 2>/dev/null || true

# Run tests
test:
	cd UrlShortener.Tests && dotnet test --verbosity minimal
