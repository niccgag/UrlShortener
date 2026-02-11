using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using UrlShortener.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped<UrlShorteningService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapPost("shorten", async (
	ShortenUrlRequest request,
	UrlShorteningService urlShorteningService,
	ApplicationDbContext dbContext,
	HttpContext httpContext) =>
{
	if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
	    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
	{
		return Results.BadRequest("The specified URL is invalid.");
	}

	var code = await urlShorteningService.GenerateUniqueCode();
	var httpRequest = httpContext.Request;
	var shortenedUrl = new ShortenedUrl
	{
		Id = Guid.NewGuid(),
		LongUrl = request.Url,
		Code = code,
		ShortUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/{code}",
		CreatedOnUtc = DateTime.UtcNow
	};

	dbContext.ShortenedUrls.Add(shortenedUrl);
	await dbContext.SaveChangesAsync();
	return Results.Ok(shortenedUrl.ShortUrl);
});

app.MapGet("{code}", async (string code, ApplicationDbContext dbContext) =>
{
	var shortendUrl = await dbContext.ShortenedUrls.SingleOrDefaultAsync(s => s.Code == code);
	if (shortendUrl is null)
	{
		return Results.NotFound();
	}
	return Results.Redirect(shortendUrl.LongUrl);
});

app.Run();

public record ShortenUrlRequest(string Url);

public partial class Program { }