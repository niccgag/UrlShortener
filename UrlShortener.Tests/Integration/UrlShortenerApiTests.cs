using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using UrlShortener.Tests.Helpers;
using Xunit;

namespace UrlShortener.Tests.Integration
{
    public class UrlShortenerApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UrlShortenerApiTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task ShortenUrl_WithValidUrl_ReturnsShortUrl()
        {
            var request = new { Url = "https://example.com" };

            var response = await _client.PostAsJsonAsync("/shorten", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var shortUrl = await response.Content.ReadAsStringAsync();
            Assert.NotNull(shortUrl);
            Assert.NotEmpty(shortUrl);
        }

        [Fact]
        public async Task ShortenUrl_WithInvalidUrl_ReturnsBadRequest()
        {
            var request = new { Url = "not-a-valid-url" };

            var response = await _client.PostAsJsonAsync("/shorten", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShortenUrl_WithRelativeUrl_ReturnsBadRequest()
        {
            var request = new { Url = "/path/to/resource" };

            var response = await _client.PostAsJsonAsync("/shorten", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShortenUrl_WithEmptyUrl_ReturnsBadRequest()
        {
            var request = new { Url = "" };

            var response = await _client.PostAsJsonAsync("/shorten", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Redirect_WithValidCode_ReturnsRedirect()
        {
            var longUrl = "https://example.com/redirect-test";
            
            // First, shorten a URL to get a code
            var shortenRequest = new { Url = longUrl };
            var shortenResponse = await _client.PostAsJsonAsync("/shorten", shortenRequest);
            var shortUrl = await shortenResponse.Content.ReadAsStringAsync();
            
            // Extract the code from the short URL (remove surrounding quotes if present)
            var code = shortUrl.Trim('"', ' ').Split('/').Last().Trim('"');

            // Now try to redirect using that code
            var response = await _client.GetAsync($"/{code}");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(longUrl, response.Headers.Location?.ToString());
        }

        [Fact]
        public async Task Redirect_WithInvalidCode_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/INVALIDCODE");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ShortenUrl_SavesToDatabase()
        {
            var request = new { Url = "https://example.com" };

            await _client.PostAsJsonAsync("/shorten", request);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedUrl = await dbContext.ShortenedUrls.FirstOrDefaultAsync(s => s.LongUrl == "https://example.com");
            
            Assert.NotNull(savedUrl);
            Assert.NotEmpty(savedUrl.Code);
            Assert.NotEmpty(savedUrl.ShortUrl);
        }

        [Fact]
        public async Task ShortenUrl_DifferentUrls_ReturnDifferentShortUrls()
        {
            var request1 = new { Url = "https://example1.com" };
            var request2 = new { Url = "https://example2.com" };

            var response1 = await _client.PostAsJsonAsync("/shorten", request1);
            var response2 = await _client.PostAsJsonAsync("/shorten", request2);

            var shortUrl1 = await response1.Content.ReadAsStringAsync();
            var shortUrl2 = await response2.Content.ReadAsStringAsync();

            Assert.NotEqual(shortUrl1, shortUrl2);
        }

        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://example.com")]
        [InlineData("https://www.example.com/path/to/resource")]
        [InlineData("https://example.com:8080")]
        public async Task ShortenUrl_AcceptsVariousValidUrls(string url)
        {
            var request = new { Url = url };

            var response = await _client.PostAsJsonAsync("/shorten", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
