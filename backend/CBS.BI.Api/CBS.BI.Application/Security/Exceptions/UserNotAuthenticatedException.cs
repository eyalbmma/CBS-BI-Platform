namespace CBS.BI.Application.Security.Exceptions;

public sealed class UserNotAuthenticatedException : Exception
{
    public UserNotAuthenticatedException()
        : base("An authenticated user is required.")
    {
    }
}
