using System.Net.Http.Json;
using FamilySync.Blazor.Web.Models.DTOs;
using FamilySync.Blazor.Web.Models.Requests;

namespace FamilySync.Blazor.Web.Clients;

public interface IIdentityClient
{
    public Task<string> Login(Login request);
    public Task<string?> RefreshAccessToken();
    public Task Logout();
    public Task<string?> Test(string accessToken);
}

public class IdentityClient : IIdentityClient
{
    private readonly HttpClient _client;

    public IdentityClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> Login(Login request)
    {
        var response = await _client.PostAsJsonAsync($"{_client.BaseAddress}/sessions", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException($"Login endpoint failed!: {response.ReasonPhrase}");
        }

        var authToken = await response.Content.ReadFromJsonAsync<AuthToken>();
        
        if (authToken is null)
        {
            throw new UnauthorizedAccessException("Failed to read AuthToken!");
        }
        
        return authToken.AccessToken;
    }

    public async Task Logout()
    {
        var response = await _client.DeleteAsync($"{_client.BaseAddress}/sessions/current");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Something went wrong when logging out! {response.ReasonPhrase}");
        }
    }

    public Task<string?> Test()
    {
        throw new NotImplementedException();
    }

    public async Task<string?> RefreshAccessToken()
    {
        var response = await _client.PutAsync($"{_client.BaseAddress}/sessions/current", null);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to refresh Access Token! {response.ReasonPhrase}");
            return null;
        }

        var authToken = await response.Content.ReadFromJsonAsync<AuthToken>();

        return authToken?.AccessToken;
    }

    
    public async Task<string?> Test(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_client.BaseAddress}/identities");
        request.Headers.Authorization = new("bearer", accessToken);
        
        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return $"{response.StatusCode}";
        }
        
        var content = await response.Content.ReadAsStringAsync();
        
        return content;
    }
}