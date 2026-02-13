using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using UrlShortener.API.Services;
using UrlShortener.API.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configure Redis with graceful fallback
builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var connectionString =
        configuration.GetValue<string>("Redis:ConnectionString", "localhost:6379")
        ?? "localhost:6379";

    try
    {
        var options = ConfigurationOptions.Parse(connectionString);
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 5000;
        return ConnectionMultiplexer.Connect(options);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Redis connection failed. Application will run without caching.");
        return ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false,connectTimeout=1");
    }
});

// Register cache service
builder.Services.AddScoped<IUrlCacheService, UrlCacheService>();

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped<UrlShorteningService>();

// Configure CORS to allow frontend requests
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// Configure ShortLinkSettings from appsettings
builder.Services.Configure<ShortLinkSettings>(
    builder.Configuration.GetSection("ShortLinkSettings")
);

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapPost(
    "shorten",
    async (
        ShortenUrlRequest request,
        UrlShorteningService urlShorteningService,
        ApplicationDbContext dbContext,
        IUrlCacheService cache
    ) =>
    {
        if (
            !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            return Results.BadRequest("The specified URL is invalid.");
        }

        var code = await urlShorteningService.GenerateUniqueCode();
        var shortenedUrl = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            LongUrl = request.Url,
            Code = code,
            ShortUrl = code,
            CreatedOnUtc = DateTime.UtcNow,
        };

        dbContext.ShortenedUrls.Add(shortenedUrl);
        await dbContext.SaveChangesAsync();

        await cache.SetAsync(code, request.Url);

        return Results.Ok(code);
    }
);

app.MapGet(
    "{code}",
    async (string code, ApplicationDbContext dbContext, IUrlCacheService cache) =>
    {
        // Try to get from cache first
        var cachedUrl = await cache.GetAsync(code);
        if (cachedUrl is not null)
        {
            return Results.Redirect(cachedUrl);
        }

        // Fall back to database
        var shortendUrl = await dbContext.ShortenedUrls.SingleOrDefaultAsync(s => s.Code == code);
        if (shortendUrl is null)
        {
            return Results.NotFound();
        }

        await cache.SetAsync(code, shortendUrl.LongUrl);

        return Results.Redirect(shortendUrl.LongUrl);
    }
);

app.Run();

public record ShortenUrlRequest(string Url);

public partial class Program { }
