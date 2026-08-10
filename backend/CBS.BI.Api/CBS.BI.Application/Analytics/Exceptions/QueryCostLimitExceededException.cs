namespace CBS.BI.Application.Analytics.Exceptions;

public class QueryCostLimitExceededException : Exception
{
    public long EstimatedBytesProcessed { get; }

    public QueryCostLimitExceededException(long estimatedBytesProcessed)
        : base($"The query would process {estimatedBytesProcessed} bytes, which exceeds the configured processing limit.")
    {
    EstimatedBytesProcessed = estimatedBytesProcessed;
    }
}
