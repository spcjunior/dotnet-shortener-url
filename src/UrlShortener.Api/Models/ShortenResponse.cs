namespace UrlShortener.Api.Models;

public record ShortenResponse(string ShortUrl, string ShortCode, string OriginalUrl);
