using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Models;
using UrlShortener.API.Settings;

namespace UrlShortener.API.Data
{
	public class ApplicationDbContext : DbContext
	{
		private string DbPath;

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
			// Use /app/data directory for database in container
			var path = "/app/data";
			DbPath = Path.Join(path, "urlShortening.db");
		}

		public ApplicationDbContext()
		{
		}

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ShortenedUrl>(builder =>
			{
				builder
					.Property(ShortendUrl => ShortendUrl.Code)
					.HasMaxLength(ShortLinkSettings.Length);

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
