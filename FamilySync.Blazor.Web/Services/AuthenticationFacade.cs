using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FamilySync.Blazor.Web.Clients;
using FamilySync.Blazor.Web.Models.DTOs;
using FamilySync.Blazor.Web.Models.Requests;

namespace FamilySync.Blazor.Web.Services;

public interface IAuthenticationFacade
{
    event Func<ClaimsPrincipal, Task> NotifyUserSignin;
    event Func<Task> NotifyUserSignout;
    
    /// <summary>
    /// Tries to validate Access Token from Local Storage <see cref="TokenService.ValidateAccessToken"/>
    /// </summary>
    /// <returns>The authorized user or Anonymous user (not authorized user) </returns>
    Task<ClaimsPrincipal> TryGetAuthenticatedUser();

    Task<bool> Register(SignupRequest request);
    Task<bool> Signin(SigninRequest request);
    Task Signout();
    Task<string?> Test();
}

public class AuthenticationFacade : IAuthenticationFacade
{
    public event Func<ClaimsPrincipal, Task> NotifyUserSignin;
    public event Func<Task> NotifyUserSignout;

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
                await _client.Signout();
                return anonymousState;
            }
        }
        
        var accessToken = await _tokenService.GetAccessToken();
        
        if (string.IsNullOrWhiteSpace(accessToken))
            return anonymousState;
        
        var principal = ReadAccessToken(accessToken);
        
        return new(principal);
    }

    public async Task<bool> Register(SignupRequest request)
    {
        await _client.Register(request);

        return true;
    }

    public async Task<bool> Signin(SigninRequest request)
    {
        var accessToken = await _client.Signin(request);
        
        if (string.IsNullOrEmpty(accessToken))
        {
            return false;
        }

        await _tokenService.CacheAccessToken(accessToken);

        await NotifyUserSignin(ReadAccessToken(accessToken));
        
        return true;
    }

    public async Task Signout()
    {
        await _tokenService.ClearAccessToken();
        await _client.Signout();
        
        await NotifyUserSignout();
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