using Microsoft.AspNetCore.Identity;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Web.Services;

public sealed class ApprovalConfirmation : IUserConfirmation<AppUser>
{
    public Task<bool> IsConfirmedAsync(UserManager<AppUser> manager, AppUser user) =>
        Task.FromResult(user.ApprovedAt.HasValue && user.DisabledAt is null);
}
