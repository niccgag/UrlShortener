# URL Shortener API

A simple, lightweight URL shortening service built with ASP.NET Core and .NET 8.0.

## Overview

This API allows you to create shortened versions of long URLs and redirect users to the original URLs using short codes. It uses SQLite for data persistence and generates unique 7-character alphanumeric codes.

## Features

- **Shorten URLs**: Convert long URLs into compact, shareable short links
- **URL Redirection**: Redirect users from short codes to original URLs
- **SQLite Database**: Lightweight, file-based database for storing URL mappings
- **Docker Support**: Fully containerized with Docker and Docker Compose
- **Swagger Documentation**: Interactive API documentation at `/swagger`
- **Health Checks**: Docker health checks for container monitoring
- **Unique Code Generation**: Collision-resistant 7-character alphanumeric codes

## API Endpoints

### POST /shorten

Creates a shortened URL from a long URL.

**Request:**
```json
{
  "url": "https://example.com/very/long/url/path"
}
```

**Response:**
```
"https://localhost:5000/abc1234"
```

**Validation:**
- Only valid absolute URLs with HTTP/HTTPS schemes are accepted
- Returns `400 Bad Request` for invalid URLs

### GET /{code}

Redirects to the original URL associated with the short code.

**Response:**
- `302 Redirect` to the original URL if code exists
- `404 Not Found` if code doesn't exist

## Technology Stack

- **.NET 8.0**: Modern, high-performance web framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: Object-relational mapper for database operations
- **SQLite**: Lightweight, serverless database
- **Swashbuckle**: Swagger/OpenAPI documentation generation
- **Docker**: Containerization for easy deployment
- **xUnit**: Testing framework with integration tests

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://docs.docker.com/get-docker/) (optional, for containerized deployment)

### Running Locally

1. Clone the repository:
```bash
git clone <repository-url>
cd UrlShortener
```

2. Run the application:
```bash
cd UrlShortener.API
dotnet run
```

3. Open your browser to:
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

### Running with Docker

1. Build and run with Docker Compose:
```bash
docker-compose up -d
```

2. The API will be available at `http://localhost:8080`

3. View logs:
```bash
docker-compose logs -f
```

4. Stop the service:
```bash
docker-compose down
```

## Configuration

The short link code settings can be modified in `UrlShortener.API/appsettings.json`:

```json
{
  "ShortLinkSettings": {
    "Length": 7,
    "Alphabet": "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
  }
}
```

- **Length**: Number of characters in generated short codes (default: 7)
- **Alphabet**: Characters used to generate short codes

You can also override these settings per environment using `appsettings.Development.json` or environment variables.

## Database

The application uses SQLite with Entity Framework Core. The database file is created automatically at:
- **Local development**: `urlShortening.db` in the project root
- **Docker**: `/app/data/urlShortening.db` (persisted via Docker volume)

### Database Migrations

To create a new migration:
```bash
cd UrlShortener.API
dotnet ef migrations add MigrationName
```

To update the database:
```bash
dotnet ef database update
```

## Testing

The project includes both unit and integration tests using xUnit.

### Run all tests:
```bash
cd UrlShortener.Tests
dotnet test
```

### Test Coverage

- **Integration Tests**: Full API endpoint testing with in-memory database
  - URL shortening validation
  - Redirection logic
  - Database persistence
  - Error handling

## Project Structure

```
UrlShortener/
├── UrlShortener.API/              # Main API project
│   ├── Data/                      # Database context
│   ├── Migrations/                # EF Core migrations
│   ├── Models/                    # Data models
│   ├── Services/                  # Business logic
│   ├── Settings/                  # Configuration settings
│   ├── Program.cs                 # Application entry point
│   └── UrlShortener.API.csproj
├── UrlShortener.Tests/            # Test project
│   ├── Integration/               # Integration tests
│   ├── Services/                  # Unit tests
│   └── Helpers/                   # Test utilities
├── Dockerfile                     # Docker image definition
├── docker-compose.yml             # Docker Compose configuration
└── README.md
```

## Docker Configuration

The application is containerized with multi-stage builds for optimized production images.

### Docker Features

- **Multi-stage build**: Separates build and runtime environments
- **SQLite persistence**: Data stored in Docker volume
- **Health checks**: Monitors application availability
- **Production-optimized**: ASP.NET Core production environment

### Volumes

- `sqlite_data`: Persists the SQLite database between container restarts

## Development

### Adding New Features

1. Create models in `Models/`
2. Add database migrations: `dotnet ef migrations add FeatureName`
3. Implement services in `Services/`
4. Add endpoints in `Program.cs`
5. Write tests in `UrlShortener.Tests/`

### Code Style

- Use nullable reference types
- Follow C# naming conventions
- Keep services focused and testable
- Document public APIs

## License

[Your License Here]

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## Support

For issues or questions, please open an issue in the repository.
