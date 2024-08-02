using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using FamilySync.Blazor.Web.Clients;

namespace FamilySync.Blazor.Web.Services;

public interface ITokenService
{
    public Task CacheAccessToken(string token);
    public Task ClearAccessToken();
    public Task<string?> GetAccessToken();
    Task<bool> ValidateAccessToken();
    public Task<bool> RefreshAccessToken();
}

public class TokenService : ITokenService
{
    private readonly IIdentityClient _client;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtSecurityTokenHandler _handler;

    public TokenService(IIdentityClient client, ILocalStorageService localStorage)
    {
        _client = client;
        _localStorage = localStorage;
        _handler ??= new();
    }

    public async Task CacheAccessToken(string token) =>
        await _localStorage.SetItemAsStringAsync("familysync.access", token);

    public async Task<string?> GetAccessToken() =>
        await _localStorage.GetItemAsStringAsync("familysync.access");

    public async Task ClearAccessToken() =>
        await _localStorage.RemoveItemAsync("familysync.access");

    public async Task<bool> ValidateAccessToken()
    {
        var token = await GetAccessToken();

        if (string.IsNullOrWhiteSpace(token))
            return false;

        var content = _handler.ReadJwtToken(token);

        return DateTime.UtcNow <= content.ValidTo;
    }
    
    public async Task<bool> RefreshAccessToken()
    {
        var newAccessToken = await _client.RefreshAccessToken();

        if (newAccessToken is null)
            return false;

        await CacheAccessToken(newAccessToken);

        return true;
    }

}