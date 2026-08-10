namespace CBS.BI.Application.Analytics.Exceptions;

/// <summary>
/// Represents an error during analytics query generation from natural language.
/// 
/// This provider-independent exception is thrown when the AI query generator
/// cannot generate a valid SQL query, either due to insufficient context,
/// invalid model response, or unexpected provider failures.
/// </summary>
public sealed class AnalyticsQueryGenerationException : Exception
{
 /// <summary>
    /// Initializes a new instance of the AnalyticsQueryGenerationException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AnalyticsQueryGenerationException(string message)
        : base(message)
    {
    }

    /// <summary>
 /// Initializes a new instance of the AnalyticsQueryGenerationException class with an inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public AnalyticsQueryGenerationException(string message, Exception innerException)
        : base(message, innerException)
 {
    }
}
