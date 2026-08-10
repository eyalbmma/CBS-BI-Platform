using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Security.Abstractions;

public interface ICurrentUserContext
{
  CurrentUser GetCurrentUser();
}
