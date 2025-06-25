using System.Text.Json.Serialization;

namespace TailwindBuild.Models;

internal sealed record GithubRelease
{
    [property: JsonPropertyName("assets")] public IEnumerable<GithubReleaseAsset> Assets { get; set; }
}