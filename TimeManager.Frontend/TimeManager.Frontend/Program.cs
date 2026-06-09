using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using TimeManager.Frontend.Auth;
using TimeManager.Frontend.Components;
using TimeManager.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<CookieContainer>();

builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7263/");
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var cookieContainer = sp.GetRequiredService<CookieContainer>();
    return new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = cookieContainer
    };
});

builder.Services.AddScoped<CookieAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CookieAuthStateProvider>());

builder.Services.AddAuthorizationCore();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    //.AddAdditionalAssemblies(typeof(TimeManager.Frontend.Client._Imports).Assembly);

app.Run();
