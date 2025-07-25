using CliWrap;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace TailwindBuild.Tasks;

public sealed class BuildTailwind : Task
{
    [Required] public string StandaloneCliPath { get; set; } = string.Empty;
    [Required] public string WorkingDir { get; set; } = string.Empty;

    [Required] public string InputFile { get; set; } = string.Empty;

    [Required] public string OutputFile { get; set; } = string.Empty;

    [Required] public bool Minify { get; set; }

    [Required] public bool Watch { get; set; }

    public override bool Execute()
    {
        if (!File.Exists(StandaloneCliPath))
        {
            Log.LogError("Could not find Tailwind CLI: {0}", StandaloneCliPath);
            return false;
        }

        if (!File.Exists(InputFile))
        {
            Log.LogWarning("Could not find input file {0}", InputFile);
            return false;
        }

        Cli.Wrap(StandaloneCliPath)
            .WithEnvironmentVariables(new Dictionary<string, string?>
            {
                { "NO_COLOR", "true" }
            })
            .WithWorkingDirectory(WorkingDir)
            .WithArguments($"-i {InputFile} -o {OutputFile} {(Minify ? "--minify" : string.Empty)} {(Watch ? "--watch" : string.Empty)}")
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
            .ExecuteAsync()
            .GetAwaiter()
            .GetResult();

        return !Log.HasLoggedErrors;
    }
}