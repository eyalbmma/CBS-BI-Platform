using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.SaveAnalyticsQuery;

/// <summary>
/// Use case for saving a new analytics question for future reuse.
///
/// Input: user-provided Name and natural-language Question.
///
/// Flow:
/// 1. Validate Name and Question (not empty)
/// 2. Create SavedAnalyticsQuery with current timestamp
/// 3. Store using ISavedAnalyticsQueryStore
/// 4. Return the saved query with assigned ID
///
/// The natural-language Question is preserved as-is, not executed or validated
/// through Gemini at save time. Gemini execution happens when the saved query
/// is later executed via /api/analytics/ask.
/// </summary>
public sealed class SaveAnalyticsQueryUseCase
{
    private readonly ISavedAnalyticsQueryStore _store;

    public SaveAnalyticsQueryUseCase(ISavedAnalyticsQueryStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<SavedAnalyticsQuery> ExecuteAsync(
        string name,
        string question,
        CurrentUser user,
 CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
      {
     throw new ArgumentException("Query name must not be empty.", nameof(name));
     }

        if (string.IsNullOrWhiteSpace(question))
{
            throw new ArgumentException("Query question must not be empty.", nameof(question));
        }

      if (user == null)
      {
        throw new ArgumentNullException(nameof(user));
        }

        var now = DateTime.UtcNow;
     var savedQuery = new SavedAnalyticsQuery(
            Id: Guid.NewGuid().ToString(),
            Name: name.Trim(),
       Question: question.Trim(),
            CreatedByUserId: user.UserId,
            CreatedAtUtc: now,
            UpdatedAtUtc: now);

        await _store.SaveAsync(savedQuery, cancellationToken);

        return savedQuery;
    }
}
