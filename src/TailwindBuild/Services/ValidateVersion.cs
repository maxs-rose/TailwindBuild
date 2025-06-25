namespace TailwindBuild.Services;

internal sealed class ValidateVersion
{
    public static bool IsValid(string version)
    {
        return version == "latest" || version.StartsWith("v4.");
    }
}