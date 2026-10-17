using SmartLivestock.Web.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient("api", client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5215"));

builder.Services.AddScoped<SmartLivestock.Web.Services.AlertFeed>();
builder.Services.AddScoped<SmartLivestock.Web.Services.AlertSettingsStore>();
builder.Services.AddScoped<SmartLivestock.Web.Services.PageSizeStore>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
