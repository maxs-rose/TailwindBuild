using System.Text.Json.Serialization;

namespace TailwindBuild.Models;

internal sealed record GithubReleaseAsset([property: JsonPropertyName("id")] ulong Id, [property: JsonPropertyName("name")] string Name);