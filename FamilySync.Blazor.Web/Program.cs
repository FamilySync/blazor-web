using Blazored.LocalStorage;
using FamilySync.Blazor.Web.Clients;
using FamilySync.Blazor.Web.Models.Interceptors;
using FamilySync.Blazor.Web.Services;
using FamilySync.Blazor.Web;
using FamilySync.Blazor.Web.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthenticationFacade, AuthenticationFacade>();
builder.Services.AddScoped<FamilySyncAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(x => x.GetRequiredService<FamilySyncAuthenticationStateProvider>());

builder.Services.AddTransient<CookieInterceptor>();
builder.Services.AddHttpClient<IIdentityClient, IdentityClient>(opt =>
    {
        // TODO: Implement into appsettings
        opt.BaseAddress = new("https://localhost:44343/api/v1");
        // opt.Timeout = TimeSpan.FromSeconds(15);
    })
    .AddHttpMessageHandler<CookieInterceptor>();


await builder.Build().RunAsync();