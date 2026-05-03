using Evydencia.PhotoSelector.Application.Models;

namespace Evydencia.PhotoSelector.Application.Activation;

public sealed record OpenFolderFromArgumentsResult(
    FolderLaunchArguments LaunchArguments,
    OpenFolderFromArgumentsStatus Status,
    OpenSessionResult? SessionResult,
    string? ErrorMessage)
{
    public static OpenFolderFromArgumentsResult NoFolderArgument(FolderLaunchArguments launchArguments)
    {
        return new OpenFolderFromArgumentsResult(
            launchArguments,
            OpenFolderFromArgumentsStatus.NoFolderArgument,
            null,
            null);
    }

    public static OpenFolderFromArgumentsResult Opened(
        FolderLaunchArguments launchArguments,
        OpenSessionResult sessionResult)
    {
        return new OpenFolderFromArgumentsResult(
            launchArguments,
            OpenFolderFromArgumentsStatus.Opened,
            sessionResult,
            null);
    }

    public static OpenFolderFromArgumentsResult Failed(
        FolderLaunchArguments launchArguments,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new OpenFolderFromArgumentsResult(
            launchArguments,
            OpenFolderFromArgumentsStatus.Failed,
            null,
            exception.Message);
    }
}
