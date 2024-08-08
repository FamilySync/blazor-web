using System.Net.Http.Json;
using FamilySync.Blazor.Web.Models.DTOs;
using FamilySync.Blazor.Web.Models.Requests;

namespace FamilySync.Blazor.Web.Clients;

public interface IIdentityClient
{
    public Task<IdentityDTO> Register(SignupRequest request);
    public Task<string> Signin(SigninRequest request);
    public Task<string?> RefreshAccessToken();
    public Task Signout();
    public Task<string?> Test(string accessToken);
}

public class IdentityClient : IIdentityClient
{
    private readonly HttpClient _client;

    public IdentityClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<IdentityDTO> Register(SignupRequest request)
    {
        var response = await _client.PostAsJsonAsync($"{_client.BaseAddress}/identities", request);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: Setup exception handling in the browser ..
            throw new Exception($"Failed to register!: {response.ReasonPhrase}");
        }

        var dto = await response.Content.ReadFromJsonAsync<IdentityDTO>();

        if (dto is null)
        {
            throw new Exception("Failed to read dto!");
        }

        return dto;
    }

    public async Task<string> Signin(SigninRequest request)
    {
        var response = await _client.PostAsJsonAsync($"{_client.BaseAddress}/sessions", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException($"Signin endpoint failed!: {response.ReasonPhrase}");
        }

        var dto = await response.Content.ReadFromJsonAsync<AuthTokenDTO>();
        
        if (dto is null)
        {
            throw new UnauthorizedAccessException("Failed to read AuthToken!");
        }
        
        return dto.AccessToken;
    }

    public async Task Signout()
    {
        var response = await _client.DeleteAsync($"{_client.BaseAddress}/sessions/current");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Something went wrong when logging out! {response.ReasonPhrase}");
        }
    }
    
    public async Task<string?> RefreshAccessToken()
    {
        var response = await _client.PutAsync($"{_client.BaseAddress}/sessions/current", null);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to refresh Access Token! {response.ReasonPhrase}");
            return null;
        }

        var authToken = await response.Content.ReadFromJsonAsync<AuthTokenDTO>();

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