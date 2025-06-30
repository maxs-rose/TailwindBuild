using System.Runtime.InteropServices;
using CliWrap;
using Microsoft.Build.Framework;
using Refit;
using TailwindBuild.Clients;
using TailwindBuild.Services;
using Task = Microsoft.Build.Utilities.Task;

namespace TailwindBuild.Tasks;

public sealed class DownloadTailwindCli : Task
{
    private string _cliPath = string.Empty;

    [Required] public string Version { get; set; } = string.Empty;
    [Required] public string RootPath { get; set; } = string.Empty;

    public string? FileName
    {
        set;
        get => string.IsNullOrWhiteSpace(field) ? SystemInfo.GetFileName() : field;
    }

    public string? AuthToken
    {
        set;
        get => string.IsNullOrWhiteSpace(field) ? null : field;
    }

    [Output] public string StandaloneCliPath { get; set; } = string.Empty;

    public override bool Execute()
    {
        if (!ValidateVersion.IsValid(Version))
            throw new ArgumentOutOfRangeException($"Invalid version {Version}. Must be within v4.x.x");

        var expectedPath = Path.Combine(RootPath, Version, FileName!);

        if (ValidateVersion.IsInstalled(Version, expectedPath))
        {
            Log.LogMessage("Using cached CLI version for {0} at {1}", Version, StandaloneCliPath);
            StandaloneCliPath = expectedPath;
            return true;
        }

        var result = ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

        return result && !Log.HasLoggedErrors;
    }

    private async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        var client = RestService.For<ITailwindClient>(new HttpClient(new AuthenticationHandler(AuthToken))
        {
            BaseAddress = new Uri("https://api.github.com")
        });

        var releaseAsset = await GetAsset(client, Version, cancellationToken);

        _cliPath = Path.Combine(RootPath, releaseAsset.Version);
        StandaloneCliPath = Path.Combine(_cliPath, FileName!);

        if (!ValidateVersion.IsInstalled(releaseAsset.Version, StandaloneCliPath))
            await Download(client, releaseAsset.AssetId, cancellationToken);
        else
            Log.LogMessage("Using cached CLI version for {0} at {1}", Version, StandaloneCliPath);

        return true;
    }

    private async Task<(string Version, ulong AssetId)> GetAsset(
        ITailwindClient client,
        string releaseVersion,
        CancellationToken cancellationToken)
    {
        var releaseResponse = await (releaseVersion switch
        {
            "latest" => client.GetLatest(cancellationToken),
            _ => client.GetVersion(releaseVersion, cancellationToken)
        });

        if (!releaseResponse.IsSuccessful || releaseResponse.Content is null)
            throw new Exception($"Could not find TailwindCLI release for {releaseVersion} {releaseResponse.ReasonPhrase}");

        if (!ValidateVersion.IsValid(Version))
            throw new ArgumentOutOfRangeException($"Invalid version {Version}. Must be within v4.x.x");

        var asset = releaseResponse.Content.Assets.FirstOrDefault(r => string.Equals(r.Name, FileName, StringComparison.InvariantCultureIgnoreCase))
                    ?? throw new Exception($"Could not find TailwindCLI release asset with name {FileName}");

        return (releaseResponse.Content.Version, asset.Id);
    }

    private async ValueTask Download(ITailwindClient client, ulong assetId, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_cliPath);
        var data = await client.DownloadAsset(assetId, cancellationToken);

        await data.EnsureSuccessStatusCodeAsync();

        await using var outputFile = File.Create(StandaloneCliPath);
        await data.Content!.CopyToAsync(outputFile, cancellationToken);
        outputFile.Close();

        await SetPosixFilePermissions();
    }

    private async ValueTask SetPosixFilePermissions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        await Cli.Wrap("chmod")
            .WithArguments($"-x {StandaloneCliPath}")
            .WithStandardOutputPipe(PipeTarget.ToDelegate(x =>
            {
                if (!string.IsNullOrWhiteSpace(x))
                    Log.LogMessage(x);
            }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(x =>
            {
                if (!string.IsNullOrWhiteSpace(x))
                    Log.LogMessage(MessageImportance.High, x);
            }))
            .ExecuteAsync();
    }
}