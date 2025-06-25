using System.Runtime.InteropServices;
using ProcessorArchitecture = Microsoft.Build.Utilities.ProcessorArchitecture;

namespace TailwindBuild.Services;

internal sealed class SystemInfo
{
    public static string GetFileName()
    {
        return Platform() switch
        {
            "windows" => $"tailwindcss-windows-{Architecture()}.exe",
            var x => $"tailwindcss-{x}-{Architecture()}"
        };
    }

    private static string Platform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "windows";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macos";

        throw new PlatformNotSupportedException("Please specify TailwindCLI file name to download");
    }

    private static string Architecture()
    {
        return ProcessorArchitecture.CurrentProcessArchitecture switch
        {
            ProcessorArchitecture.AMD64 => "x64",
            ProcessorArchitecture.ARM64 => "arm64",
            _ => throw new PlatformNotSupportedException("Please specify TailwindCLI file name to download")
        };
    }
}