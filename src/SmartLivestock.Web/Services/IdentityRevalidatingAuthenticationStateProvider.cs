using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Web.Services;

public sealed class IdentityRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> options) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await users.GetUserAsync(authenticationState.User);
        if (user is null || user.ApprovedAt is null || user.DisabledAt is not null || await users.IsLockedOutAsync(user)) return false;
        var claim = authenticationState.User.FindFirst(options.Value.ClaimsIdentity.SecurityStampClaimType)?.Value;
        return claim is not null && claim == await users.GetSecurityStampAsync(user);
    }
}
