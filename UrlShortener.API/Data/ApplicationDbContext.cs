using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.API.Models;
using UrlShortener.API.Settings;

namespace UrlShortener.API.Data
{
	public class ApplicationDbContext : DbContext
	{
		private string DbPath;
		private readonly ShortLinkSettings _settings;

		public ApplicationDbContext(DbContextOptions options, IOptions<ShortLinkSettings>? settings = null) : base(options)
		{
			// Use /app/data directory for database in container if it exists, otherwise use local directory
			var path = Directory.Exists("/app/data") ? "/app/data" : Directory.GetCurrentDirectory();
			DbPath = Path.Join(path, "urlShortening.db");
			_settings = settings?.Value ?? new ShortLinkSettings();
		}

		public ApplicationDbContext()
		{
			// Use /app/data directory for database in container if it exists, otherwise use local directory
			var path = Directory.Exists("/app/data") ? "/app/data" : Directory.GetCurrentDirectory();
			DbPath = Path.Join(path, "urlShortening.db");
			_settings = new ShortLinkSettings();
		}

		public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ShortenedUrl>(builder =>
			{
				builder
					.Property(ShortendUrl => ShortendUrl.Code)
					.HasMaxLength(_settings.Length);

				builder
					.HasIndex(ShortenedUrl => ShortenedUrl.Code)
					.IsUnique();
			});
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				var dbPath = DbPath ?? "urlShortening.db";
				optionsBuilder.UseSqlite($"Data Source={dbPath}");
			}
		}
	}
}
