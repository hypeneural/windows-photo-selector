namespace Evydencia.PhotoSelector.Application.Models;

public sealed class FileMoveResult
{
    private FileMoveResult(
        string sourcePath,
        string requestedDestinationPath,
        string? actualDestinationPath,
        bool collisionResolved,
        DateTimeOffset? preservedLastWriteTimeUtc,
        FileMoveErrorCode errorCode,
        string? errorMessage)
    {
        SourcePath = sourcePath;
        RequestedDestinationPath = requestedDestinationPath;
        ActualDestinationPath = actualDestinationPath;
        CollisionResolved = collisionResolved;
        PreservedLastWriteTimeUtc = preservedLastWriteTimeUtc;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public string SourcePath { get; }

    public string RequestedDestinationPath { get; }

    public string? ActualDestinationPath { get; }

    public bool CollisionResolved { get; }

    public DateTimeOffset? PreservedLastWriteTimeUtc { get; }

    public FileMoveErrorCode ErrorCode { get; }

    public string? ErrorMessage { get; }

    public bool IsSuccess => ErrorCode == FileMoveErrorCode.None;

    public static FileMoveResult Success(
        string sourcePath,
        string requestedDestinationPath,
        string actualDestinationPath,
        bool collisionResolved,
        DateTimeOffset preservedLastWriteTimeUtc)
    {
        return new FileMoveResult(
            sourcePath,
            requestedDestinationPath,
            actualDestinationPath,
            collisionResolved,
            preservedLastWriteTimeUtc,
            FileMoveErrorCode.None,
            errorMessage: null);
    }

    public static FileMoveResult Failure(
        string sourcePath,
        string requestedDestinationPath,
        FileMoveErrorCode errorCode,
        string errorMessage)
    {
        if (errorCode == FileMoveErrorCode.None)
        {
            throw new ArgumentException("Failure results must include an error code.", nameof(errorCode));
        }

        return new FileMoveResult(
            sourcePath,
            requestedDestinationPath,
            actualDestinationPath: null,
            collisionResolved: false,
            preservedLastWriteTimeUtc: null,
            errorCode,
            errorMessage);
    }
}
