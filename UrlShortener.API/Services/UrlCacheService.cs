using StackExchange.Redis;

namespace UrlShortener.API.Services;

public interface IUrlCacheService
{
    Task<string?> GetAsync(string code);
    Task SetAsync(string code, string url);
}

public class UrlCacheService : IUrlCacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly int _cacheExpirationMinutes;
    private readonly ILogger<UrlCacheService> _logger;

    public UrlCacheService(
        IConnectionMultiplexer? redis,
        IConfiguration configuration,
        ILogger<UrlCacheService> logger
    )
    {
        _redis = redis;
        _cacheExpirationMinutes = configuration.GetValue<int>("Redis:CacheExpirationMinutes", 60);
        _logger = logger;
    }

    public async Task<string?> GetAsync(string code)
    {
        if (_redis?.IsConnected != true)
        {
            return null;
        }

        try
        {
            var db = _redis.GetDatabase();
            var cachedUrl = await db.StringGetAsync($"url:{code}");
            return cachedUrl.IsNullOrEmpty ? null : cachedUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve URL from cache for code: {Code}", code);
            return null;
        }
    }

    public async Task SetAsync(string code, string url)
    {
        if (_redis?.IsConnected != true)
        {
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(
                $"url:{code}",
                url,
                TimeSpan.FromMinutes(_cacheExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache URL for code: {Code}", code);
        }
    }
}
