namespace TailwindBuild.Services;

internal sealed class ValidateVersion
{
    public static bool IsValid(string version)
    {
        return version == "latest" || version.StartsWith("v4.");
    }

    public static bool IsInstalled(string version, string location)
    {
        if (version == "latest")
            return false;

        return File.Exists(location);
    }
}