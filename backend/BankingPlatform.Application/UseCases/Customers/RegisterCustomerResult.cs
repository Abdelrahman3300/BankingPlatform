namespace BankingPlatform.Application.UseCases.Customers;

public sealed record RegisterCustomerResult(
    bool Succeeded,
    Guid? CustomerId,
    string? Error)
{
    public static RegisterCustomerResult Success(Guid customerId)
    {
        return new RegisterCustomerResult(
            Succeeded: true,
            CustomerId: customerId,
            Error: null);
    }

    public static RegisterCustomerResult Failure(string error)
    {
        return new RegisterCustomerResult(
            Succeeded: false,
            CustomerId: null,
            Error: error);
    }
}