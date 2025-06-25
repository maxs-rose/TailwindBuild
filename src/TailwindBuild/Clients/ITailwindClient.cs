using Refit;
using TailwindBuild.Models;

namespace TailwindBuild.Clients;

[Headers("User-Agent: TailwindBuild.TailwindBuild Zadosx")]
internal interface ITailwindClient
{
    [Get("/repos/tailwindlabs/tailwindcss/releases/{release}")]
    Task<ApiResponse<GithubRelease>> GetReleases(string release, CancellationToken cancellationToken);

    [Headers("Accept: application/octet-stream")]
    [Get("/repos/tailwindlabs/tailwindcss/releases/assets/{asset}")]
    Task<ApiResponse<Stream>> DownloadAsset(ulong asset, CancellationToken cancellationToken = default);
}