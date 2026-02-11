using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using UrlShortener.API.Services;
using UrlShortener.API.Settings;
using Xunit;

namespace UrlShortener.Tests.Services
{
    public class UrlShorteningServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UrlShorteningService _service;
        private readonly ShortLinkSettings _settings;

        public UrlShorteningServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _settings = new ShortLinkSettings { Length = 7, Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" };
            var settingsOptions = Options.Create(_settings);
            _dbContext = new ApplicationDbContext(options, settingsOptions);
            _service = new UrlShorteningService(_dbContext, settingsOptions);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GenerateUniqueCode_ReturnsCode_WithCorrectLength()
        {
            var code = await _service.GenerateUniqueCode();

            Assert.Equal(_settings.Length, code.Length);
        }

        [Fact]
        public async Task GenerateUniqueCode_ReturnsCode_UsingOnlyValidCharacters()
        {
            var code = await _service.GenerateUniqueCode();

            Assert.All(code, c => Assert.Contains(c, _settings.Alphabet));
        }

        [Fact]
        public async Task GenerateUniqueCode_ReturnsUniqueCode_WhenCalledMultipleTimes()
        {
            var codes = new HashSet<string>();

            for (int i = 0; i < 10; i++)
            {
                var code = await _service.GenerateUniqueCode();
                codes.Add(code);
            }

            Assert.Equal(10, codes.Count);
        }

        [Fact]
        public async Task GenerateUniqueCode_GeneratesNewCode_WhenCodeAlreadyExists()
        {
            var existingCode = "ABC1234";
            _dbContext.ShortenedUrls.Add(new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                LongUrl = "https://example.com",
                Code = existingCode,
                ShortUrl = $"https://short.url/{existingCode}",
                CreatedOnUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            var attempts = 0;
            string newCode;
            do
            {
                newCode = await _service.GenerateUniqueCode();
                attempts++;
            } while (newCode == existingCode && attempts < 100);

            Assert.NotEqual(existingCode, newCode);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(20)]
        public async Task GenerateUniqueCode_GeneratesMultipleUniqueCodes(int count)
        {
            var codes = new List<string>();

            for (int i = 0; i < count; i++)
            {
                codes.Add(await _service.GenerateUniqueCode());
            }

            Assert.Equal(count, codes.Distinct().Count());
        }
    }
}
