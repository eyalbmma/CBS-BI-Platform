using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;

namespace CBS.BI.Infrastructure.Analytics;

public class FakeAnalyticsQueryExecutor : IAnalyticsQueryExecutor
{
    public Task<AnalyticsQueryResult> ExecuteAsync(
     string query,
    CancellationToken cancellationToken)
    {
 var result = new AnalyticsQueryResult
        {
     Columns = new[] { "City", "Population" }.AsReadOnly(),
  Rows = new[]
            {
       new Dictionary<string, object?> 
       { 
       { "City", "Jerusalem" }, 
        { "Population", 1000000 } 
 }.AsReadOnly(),
    new Dictionary<string, object?> 
     { 
           { "City", "Tel Aviv" }, 
          { "Population", 480000 } 
    }.AsReadOnly()
            }.AsReadOnly()
        };

      return Task.FromResult(result);
}
}
