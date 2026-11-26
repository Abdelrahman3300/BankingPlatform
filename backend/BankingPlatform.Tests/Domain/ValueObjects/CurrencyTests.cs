
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;
using Xunit;

namespace BankingPlatform.Tests.Domain.ValueObjects;

public class CurrencyTests
{
    [Theory]
    [InlineData("KWD")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("EGP")]
    public void Create_WithSupportedCode_ReturnsCurrency(
        string code)
    {
        var currency = Currency.Create(code);

        Assert.Equal(code, currency.Code);
    }

    [Fact]
    public void Create_WithLowercaseCode_NormalizesToUppercase()
    {
        var currency = Currency.Create("usd");

        Assert.Equal("USD", currency.Code);
    }

    [Fact]
    public void Create_WithWhitespaceAroundCode_TrimsValue()
    {
        var currency = Currency.Create("  KWD  ");

        Assert.Equal("KWD", currency.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCode_ThrowsDomainException(
        string? code)
    {
        Assert.Throws<DomainException>(
            () => Currency.Create(code!));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("12A")]
    [InlineData("U D")]
    [InlineData("أبج")]
    public void Create_WithInvalidFormat_ThrowsDomainException(
        string code)
    {
        Assert.Throws<DomainException>(
            () => Currency.Create(code));
    }

    [Fact]
    public void Create_WithUnsupportedCode_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(
            () => Currency.Create("GBP"));
    }

    [Fact]
    public void Equals_WithSameCurrencyCode_ReturnsTrue()
    {
        var first = Currency.Create("USD");
        var second = Currency.Create("usd");

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_WithDifferentCurrencyCodes_ReturnsFalse()
    {
        var usd = Currency.Create("USD");
        var eur = Currency.Create("EUR");

        Assert.NotEqual(usd, eur);
        Assert.True(usd != eur);
    }

    [Fact]
    public void ToString_ReturnsCurrencyCode()
    {
        var currency = Currency.Create("EGP");

        Assert.Equal("EGP", currency.ToString());
    }
}
