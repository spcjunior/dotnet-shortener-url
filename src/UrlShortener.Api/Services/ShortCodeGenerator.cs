using HashidsNet;

namespace UrlShortener.Api.Services;

public class ShortCodeGenerator
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Salt = "url-shortener-poc-secret-key"; // Chave secreta para ofuscação
    private const int MinHashLength = 6; // Comprimento mínimo de 6 caracteres
    
    private static readonly Hashids _hashids = new Hashids(
        salt: Salt,
        minHashLength: MinHashLength,
        alphabet: Alphabet
    );

    public static string Generate(long id)
    {
        // Usar Hashids para gerar código ofuscado
        return _hashids.EncodeLong(id);
    }

    public static long Decode(string shortCode)
    {
        // Decodificar usando Hashids
        var numbers = _hashids.DecodeLong(shortCode);
        
        if (numbers.Length == 0)
            throw new ArgumentException($"Invalid short code: {shortCode}");
        
        return numbers[0];
    }
}
