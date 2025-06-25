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
    [Required] public string Version { get; set; } = string.Empty;
    [Required] public string RootPath { get; set; } = string.Empty;
    public string? FileName { get; set; }


    [Output] public string StandaloneCliPath { get; set; } = string.Empty;

    private string CliPath => Path.Combine(RootPath, Version);
    private string CLiLocation => Path.Combine(CliPath, FileName ?? SystemInfo.GetFileName());

    public override bool Execute()
    {
        if (File.Exists(StandaloneCliPath))
        {
            Log.LogMessage(MessageImportance.Normal, "Using cached cli located at: {0}", StandaloneCliPath);
            return true;
        }

        var cts = new CancellationTokenSource();

        try
        {
            var result = ExecuteAsync(cts.Token).GetAwaiter().GetResult();
            StandaloneCliPath = CLiLocation;

            return result && !Log.HasLoggedErrors;
        }
        catch
        {
            cts.Cancel();
        }
        finally
        {
            cts.Dispose();
        }

        return !Log.HasLoggedErrors;
    }

    private async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        var client = RestService.For<ITailwindClient>("https://api.github.com");

        var releaseAsset = await GetAsset(client, Version, cancellationToken);

        Log.LogMessage(MessageImportance.Low, "Downloading: {0}", releaseAsset);

        await Download(client, releaseAsset, cancellationToken);

        Log.LogMessage(MessageImportance.Low, "Downloaded: {0}", releaseAsset);

        return true;
    }

    private async Task<ulong> GetAsset(
        ITailwindClient client,
        string releaseVersion,
        CancellationToken cancellationToken)
    {
        var releaseResponse = await client.GetReleases(
            releaseVersion switch
            {
                "latest" => "latest",
                _ => $"tags/{releaseVersion}"
            },
            cancellationToken);

        if (!releaseResponse.IsSuccessful || releaseResponse.Content is null)
            throw new Exception($"Could not find TailwindCLI release for {releaseVersion}");

        var fileName = FileName ?? SystemInfo.GetFileName();

        var asset = releaseResponse.Content.Assets.FirstOrDefault(r => string.Equals(r.Name, fileName, StringComparison.InvariantCultureIgnoreCase))
                    ?? throw new Exception($"Could not find TailwindCLI release asset with name {fileName}");

        return asset.Id;
    }

    private async System.Threading.Tasks.Task Download(ITailwindClient client, ulong assetId, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(CliPath);
        var data = await client.DownloadAsset(assetId, cancellationToken);

        await data.EnsureSuccessStatusCodeAsync();

        using var outputFile = File.Create(CLiLocation);
        await data.Content!.CopyToAsync(outputFile);
        outputFile.Close();

        await SetPosixFilePermissions();
    }

    private async ValueTask SetPosixFilePermissions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        await Cli.Wrap("chmod")
            .WithArguments($"-x {CLiLocation}")
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