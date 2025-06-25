using System.Text.Json.Serialization;

namespace TailwindBuild.Models;

internal sealed record GithubRelease(
    [property: JsonPropertyName("tag_name")]
    string Version,
    [property: JsonPropertyName("assets")] IEnumerable<GithubReleaseAsset> Assets);