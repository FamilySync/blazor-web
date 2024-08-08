using System.Security.Claims;
using FamilySync.Blazor.Web.Models.Entity;
using FamilySync.Blazor.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace FamilySync.Blazor.Web.Providers;

public sealed class FamilySyncAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    public ApplicationUser? CurrentUser { get; set; } = default;
    private readonly IAuthenticationFacade _authFacade;

    public FamilySyncAuthenticationStateProvider(IAuthenticationFacade authFacade)
    {
        _authFacade = authFacade;

        _authFacade.NotifyUserSignin += OnNotifyUserSignin;
        _authFacade.NotifyUserSignout += OnNotifyUserSignout;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = await _authFacade.TryGetAuthenticatedUser();
        
        CurrentUser = new() { Username = principal.Claims.FirstOrDefault(x => x.Type == "sub")?.Value };
        
        return new(principal);
    }

    private Task OnNotifyUserSignin(ClaimsPrincipal principal)
    {
        // TODO: CurrentUser needs a better implementation of getting the users data when they are authenticated ...
        
        var state = new AuthenticationState(principal);
        NotifyAuthenticationStateChanged(Task.FromResult(state));

        CurrentUser = new() { Username = principal.Claims.FirstOrDefault(x => x.Type == "sub")!.Value };
        
        return Task.CompletedTask;
    }

    private Task OnNotifyUserSignout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
        CurrentUser = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _authFacade.NotifyUserSignin -= OnNotifyUserSignin;
        _authFacade.NotifyUserSignout -= OnNotifyUserSignout;
    }
}