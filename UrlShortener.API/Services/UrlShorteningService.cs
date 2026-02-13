using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.API.Data;
using UrlShortener.API.Settings;

namespace UrlShortener.API.Services
{
    public class UrlShorteningService(
        ApplicationDbContext dbContext,
        IOptions<ShortLinkSettings> settings
    )
    {
        private readonly Random _random = new();
        private readonly ShortLinkSettings _settings = settings.Value;

        public async Task<string> GenerateUniqueCode()
        {
            var codeChars = new char[_settings.Length];
            int maxValue = _settings.Alphabet.Length;

            while (true)
            {
                for (var i = 0; i < _settings.Length; i++)
                {
                    var randomIndex = _random.Next(maxValue);
                    codeChars[i] = _settings.Alphabet[randomIndex];
                }

                var code = new string(codeChars);

                if (!await dbContext.ShortenedUrls.AnyAsync(s => s.Code == code))
                {
                    return code;
                }
            }
        }
    }
}
