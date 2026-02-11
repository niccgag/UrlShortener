#!/bin/sh

# Wait for the app to be ready, then run migrations
echo "Starting application..."

# Run database migrations
echo "Running database migrations..."
dotnet ef database update --project /app/UrlShortener.API.dll || echo "Migration failed or not needed"

# Start the application
echo "Starting URL Shortener API..."
exec dotnet UrlShortener.API.dll