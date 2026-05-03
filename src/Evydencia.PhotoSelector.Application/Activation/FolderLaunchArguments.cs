namespace Evydencia.PhotoSelector.Application.Activation;

public sealed record FolderLaunchArguments(string? FolderPath, string? Source)
{
    public static FolderLaunchArguments Empty { get; } = new(null, null);

    public bool HasFolder => !string.IsNullOrWhiteSpace(FolderPath);
}
