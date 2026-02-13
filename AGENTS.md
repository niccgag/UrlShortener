# AGENTS.md - AI Coding Assistant Guidelines

## Build Commands (Local Development)

For Docker-based development, use the Make commands below in the Docker Commands section.

```bash
# Run both API and Web frontend together (uses concurrently)
yarn dev

# Run API only
cd UrlShortener.API && dotnet run

# Run Web frontend only
cd UrlShortener.Web && yarn dev

# Build the entire solution
dotnet build

# Build specific project
dotnet build UrlShortener.API
dotnet build UrlShortener.Tests

# Run with hot reload (development)
dotnet watch run --project UrlShortener.API
```

## Test Commands

```bash
# Run all tests
cd UrlShortener.Tests && dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~UrlShorteningServiceTests"
dotnet test --filter "FullyQualifiedName~UrlShortenerApiTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~GenerateUniqueCode_ReturnsCode_WithCorrectLength"

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests by category (if traits are used)
dotnet test --filter "Category=Integration"
```

## Docker Commands

### Using Make (Recommended)

```bash
# Production mode (default)
make up

# Development mode (with hot reload)
make dev

# Stop all services
make down

# Build/rebuild services
make build          # Production
make build-dev      # Development

# View logs
make logs           # Production
make logs-dev       # Development

# Clean up (removes volumes and images)
make clean
```

### Using Docker Compose Directly

```bash
# Production mode
docker compose up -d

# Development mode (with hot reload)
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d

# View logs
docker compose logs -f

# Stop services
docker compose down

# Rebuild after changes
docker compose up -d --build

# Access services:
# - Frontend: http://localhost:8081
# - API: http://localhost:8080
# - Redis: localhost:6379
```

## Database Commands

```bash
# Create new migration
cd UrlShortener.API && dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Code Style Guidelines

### Naming Conventions
- **Classes/Interfaces**: PascalCase (e.g., `UrlShorteningService`, `IRepository`)
- **Methods**: PascalCase (e.g., `GenerateUniqueCode()`)
- **Properties**: PascalCase (e.g., `LongUrl`, `CreatedOnUtc`)
- **Fields**: `_camelCase` with underscore prefix for private fields
- **Local variables**: camelCase (e.g., `var code = ...`)
- **Constants**: PascalCase (e.g., `DefaultLength`)
- **Namespaces**: PascalCase matching folder structure

### Language Features
- Use **nullable reference types** (`<Nullable>enable</Nullable>` in csproj)
- Use **implicit usings** (`<ImplicitUsings>enable</ImplicitUsings>`)
- Prefer **primary constructors** for dependency injection (C# 12)
- Use `var` for local variable declarations
- Use **file-scoped namespaces** where possible
- Use **records** for simple DTOs (e.g., `public record ShortenUrlRequest(string Url)`)

### Imports
- Group imports: System, then Microsoft, then third-party, then local
- Remove unused imports
- Use fully qualified names only when necessary to resolve conflicts

### Formatting
- Indentation: Tabs (observed in codebase)
- Opening braces on same line
- Single blank line between methods
- Two blank lines between classes

### Error Handling
- Use `is null` instead of `== null` for null checks
- Return appropriate HTTP status codes in minimal APIs (`Results.BadRequest()`, `Results.NotFound()`)
- Validate input at API boundary
- Use `ArgumentException` for invalid arguments in services

### Async Patterns
- Use `async`/`await` consistently
- Name async methods with `Async` suffix
- Return `Task<T>` or `Task` from async methods
- Use `CancellationToken` for cancellable operations when applicable

### Testing Conventions
- **Unit tests**: Inherit from `IDisposable` for cleanup, use InMemory database
- **Integration tests**: Use `IClassFixture<CustomWebApplicationFactory>`
- Test method names: `MethodName_ExpectedBehavior_Condition`
- Use `[Fact]` for single test cases, `[Theory]` with `[InlineData]` for parameterized tests
- Use `Assert` methods from xUnit (e.g., `Assert.Equal`, `Assert.NotNull`)
- Integration tests verify database state when needed

### Architecture Patterns
- **Minimal APIs**: Map endpoints directly in `Program.cs` using `app.MapGet`, `app.MapPost`
- **Services**: Business logic in `Services/` folder, registered with `AddScoped<T>()`
- **Data**: EF Core with SQLite in development, migrations in `Migrations/`
- **Settings**: Use `IOptions<T>` pattern for configuration
- **Models**: Keep simple POCOs in `Models/` folder
- Initialize string properties with `string.Empty` to avoid null warnings

### Docker Guidelines
- Multi-stage builds for optimized images
- SQLite database persisted via Docker volume
- Health checks configured for container monitoring

### Common Patterns
```csharp
// Service with primary constructor
public class MyService(ApplicationDbContext dbContext, IOptions<Settings> settings)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly Settings _settings = settings.Value;
}

// Null check pattern
if (shortenedUrl is null)
{
    return Results.NotFound();
}

// Model initialization
public class ShortenedUrl
{
    public string LongUrl { get; set; } = string.Empty;
}
```

## Project Structure
- `UrlShortener.API/` - Main API project
  - `Data/` - DbContext and migrations
  - `Models/` - Entity classes
  - `Services/` - Business logic
  - `Settings/` - Configuration classes
  - `Program.cs` - Entry point with minimal API endpoints
- `UrlShortener.Tests/` - Test project
  - `Integration/` - API integration tests
  - `Services/` - Unit tests
  - `Helpers/` - Test utilities (CustomWebApplicationFactory)
- `UrlShortener.Web/` - Vue 3 frontend
  - `src/components/` - Vue components
  - `src/services/` - API client

## Target Framework
- .NET 8.0
- C# 12.0 features enabled
- Vue 3 + TypeScript + Vite
