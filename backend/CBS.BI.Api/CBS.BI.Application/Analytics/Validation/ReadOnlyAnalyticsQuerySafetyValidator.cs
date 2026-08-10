using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Exceptions;
using System.Text;
using System.Text.RegularExpressions;

namespace CBS.BI.Application.Analytics.Validation;

public sealed class ReadOnlyAnalyticsQuerySafetyValidator : IAnalyticsQuerySafetyValidator
{
    // POC application-level guard for read-only queries.
    // This is a lightweight lexical safety check, not a complete SQL parser.
    // Production security must also rely on BigQuery IAM and least-privilege permissions.
    private static readonly string[] DataChangingKeywords =
    {
        "INSERT", "UPDATE", "DELETE", "MERGE", "DROP", "CREATE", "ALTER",
        "TRUNCATE", "GRANT", "REVOKE", "CALL", "EXECUTE", "BEGIN"
    };

    private static readonly string[] ReadOnlyKeywords = { "SELECT", "WITH" };

    public void EnsureSafe(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null, empty, or whitespace.", nameof(query));
        }

        var trimmedQuery = query.Trim();
        var firstKeyword = ExtractFirstKeyword(trimmedQuery);

        if (string.IsNullOrEmpty(firstKeyword))
        {
            throw new UnsafeAnalyticsQueryException();
        }

        if (!IsReadOnlyKeyword(firstKeyword))
        {
            throw new UnsafeAnalyticsQueryException();
        }

        // Sanitize query for keyword detection (ignores strings, comments, quoted identifiers)
        var sanitizedQuery = SanitizeQueryForAnalysis(trimmedQuery);

        if (ContainsDataChangingKeywords(sanitizedQuery))
        {
            throw new UnsafeAnalyticsQueryException();
        }
    }

    private static string? ExtractFirstKeyword(string query)
    {
        var sanitizedForFirstKeyword = RemoveCommentsOnly(query);
        var match = Regex.Match(sanitizedForFirstKeyword, @"\b([A-Za-z_][A-Za-z0-9_]*)\b", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static bool IsReadOnlyKeyword(string keyword)
    {
        return ReadOnlyKeywords.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsDataChangingKeywords(string query)
    {
        return DataChangingKeywords.Any(keyword =>
          Regex.IsMatch(query, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Sanitizes query for safety analysis by replacing strings, comments, and quoted identifiers with spaces.
    /// Does NOT modify the original query - this is only for keyword detection.
    /// </summary>
    private static string SanitizeQueryForAnalysis(string query)
    {
        var result = new StringBuilder(query);

        // Remove SQL strings and comments in order of appearance
        int position = 0;
        while (position < result.Length)
        {
            // Check for line comment -- (SQL standard)
            if (position < result.Length - 1 && result[position] == '-' && result[position + 1] == '-')
            {
                int endOfLine = result.Length;
                for (int i = position + 2; i < result.Length; i++)
                {
                    if (result[i] == '\n')
                    {
                        endOfLine = i;
                        break;
                    }
                }
                for (int i = position; i < endOfLine; i++)
                {
                    result[i] = ' ';
                }
                position = endOfLine + 1;
                continue;
            }

            // Check for line comment # (BigQuery)
            if (result[position] == '#')
            {
                int endOfLine = result.Length;
                for (int i = position + 1; i < result.Length; i++)
                {
                    if (result[i] == '\n')
                    {
                        endOfLine = i;
                        break;
                    }
                }
                for (int i = position; i < endOfLine; i++)
                {
                    result[i] = ' ';
                }
                position = endOfLine + 1;
                continue;
            }

            // Check for block comment /* ... */
            if (position < result.Length - 1 && result[position] == '/' && result[position + 1] == '*')
            {
                int endOfBlock = result.Length;
                for (int i = position + 2; i < result.Length - 1; i++)
                {
                    if (result[i] == '*' && result[i + 1] == '/')
                    {
                        endOfBlock = i + 2;
                        break;
                    }
                }
                for (int i = position; i < endOfBlock; i++)
                {
                    result[i] = ' ';
                }
                position = endOfBlock;
                continue;
            }

            // Check for single-quoted string 'text'
            if (result[position] == '\'')
            {
                int endOfString = result.Length;
                for (int i = position + 1; i < result.Length; i++)
                {
                    if (result[i] == '\'')
                    {
                        // Check for escaped quote ''
                        if (i + 1 < result.Length && result[i + 1] == '\'')
                        {
                            i++; // Skip the escaped quote
                            continue;
                        }
                        endOfString = i + 1;
                        break;
                    }
                }
                for (int i = position; i < endOfString; i++)
                {
                    result[i] = ' ';
                }
                position = endOfString;
                continue;
            }

            // Check for double-quoted string "text"
            if (result[position] == '"')
            {
                int endOfString = result.Length;
                for (int i = position + 1; i < result.Length; i++)
                {
                    if (result[i] == '"')
                    {
                        // Check for escaped quote ""
                        if (i + 1 < result.Length && result[i + 1] == '"')
                        {
                            i++; // Skip the escaped quote
                            continue;
                        }
                        endOfString = i + 1;
                        break;
                    }
                }
                for (int i = position; i < endOfString; i++)
                {
                    result[i] = ' ';
                }
                position = endOfString;
                continue;
            }

            // Check for backtick-quoted identifier `text` (BigQuery)
            if (result[position] == '`')
            {
                int endOfIdentifier = result.Length;
                for (int i = position + 1; i < result.Length; i++)
                {
                    if (result[i] == '`')
                    {
                        endOfIdentifier = i + 1;
                        break;
                    }
                }
                for (int i = position; i < endOfIdentifier; i++)
                {
                    result[i] = ' ';
                }
                position = endOfIdentifier;
                continue;
            }

            position++;
        }

        return result.ToString();
    }

    /// <summary>
    /// Removes only comments, preserving strings and identifiers.
    /// Used for extracting the first keyword without interference from comment text.
    /// </summary>
    private static string RemoveCommentsOnly(string query)
    {
        var result = new StringBuilder(query);

        int position = 0;
        while (position < result.Length)
        {
            // Check for line comment --
            if (position < result.Length - 1 && result[position] == '-' && result[position + 1] == '-')
            {
                int endOfLine = result.Length;
                for (int i = position + 2; i < result.Length; i++)
                {
                    if (result[i] == '\n')
                    {
                        endOfLine = i;
                        break;
                    }
                }
                for (int i = position; i < endOfLine; i++)
                {
                    result[i] = ' ';
                }
                position = endOfLine + 1;
                continue;
            }

            // Check for line comment #
            if (result[position] == '#')
            {
                int endOfLine = result.Length;
                for (int i = position + 1; i < result.Length; i++)
                {
                    if (result[i] == '\n')
                    {
                        endOfLine = i;
                        break;
                    }
                }
                for (int i = position; i < endOfLine; i++)
                {
                    result[i] = ' ';
                }
                position = endOfLine + 1;
                continue;
            }

            // Check for block comment
            if (position < result.Length - 1 && result[position] == '/' && result[position + 1] == '*')
            {
                int endOfBlock = result.Length;
                for (int i = position + 2; i < result.Length - 1; i++)
                {
                    if (result[i] == '*' && result[i + 1] == '/')
                    {
                        endOfBlock = i + 2;
                        break;
                    }
                }
                for (int i = position; i < endOfBlock; i++)
                {
                    result[i] = ' ';
                }
                position = endOfBlock;
                continue;
            }

            position++;
        }

        return result.ToString();
    }
}
