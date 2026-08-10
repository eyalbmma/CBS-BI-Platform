namespace CBS.BI.Application.Security.Models;

public sealed class CurrentUser
{
    public required string UserId { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}
