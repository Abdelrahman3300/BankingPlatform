
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Domain.ValueObjects;

public class EmailAddressTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldCreateEmailAddress()
    {
        // Act
        var email = EmailAddress.Create("john@example.com");

        // Assert
        Assert.Equal("john@example.com", email.Value);
    }

    [Fact]
    public void Create_WithUppercaseEmail_ShouldNormalizeToLowercase()
    {
        // Act
        var email = EmailAddress.Create("JOHN@EXAMPLE.COM");

        // Assert
        Assert.Equal("john@example.com", email.Value);
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_ShouldTrimEmail()
    {
        // Act
        var email = EmailAddress.Create("  john@example.com  ");

        // Assert
        Assert.Equal("john@example.com", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("john@@example.com")]
    [InlineData("john @example.com")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(
        string? value)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() =>
            EmailAddress.Create(value!));
    }

    [Fact]
    public void Create_WithSameNormalizedEmail_ShouldBeEqual()
    {
        // Arrange
        var first = EmailAddress.Create("JOHN@EXAMPLE.COM");
        var second = EmailAddress.Create("john@example.com");

        // Assert
        Assert.Equal(first, second);
    }
}
