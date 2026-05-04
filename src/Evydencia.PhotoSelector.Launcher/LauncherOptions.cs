namespace Evydencia.PhotoSelector.Launcher;

public sealed record LauncherOptions(
    string? FolderPath,
    string? AppPath,
    string Source,
    bool ShowHelp)
{
    public const string DefaultSource = "launcher";

    public bool HasFolder => !string.IsNullOrWhiteSpace(FolderPath);
}
