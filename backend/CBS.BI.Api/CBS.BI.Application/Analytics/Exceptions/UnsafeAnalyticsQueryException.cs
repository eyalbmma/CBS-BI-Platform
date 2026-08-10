namespace CBS.BI.Application.Analytics.Exceptions;

public sealed class UnsafeAnalyticsQueryException : Exception
{
    public UnsafeAnalyticsQueryException()
        : base("Only read-only analytics queries are allowed.")
    {
    }
}
