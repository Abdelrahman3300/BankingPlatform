
using BankingPlatform.Infrastructure.Services;

namespace BankingPlatform.Tests.Infrastructure.Services;

public class AccountNumberGeneratorTests
{
    private readonly AccountNumberGenerator _generator = new();

    [Fact]
    public void Generate_ShouldReturnExactly16Digits()
    {
        // Act
        var accountNumber = _generator.Generate();

        // Assert
        Assert.Equal(16, accountNumber.Length);
        Assert.All(accountNumber, character =>
            Assert.True(char.IsAsciiDigit(character)));
    }

    [Fact]
    public void Generate_ShouldNotStartWithZero()
    {
        // Act
        var accountNumber = _generator.Generate();

        // Assert
        Assert.NotEqual('0', accountNumber[0]);
    }

    [Fact]
    public void Generate_ShouldReturnDifferentNumbersAcrossMultipleCalls()
    {
        // Act
        var accountNumbers = Enumerable.Range(0, 1_000)
            .Select(_ => _generator.Generate())
            .ToList();

        // Assert
        Assert.Equal(1_000, accountNumbers.Distinct().Count());
    }
}
