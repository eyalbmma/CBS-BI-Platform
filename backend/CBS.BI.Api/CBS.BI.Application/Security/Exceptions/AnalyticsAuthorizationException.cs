namespace CBS.BI.Application.Security.Exceptions;

public sealed class AnalyticsAuthorizationException : Exception
{
    public AnalyticsAuthorizationException()
      : base("User is not authorized to execute analytics queries.")
    {
    }
}
