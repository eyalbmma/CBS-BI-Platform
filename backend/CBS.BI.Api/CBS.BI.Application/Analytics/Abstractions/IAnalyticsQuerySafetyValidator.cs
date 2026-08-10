namespace CBS.BI.Application.Analytics.Abstractions;

public interface IAnalyticsQuerySafetyValidator
{
    void EnsureSafe(string query);
}
