using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FamilySync.Blazor.Web.Clients;
using FamilySync.Blazor.Web.Models.Requests;

namespace FamilySync.Blazor.Web.Services;

public interface IAuthenticationFacade
{
    event Func<ClaimsPrincipal, Task> NotifyUserLogin;
    event Func<Task> NotifyUserLogout;
    
    /// <summary>
    /// Tries to validate Access Token from Local Storage <see cref="TokenService.ValidateAccessToken"/>
    /// </summary>
    /// <returns>The authorized user or Anonymous user (not authorized user) </returns>
    Task<ClaimsPrincipal> TryGetAuthenticatedUser();
    Task<bool> Login(Login request);
    Task Logout();
    Task<string?> Test();
}

public class AuthenticationFacade : IAuthenticationFacade
{
    public event Func<ClaimsPrincipal, Task> NotifyUserLogin;
    public event Func<Task> NotifyUserLogout;

    private readonly IIdentityClient _client;
    private readonly ITokenService _tokenService;

    private JwtSecurityTokenHandler _handler;

    public AuthenticationFacade(IIdentityClient client, ITokenService tokenService)
    {
        _client = client;
        _tokenService = tokenService;
        _handler ??= new();
    }

   
    public async Task<ClaimsPrincipal> TryGetAuthenticatedUser()
    {
        var anonymousState = new ClaimsPrincipal();

        // Check if accessToken is valid otherwise we should try and refresh it.
        if (!await _tokenService.ValidateAccessToken())
        {
            var refreshed = await _tokenService.RefreshAccessToken();

            if (!refreshed)
            {
                await _tokenService.ClearAccessToken();
                await _client.Logout();
                return anonymousState;
            }
        }
        
        var accessToken = await _tokenService.GetAccessToken();
        
        if (string.IsNullOrWhiteSpace(accessToken))
            return anonymousState;
        
        var principal = ReadAccessToken(accessToken);
        
        return new(principal);
    }

    public async Task<bool> Login(Login request)
    {
        var accessToken = await _client.Login(request);
        
        if (string.IsNullOrEmpty(accessToken))
        {
            return false;
        }

        await _tokenService.CacheAccessToken(accessToken);

        await NotifyUserLogin(ReadAccessToken(accessToken));
        
        return true;
    }

    public async Task Logout()
    {
        await _tokenService.ClearAccessToken();
        await _client.Logout();
        
        await NotifyUserLogout();
    }

    public async Task<string?> Test()
    {
        var token = await _tokenService.GetAccessToken();

        var text = await _client.Test(token!);

        return text;
    }

    private ClaimsPrincipal ReadAccessToken(string accessToken)
    {
        var content = _handler.ReadJwtToken(accessToken);
        var claims = content.Claims;
        var principal = new ClaimsIdentity(claims, "jwt");
        
        return new(principal);
    }
}