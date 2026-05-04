namespace Evydencia.PhotoSelector.Launcher;

public sealed record LauncherParseResult(
    bool IsSuccess,
    LauncherOptions? Options,
    string? ErrorMessage)
{
    public static LauncherParseResult Success(LauncherOptions options)
    {
        return new LauncherParseResult(true, options, null);
    }

    public static LauncherParseResult Failure(string errorMessage)
    {
        return new LauncherParseResult(false, null, errorMessage);
    }
}
