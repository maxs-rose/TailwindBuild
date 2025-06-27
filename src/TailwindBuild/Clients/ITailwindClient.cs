using System.Net.Http.Headers;
using Refit;
using TailwindBuild.Models;

namespace TailwindBuild.Clients;

[Headers("User-Agent: TailwindBuild.TailwindBuild Zadosx")]
internal interface ITailwindClient
{
    [Get("/repos/tailwindlabs/tailwindcss/releases/latest")]
    Task<ApiResponse<GithubRelease>> GetLatest(CancellationToken cancellationToken);

    [Get("/repos/tailwindlabs/tailwindcss/releases/tags/{release}")]
    Task<ApiResponse<GithubRelease>> GetVersion(string release, CancellationToken cancellationToken);

    [Headers("Accept: application/octet-stream")]
    [Get("/repos/tailwindlabs/tailwindcss/releases/assets/{asset}")]
    Task<ApiResponse<Stream>> DownloadAsset(ulong asset, CancellationToken cancellationToken = default);
}

internal class AuthenticationHandler : DelegatingHandler
{
    private readonly string? _authToken;

    public AuthenticationHandler(string? authToken)
    {
        _authToken = authToken;
        InnerHandler = new HttpClientHandler();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_authToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        return base.SendAsync(request, cancellationToken);
    }
}