
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.Enums;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;
using Xunit;

namespace BankingPlatform.Tests.Domain.Entities;

public class AccountTests
{
    private static readonly Guid ValidCustomerId =
        Guid.NewGuid();

    private static Account CreateValidAccount()
    {
        return Account.Create(
            ValidCustomerId,
            "1234567890123456",
            Currency.Create("USD"));
    }

    [Fact]
    public void Create_WithValidData_CreatesActiveAccount()
    {
        var account = CreateValidAccount();

        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(ValidCustomerId, account.CustomerId);
        Assert.Equal("1234567890123456", account.AccountNumber);
        Assert.Equal("USD", account.Currency.Code);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.NotEqual(default, account.CreatedAtUtc);
        Assert.Null(account.UpdatedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Account.Create(
                Guid.Empty,
                "1234567890123456",
                Currency.Create("USD")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyAccountNumber_ThrowsDomainException(
        string? accountNumber)
    {
        Assert.Throws<DomainException>(() =>
            Account.Create(
                ValidCustomerId,
                accountNumber!,
                Currency.Create("USD")));
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012345678901")]
    [InlineData("12345678901234AB")]
    [InlineData("1234 6789012345")]
    [InlineData("123-456-7890")]
    public void Create_WithInvalidAccountNumber_ThrowsDomainException(
        string accountNumber)
    {
        Assert.Throws<DomainException>(() =>
            Account.Create(
                ValidCustomerId,
                accountNumber,
                Currency.Create("USD")));
    }

    [Fact]
    public void Create_WithWhitespaceAroundAccountNumber_TrimsValue()
    {
        var account = Account.Create(
            ValidCustomerId,
            "  1234567890123456  ",
            Currency.Create("USD"));

        Assert.Equal("1234567890123456", account.AccountNumber);
    }

    [Fact]
    public void Create_WithNullCurrency_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Account.Create(
                ValidCustomerId,
                "1234567890123456",
                null!));
    }

    [Fact]
    public void Freeze_ActiveAccount_ChangesStatusToFrozen()
    {
        var account = CreateValidAccount();

        account.Freeze();

        Assert.Equal(AccountStatus.Frozen, account.Status);
        Assert.NotNull(account.UpdatedAtUtc);
    }

    [Fact]
    public void Freeze_FrozenAccount_ThrowsDomainException()
    {
        var account = CreateValidAccount();
        account.Freeze();

        Assert.Throws<DomainException>(
            () => account.Freeze());
    }

    [Fact]
    public void Freeze_ClosedAccount_ThrowsDomainException()
    {
        var account = CreateValidAccount();
        account.Close();

        Assert.Throws<DomainException>(
            () => account.Freeze());
    }

    [Fact]
    public void Reactivate_FrozenAccount_ChangesStatusToActive()
    {
        var account = CreateValidAccount();
        account.Freeze();

        account.Reactivate();

        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.NotNull(account.UpdatedAtUtc);
    }

    [Fact]
    public void Reactivate_ActiveAccount_ThrowsDomainException()
    {
        var account = CreateValidAccount();

        Assert.Throws<DomainException>(
            () => account.Reactivate());
    }

    [Fact]
    public void Reactivate_ClosedAccount_ThrowsDomainException()
    {
        var account = CreateValidAccount();
        account.Close();

        Assert.Throws<DomainException>(
            () => account.Reactivate());
    }

    [Fact]
    public void Close_ActiveAccount_ChangesStatusToClosed()
    {
        var account = CreateValidAccount();

        account.Close();

        Assert.Equal(AccountStatus.Closed, account.Status);
        Assert.NotNull(account.UpdatedAtUtc);
    }

    [Fact]
    public void Close_FrozenAccount_ChangesStatusToClosed()
    {
        var account = CreateValidAccount();
        account.Freeze();

        account.Close();

        Assert.Equal(AccountStatus.Closed, account.Status);
    }

    [Fact]
    public void Close_AlreadyClosedAccount_ThrowsDomainException()
    {
        var account = CreateValidAccount();
        account.Close();

        Assert.Throws<DomainException>(
            () => account.Close());
    }
}
