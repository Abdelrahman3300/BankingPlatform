
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.Enums;
using BankingPlatform.Domain.Exceptions;
using BankingPlatform.Domain.ValueObjects;

namespace BankingPlatform.Tests.Domain.Entities;

public class CustomerTests
{
    private static Customer CreateCustomer()
    {
        return Customer.Create(
            "Abdelrahman",
            "Shendy",
            EmailAddress.Create("abdelrahman@example.com"));
    }

    [Fact]
    public void Create_WithValidData_ShouldCreatePendingCustomer()
    {
        // Act
        var customer = CreateCustomer();

        // Assert
        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Abdelrahman", customer.FirstName);
        Assert.Equal("Shendy", customer.LastName);
        Assert.Equal(
            "abdelrahman@example.com",
            customer.Email.Value);

        Assert.Equal(
            CustomerStatus.PendingVerification,
            customer.Status);

        Assert.Null(customer.UpdatedAtUtc);
        Assert.NotEqual(default, customer.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithWhitespaceInNames_ShouldTrimNames()
    {
        // Act
        var customer = Customer.Create(
            "  John  ",
            "  Smith  ",
            EmailAddress.Create("john@example.com"));

        // Assert
        Assert.Equal("John", customer.FirstName);
        Assert.Equal("Smith", customer.LastName);
    }

    [Theory]
    [InlineData(null, "Smith")]
    [InlineData("", "Smith")]
    [InlineData("   ", "Smith")]
    [InlineData("John", null)]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void Create_WithInvalidNames_ShouldThrowDomainException(
        string? firstName,
        string? lastName)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() =>
            Customer.Create(
                firstName!,
                lastName!,
                EmailAddress.Create("john@example.com")));
    }

    [Fact]
    public void Activate_WhenPending_ShouldActivateCustomer()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        customer.Activate();

        // Assert
        Assert.Equal(CustomerStatus.Active, customer.Status);
        Assert.NotNull(customer.UpdatedAtUtc);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.Activate());
    }

    [Fact]
    public void Suspend_WhenActive_ShouldSuspendCustomer()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();

        // Act
        customer.Suspend();

        // Assert
        Assert.Equal(CustomerStatus.Suspended, customer.Status);
    }

    [Fact]
    public void Reactivate_WhenSuspended_ShouldActivateCustomer()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();
        customer.Suspend();

        // Act
        customer.Reactivate();

        // Assert
        Assert.Equal(CustomerStatus.Active, customer.Status);
    }

    [Fact]
    public void Close_WhenActive_ShouldCloseCustomer()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();

        // Act
        customer.Close();

        // Assert
        Assert.Equal(CustomerStatus.Closed, customer.Status);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Close();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.Close());
    }

    [Fact]
    public void UpdateName_WhenCustomerIsActive_ShouldUpdateNames()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();

        // Act
        customer.UpdateName("Mohamed", "Ali");

        // Assert
        Assert.Equal("Mohamed", customer.FirstName);
        Assert.Equal("Ali", customer.LastName);
        Assert.NotNull(customer.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateName_WhenCustomerIsClosed_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Close();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.UpdateName("Mohamed", "Ali"));
    }

    [Fact]
    public void Suspend_WhenPending_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.Suspend());
    }

    [Fact]
    public void Reactivate_WhenActive_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.Reactivate());
    }

    [Fact]
    public void EnsureCanOpenAccount_WhenCustomerIsActive_ShouldNotThrow()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();

        // Act & Assert
        var exception = Record.Exception(() =>
            customer.EnsureCanOpenAccount());

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanOpenAccount_WhenCustomerIsPending_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.EnsureCanOpenAccount());
    }

    [Fact]
    public void EnsureCanOpenAccount_WhenCustomerIsSuspended_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Activate();
        customer.Suspend();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.EnsureCanOpenAccount());
    }

    [Fact]
    public void EnsureCanOpenAccount_WhenCustomerIsClosed_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Close();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            customer.EnsureCanOpenAccount());
    }
}
