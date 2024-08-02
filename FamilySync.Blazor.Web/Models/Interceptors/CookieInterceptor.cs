using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace FamilySync.Blazor.Web.Models.Interceptors;

/// <summary>
/// Allows the backend to set cookies in the response.
/// </summary>
/// <remarks>
/// This client should only be used for performing CRUD operations on our own backend.
/// It is not intended for use with external services.
/// </remarks>
public class CookieInterceptor : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);
        return base.SendAsync(request, cancellationToken);
    }
}