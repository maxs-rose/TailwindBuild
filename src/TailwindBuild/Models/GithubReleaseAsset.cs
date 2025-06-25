using System.Text.Json.Serialization;

namespace TailwindBuild.Models;

internal sealed class GithubReleaseAsset
{
    [property: JsonPropertyName("id")] public ulong Id { get; set; }
    [property: JsonPropertyName("name")] public string Name { get; set; }
}