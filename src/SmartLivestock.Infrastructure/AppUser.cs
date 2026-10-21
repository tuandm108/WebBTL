using Microsoft.AspNetCore.Identity;

namespace SmartLivestock.Infrastructure;

public sealed class AppUser : IdentityUser
{
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }
    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;
}
