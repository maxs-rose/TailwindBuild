using Microsoft.Build.Framework;
using NSubstitute;
using Refit;
using TailwindBuild.Clients;
using TailwindBuild.Tasks;

namespace TailwindBuild.Test.Tasks;

public sealed class DownloadTailwindCliTest : IDisposable
{
    private readonly DirectoryInfo _tempDir = Directory.CreateTempSubdirectory();
    private readonly string IsCi = Environment.GetEnvironmentVariable("CI") ?? string.Empty;

    public void Dispose()
    {
        _tempDir.Delete(true);
    }

    [Fact]
    private async Task ShouldDownloadLatest()
    {
        Assert.SkipWhen(IsCi == "true", "Skipping on CI");

        // Setup
        var client = RestService.For<ITailwindClient>("https://api.github.com");
        var latestVersionResponse = await client.GetLatest(CancellationToken.None);
        await latestVersionResponse.EnsureSuccessStatusCodeAsync();
        var latestVersion = latestVersionResponse.Content!.Version;

        var sut = new DownloadTailwindCli
        {
            Version = "latest",
            RootPath = _tempDir.FullName,
            FileName = "tailwindcss-windows-x64.exe",
            BuildEngine = Substitute.For<IBuildEngine>()
        };

        // Execute
        var result = sut.Execute();

        // Verify
        Assert.True(result);

        var expectedFileName = Path.Combine(_tempDir.FullName, latestVersion, "tailwindcss-windows-x64.exe");
        Assert.True(File.Exists(expectedFileName));
    }

    [Fact]
    private async Task ShouldDownloadSpecificVersion()
    {
        Assert.SkipWhen(IsCi == "true", "Skipping on CI");

        // Setup
        var client = RestService.For<ITailwindClient>("https://api.github.com");
        var latestVersionResponse = await client.GetVersion("v4.1.7", CancellationToken.None);
        await latestVersionResponse.EnsureSuccessStatusCodeAsync();
        var latestVersion = latestVersionResponse.Content!.Version;

        var sut = new DownloadTailwindCli
        {
            Version = "v4.1.7",
            RootPath = _tempDir.FullName,
            FileName = "tailwindcss-windows-x64.exe",
            BuildEngine = Substitute.For<IBuildEngine>()
        };

        // Execute
        var result = sut.Execute();

        // Verify
        Assert.True(result);

        var expectedFileName = Path.Combine(_tempDir.FullName, latestVersion, "tailwindcss-windows-x64.exe");
        Assert.True(File.Exists(expectedFileName));
    }

    [Theory]
    [InlineData("4.0.0")]
    [InlineData("41.0.0")]
    [InlineData("v3.0.0")]
    [InlineData("3.0.0")]
    [InlineData("5.0.0")]
    [InlineData("v5.0.0")]
    [InlineData("v43.0.0")]
    private void ShouldThrowOnInvalidVersion(string version)
    {
        // Setup
        var sut = new DownloadTailwindCli
        {
            Version = version,
            RootPath = _tempDir.FullName,
            FileName = "tailwindcss-windows-x64.exe",
            BuildEngine = Substitute.For<IBuildEngine>()
        };

        // Execute
        // Verify
        var result = Assert.Throws<ArgumentOutOfRangeException>(() => sut.Execute());
        Assert.Equal($"Invalid version {version}. Must be within v4.x.x", result.ParamName);
    }
}