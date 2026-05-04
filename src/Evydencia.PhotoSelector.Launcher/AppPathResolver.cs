namespace Evydencia.PhotoSelector.Launcher;

public sealed class AppPathResolver
{
    public const string AppPathEnvironmentVariable = "EVYDENCIA_PHOTO_SELECTOR_APP_EXE";
    private const string AppExecutableName = "Evydencia.PhotoSelector.App.exe";

    public string? Resolve(string? requestedAppPath)
    {
        if (IsExistingFile(requestedAppPath))
        {
            return Path.GetFullPath(requestedAppPath!);
        }

        var environmentPath = Environment.GetEnvironmentVariable(AppPathEnvironmentVariable);
        if (IsExistingFile(environmentPath))
        {
            return Path.GetFullPath(environmentPath!);
        }

        var siblingPath = Path.Combine(AppContext.BaseDirectory, AppExecutableName);
        return IsExistingFile(siblingPath)
            ? Path.GetFullPath(siblingPath)
            : null;
    }

    private static bool IsExistingFile(string? path)
    {
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }
}
