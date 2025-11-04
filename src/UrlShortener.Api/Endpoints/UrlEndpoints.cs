using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Endpoints;

public static class UrlEndpoints
{
    public static IEndpointRouteBuilder MapUrlEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/")
            .WithTags("URL Shortener");

        // POST /shorten - Criar URL encurtada
        group.MapPost("/shorten", ShortenUrl)
            .WithName("ShortenUrl")
            .WithSummary("Criar URL encurtada")
            .WithDescription("Recebe uma URL completa e retorna um código curto de 6 caracteres para redirecionamento.")
            .Produces<ShortenResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        // GET /{shortCode} - Redirecionar para URL original
        group.MapGet("/{shortCode}", RedirectToOriginalUrl)
            .WithName("RedirectToOriginalUrl")
            .WithSummary("Redirecionar para URL original")
            .WithDescription("Redireciona permanentemente (HTTP 301) para a URL original associada ao código curto.")
            .Produces(StatusCodes.Status301MovedPermanently)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> ShortenUrl(
        ShortenRequest request, 
        AppDbContext db, 
        HttpContext context,
        ILogger<Program> logger)
    {
        // Validação básica
        if (string.IsNullOrWhiteSpace(request.OriginalUrl))
        {
            return Results.BadRequest(new { error = "URL is required" });
        }

        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Results.BadRequest(new { error = "Invalid URL format" });
        }

        try
        {
            // Criar nova entrada
            var url = new Url
            {
                OriginalUrl = request.OriginalUrl
            };

            db.Urls.Add(url);
            await db.SaveChangesAsync();

            // Gerar short code a partir do ID
            url.ShortCode = ShortCodeGenerator.Generate(url.Id);
            await db.SaveChangesAsync();

            // Construir URL completa
            var scheme = context.Request.Scheme;
            var host = context.Request.Host.Value;
            var shortUrl = $"{scheme}://{host}/{url.ShortCode}";

            var response = new ShortenResponse(shortUrl, url.ShortCode, url.OriginalUrl);
            
            return Results.Created($"/{url.ShortCode}", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating short URL");
            return Results.Problem("An error occurred while creating the short URL");
        }
    }

    private static async Task<IResult> RedirectToOriginalUrl(
        string shortCode, 
        AppDbContext db,
        ILogger<Program> logger)
    {
        try
        {
            // 1. Decodificar shortCode para obter o ID
            long id;
            try
            {
                id = ShortCodeGenerator.Decode(shortCode);
            }
            catch (ArgumentException)
            {
                // ShortCode inválido (não é um Hashids válido)
                return Results.NotFound(new { error = "Short URL not found" });
            }

            // 2. Buscar no banco por PRIMARY KEY (muito mais rápido)
            var url = await db.Urls
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (url == null)
            {
                return Results.NotFound(new { error = "Short URL not found" });
            }

            return Results.Redirect(url.OriginalUrl, permanent: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving short URL: {ShortCode}", shortCode);
            return Results.Problem("An error occurred while retrieving the URL");
        }
    }
}
