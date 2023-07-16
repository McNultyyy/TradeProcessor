using Hangfire.Dashboard;

namespace TradeProcessor.Api.Authorization;

public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}