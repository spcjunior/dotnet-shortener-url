using UrlShortener.Api.Services;
using Xunit;

namespace UrlShortener.Tests;

public class ShortCodeGeneratorTests
{
    [Fact]
    public void Generate_WithMinimumId_ShouldReturnAtLeast6Characters()
    {
        // Arrange
        long id = 916132832; // 62^5 - primeiro ID

        // Act
        var shortCode = ShortCodeGenerator.Generate(id);

        // Assert - Hashids garante mínimo de 6 caracteres
        Assert.True(shortCode.Length >= 6, $"Expected at least 6 characters, got {shortCode.Length}");
    }

    [Fact]
    public void Generate_ShouldReturnValidBase62Characters()
    {
        // Arrange
        long id = 916132832;
        var validChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Act
        var shortCode = ShortCodeGenerator.Generate(id);

        // Assert
        Assert.All(shortCode, c => Assert.Contains(c, validChars));
    }

    [Fact]
    public void Generate_DifferentIds_ShouldReturnDifferentShortCodes()
    {
        // Arrange
        long id1 = 916132832;
        long id2 = 916132833;

        // Act
        var shortCode1 = ShortCodeGenerator.Generate(id1);
        var shortCode2 = ShortCodeGenerator.Generate(id2);

        // Assert
        Assert.NotEqual(shortCode1, shortCode2);
    }

    [Fact]
    public void Decode_ShouldReturnOriginalId()
    {
        // Arrange
        long originalId = 916132832;
        var shortCode = ShortCodeGenerator.Generate(originalId);

        // Act
        var decodedId = ShortCodeGenerator.Decode(shortCode);

        // Assert
        Assert.Equal(originalId, decodedId);
    }

    [Fact]
    public void Generate_MultipleSequentialIds_ShouldAllBeAtLeast6Characters()
    {
        // Arrange & Act & Assert
        for (long i = 916132832; i < 916132842; i++) // Testar 10 IDs sequenciais
        {
            var shortCode = ShortCodeGenerator.Generate(i);
            Assert.True(shortCode.Length >= 6, $"ID {i} generated code with {shortCode.Length} characters");
        }
    }

    [Theory]
    [InlineData(916132832)]
    [InlineData(1000000000)]
    [InlineData(5000000000)]
    public void RoundTrip_ShouldPreserveId(long id)
    {
        // Arrange
        var shortCode = ShortCodeGenerator.Generate(id);

        // Act
        var decodedId = ShortCodeGenerator.Decode(shortCode);

        // Assert
        Assert.Equal(id, decodedId);
    }

    [Fact]
    public void Generate_SequentialIds_ShouldNotBeSequentialCodes()
    {
        // Arrange - IDs sequenciais
        long id1 = 916132832;
        long id2 = 916132833;
        long id3 = 916132834;

        // Act
        var code1 = ShortCodeGenerator.Generate(id1);
        var code2 = ShortCodeGenerator.Generate(id2);
        var code3 = ShortCodeGenerator.Generate(id3);

        // Assert - Códigos não devem ser sequenciais
        // Hashids garante que IDs sequenciais geram códigos "aleatórios"
        Assert.NotEqual(code1, code2);
        Assert.NotEqual(code2, code3);
        
        // Os códigos devem parecer aleatórios (não incrementais)
        // Verificar que pelo menos um caractere muda em posições diferentes
        var samePositions = 0;
        for (int i = 0; i < Math.Min(code1.Length, code2.Length); i++)
        {
            if (code1[i] == code2[i]) samePositions++;
        }
        
        // Se fossem sequenciais, teriam muitos caracteres iguais nas mesmas posições
        Assert.True(samePositions < code1.Length - 1);
    }

    [Fact]
    public void Decode_InvalidShortCode_ShouldThrowException()
    {
        // Arrange
        var invalidCode = "invalid!@#";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ShortCodeGenerator.Decode(invalidCode));
    }
}
