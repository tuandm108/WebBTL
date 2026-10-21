using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartLivestock.Contracts;
using SmartLivestock.Infrastructure;

namespace SmartLivestock.Web;

public static class AuthEndpoints
{
    public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration config, LocalAuthSettings? local)
    {
        await using var scope = services.CreateAsyncScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        if (!await roles.RoleExistsAsync("Admin"))
        {
            var result = await roles.CreateAsync(new IdentityRole("Admin"));
            if (!result.Succeeded) throw new InvalidOperationException("Could not create Admin role");
        }
        if (await users.GetUsersInRoleAsync("Admin") is { Count: > 0 })
        {
            if (local?.AdminPassword is not null) LocalAuthSettings.RemoveBootstrapPassword();
            return;
        }
        var email = config["BootstrapAdmin:Email"] ?? local?.AdminEmail;
        var password = config["BootstrapAdmin:Password"] ?? local?.AdminPassword;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configure BootstrapAdmin:Email and Password for the first admin");
        if (!new EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException("Bootstrap admin email is invalid");
        var user = new AppUser { UserName = email.Trim(), Email = email.Trim(), EmailConfirmed = true,
            ApprovedAt = DateTimeOffset.UtcNow };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new InvalidOperationException("Bootstrap admin password does not meet Identity password rules: " +
                string.Join(", ", created.Errors.Select(x => x.Description)));
        var assigned = await users.AddToRoleAsync(user, "Admin");
        if (!assigned.Succeeded) throw new InvalidOperationException("Could not assign Admin role");
        if (local?.AdminPassword is not null) LocalAuthSettings.RemoveBootstrapPassword();
    }

    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/login", (HttpContext ctx, IAntiforgery xsrf) => Page(ctx, xsrf, "Đăng nhập", "/login",
            LoginFields(), "Đăng nhập", "Chưa có tài khoản?", "/register", "Đăng ký", "Chào mừng trở lại", "Đăng nhập để tiếp tục theo dõi trang trại."));

        app.MapPost("/login", async (HttpContext ctx, IAntiforgery xsrf,
            SignInManager<AppUser> signIn, UserManager<AppUser> users) =>
        {
            if (!await xsrf.IsRequestValidAsync(ctx)) return Results.BadRequest();
            var form = await ctx.Request.ReadFormAsync();
            var email = form["email"].ToString().Trim();
            var user = await users.FindByEmailAsync(email);
            if (user is not null)
            {
                var result = await signIn.PasswordSignInAsync(user, form["password"].ToString(), false, true);
                if (result.Succeeded) return Results.Redirect("/");
            }
            return Page(ctx, xsrf, "Đăng nhập", "/login",
                LoginFields(email), "Đăng nhập", "Chưa có tài khoản?", "/register", "Đăng ký",
                "Chào mừng trở lại", "Đăng nhập để tiếp tục theo dõi trang trại.",
                "Thông tin đăng nhập không đúng, tài khoản chưa được duyệt hoặc đã bị khóa.");
        }).DisableAntiforgery().RequireRateLimiting("auth");

        app.MapGet("/register", (HttpContext ctx, IAntiforgery xsrf) => Page(ctx, xsrf, "Đăng ký", "/register",
            RegistrationFields(), "Gửi đăng ký", "Đã có tài khoản?", "/login", "Đăng nhập",
            "Tạo tài khoản", "Đăng ký để gửi yêu cầu truy cập đến admin."));

        app.MapPost("/register", async (HttpContext ctx, IAntiforgery xsrf, UserManager<AppUser> users) =>
        {
            if (!await xsrf.IsRequestValidAsync(ctx)) return Results.BadRequest();
            var form = await ctx.Request.ReadFormAsync();
            var email = form["email"].ToString().Trim();
            var password = form["password"].ToString();
            var confirm = form["confirm"].ToString();
            string? error = null;
            if (!new EmailAddressAttribute().IsValid(email) || email.Length > 256) error = "Email không hợp lệ.";
            else if (password != confirm) error = "Hai mật khẩu không khớp.";
            else if (password.Length > 128) error = "Mật khẩu quá dài.";
            else if (await users.FindByEmailAsync(email) is not null) error = "Email đã được sử dụng.";
            else
            {
                var user = new AppUser { UserName = email, Email = email };
                var result = await users.CreateAsync(user, password);
                if (result.Succeeded)
                    return MessagePage("Đã gửi đăng ký", "success", "Yêu cầu đã được gửi",
                        "Tài khoản đang chờ admin chấp thuận. Bạn có thể đăng nhập sau khi được duyệt.",
                        "/login", "Về trang đăng nhập");
                error = string.Join(" ", result.Errors.Select(x => x.Description));
            }
            return Page(ctx, xsrf, "Đăng ký", "/register",
                RegistrationFields(email), "Gửi đăng ký", "Đã có tài khoản?", "/login", "Đăng nhập",
                "Tạo tài khoản", "Đăng ký để gửi yêu cầu truy cập đến admin.", error);
        }).DisableAntiforgery().RequireRateLimiting("auth");

        app.MapGet("/logout", (HttpContext ctx, IAntiforgery xsrf) => Page(ctx, xsrf, "Đăng xuất", "/logout",
            "", "Đăng xuất", "", "/", "Quay lại trang chủ",
            "Kết thúc phiên làm việc?", "Bạn sẽ cần đăng nhập lại để truy cập trang trại."))
            .RequireAuthorization();
        app.MapPost("/logout", async (HttpContext ctx, IAntiforgery xsrf, SignInManager<AppUser> signIn) =>
        {
            if (!await xsrf.IsRequestValidAsync(ctx)) return Results.BadRequest();
            await signIn.SignOutAsync();
            return Results.Redirect("/logged-out");
        }).DisableAntiforgery().RequireAuthorization();
        app.MapGet("/account/password-changed", async (SignInManager<AppUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.Redirect("/logged-out?reason=password");
        }).RequireAuthorization();
        app.MapGet("/logged-out", (HttpContext ctx) =>
            ctx.Request.Query["reason"] == "password"
                ? MessagePage("Đã đổi mật khẩu", "success", "Mật khẩu đã được cập nhật",
                    "Phiên hiện tại đã kết thúc. Hãy đăng nhập lại bằng mật khẩu mới.", "/login", "Đăng nhập lại")
                : MessagePage("Đã đăng xuất", "success", "Bạn đã đăng xuất",
                    "Phiên làm việc đã kết thúc an toàn.", "/login", "Đăng nhập lại"));
        app.MapGet("/denied", () => MessagePage("Không có quyền", "warning", "Không có quyền truy cập",
            "Tài khoản của bạn không có quyền xem trang này.", "/", "Về trang chủ"));
    }

    private static IResult Page(HttpContext ctx, IAntiforgery xsrf, string title, string action,
        string fields, string button, string linkLead, string linkUrl, string linkText,
        string heading, string description, string? error = null)
    {
        var token = xsrf.GetAndStoreTokens(ctx).RequestToken;
        var html = $"<div class='auth-intro'><span class='auth-eyebrow'>{title}</span><h1>{heading}</h1><p>{description}</p></div>" +
            (error is null ? "" : $"<div role='alert' class='auth-alert'>{WebUtility.HtmlEncode(error)}</div>") +
            $"<form class='auth-form' method='post' action='{action}'>" +
            $"<input type='hidden' name='__RequestVerificationToken' value='{WebUtility.HtmlEncode(token)}'>" +
            fields + $"<button class='auth-button' type='submit'>{button}</button></form>" +
            $"<div class='auth-footer'>{linkLead} <a href='{linkUrl}'>{linkText}</a></div>";
        return Results.Content(Layout(title, html), "text/html; charset=utf-8");
    }

    private static string LoginFields(string email = "") =>
        $"<div class='auth-field'><label for='auth-email'>Email</label><input id='auth-email' name='email' type='email' autocomplete='username' value='{WebUtility.HtmlEncode(email)}' placeholder='ban@example.com' required autofocus></div>" +
        "<div class='auth-field'><label for='auth-password'>Mật khẩu</label><input id='auth-password' name='password' type='password' autocomplete='current-password' placeholder='Nhập mật khẩu' required></div>";

    private static string RegistrationFields(string email = "") =>
        $"<div class='auth-field'><label for='auth-email'>Email</label><input id='auth-email' name='email' type='email' autocomplete='email' value='{WebUtility.HtmlEncode(email)}' placeholder='ban@example.com' required autofocus></div>" +
        "<div class='auth-field'><label for='auth-password'>Mật khẩu</label><input id='auth-password' name='password' type='password' autocomplete='new-password' minlength='12' maxlength='128' placeholder='Tạo mật khẩu mạnh' aria-describedby='password-hint' required></div>" +
        "<div class='auth-field'><label for='auth-confirm'>Nhập lại mật khẩu</label><input id='auth-confirm' name='confirm' type='password' autocomplete='new-password' placeholder='Nhập lại mật khẩu' required></div>" +
        "<p id='password-hint' class='auth-hint'>Ít nhất 12 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.</p>";

    private static IResult MessagePage(string title, string tone, string heading, string description,
        string linkUrl, string linkText)
    {
        var html = $"<div class='auth-message'><div class='auth-status auth-status-{tone}' aria-hidden='true'>{(tone == "success" ? "✓" : "!")}</div>" +
            $"<span class='auth-eyebrow'>{title}</span><h1>{heading}</h1><p>{description}</p>" +
            $"<a class='auth-button' href='{linkUrl}'>{linkText}</a></div>";
        return Results.Content(Layout(title, html), "text/html; charset=utf-8");
    }

    private static string Layout(string title, string body) =>
        "<!doctype html><html lang='vi'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><meta name='color-scheme' content='light'>" +
        $"<title>{title} | Smart Livestock</title>" +
        "<link rel='stylesheet' href='/auth.css'><link rel='icon' type='image/png' href='/favicon.png'>" +
        "</head><body><main class='auth-shell'><div class='auth-brand' aria-label='Smart Livestock'><span class='auth-mark'>SL</span><span>Smart Livestock</span></div>" +
        $"<section class='auth-card'>{body}</section><p class='auth-caption'>Hệ thống quản lý trang trại thông minh</p></main></body></html>";
}
