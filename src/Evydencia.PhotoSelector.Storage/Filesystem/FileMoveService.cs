using System.Globalization;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;

namespace Evydencia.PhotoSelector.Storage.Filesystem;

public sealed class FileMoveService : IFileMoveService
{
    private const int ErrorSharingViolation = 32;
    private const int ErrorLockViolation = 33;

    public Task<FileMoveResult> MoveToDeletedFolderAsync(
        string sourcePath,
        string sessionFolderPath,
        CancellationToken cancellationToken = default)
    {
        var requestedDestinationPath = BuildDeletedDestinationPath(sourcePath, sessionFolderPath);
        return MoveAsync(sourcePath, requestedDestinationPath, "deleted", cancellationToken);
    }

    public Task<FileMoveResult> RestoreAsync(
        string deletedPath,
        string originalPath,
        CancellationToken cancellationToken = default)
    {
        return MoveAsync(deletedPath, originalPath, "restored", cancellationToken);
    }

    private static Task<FileMoveResult> MoveAsync(
        string sourcePath,
        string requestedDestinationPath,
        string collisionSuffix,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => MoveCore(sourcePath, requestedDestinationPath, collisionSuffix, cancellationToken),
            cancellationToken);
    }

    private static FileMoveResult MoveCore(
        string sourcePath,
        string requestedDestinationPath,
        string collisionSuffix,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(requestedDestinationPath))
        {
            return FileMoveResult.Failure(
                sourcePath,
                requestedDestinationPath,
                FileMoveErrorCode.InvalidPath,
                "Source and destination paths are required.");
        }

        try
        {
            var fullSourcePath = Path.GetFullPath(sourcePath);
            var fullRequestedDestinationPath = Path.GetFullPath(requestedDestinationPath);

            if (!File.Exists(fullSourcePath))
            {
                return FileMoveResult.Failure(
                    fullSourcePath,
                    fullRequestedDestinationPath,
                    FileMoveErrorCode.SourceMissing,
                    "Source file does not exist.");
            }

            var destinationDirectory = Path.GetDirectoryName(fullRequestedDestinationPath);
            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                return FileMoveResult.Failure(
                    fullSourcePath,
                    fullRequestedDestinationPath,
                    FileMoveErrorCode.InvalidPath,
                    "Destination directory could not be resolved.");
            }

            Directory.CreateDirectory(destinationDirectory);
            var actualDestinationPath = ResolveUniqueDestinationPath(fullRequestedDestinationPath, collisionSuffix);
            var collisionResolved = !PathsEqual(fullRequestedDestinationPath, actualDestinationPath);
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(fullSourcePath);

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(fullSourcePath, actualDestinationPath, overwrite: false);
            File.SetLastWriteTimeUtc(actualDestinationPath, lastWriteTimeUtc);

            return FileMoveResult.Success(
                fullSourcePath,
                fullRequestedDestinationPath,
                actualDestinationPath,
                collisionResolved,
                new DateTimeOffset(lastWriteTimeUtc, TimeSpan.Zero));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (MapErrorCode(exception) is { } errorCode)
        {
            return FileMoveResult.Failure(
                sourcePath,
                requestedDestinationPath,
                errorCode,
                exception.Message);
        }
    }

    private static string BuildDeletedDestinationPath(string sourcePath, string sessionFolderPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(sessionFolderPath))
        {
            return string.Empty;
        }

        var fileName = Path.GetFileName(sourcePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return Path.Combine(sessionFolderPath, FolderScanPolicy.DeletedFolderName, fileName);
    }

    private static string ResolveUniqueDestinationPath(string requestedDestinationPath, string collisionSuffix)
    {
        if (!File.Exists(requestedDestinationPath))
        {
            return requestedDestinationPath;
        }

        var directory = Path.GetDirectoryName(requestedDestinationPath)
            ?? throw new InvalidOperationException("Destination directory could not be resolved.");
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(requestedDestinationPath);
        var extension = Path.GetExtension(requestedDestinationPath);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);

        for (var counter = 1; counter <= 999; counter++)
        {
            var candidate = Path.Combine(
                directory,
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{fileNameWithoutExtension}__{collisionSuffix}_{timestamp}_{counter:000}{extension}"));
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new IOException("Could not resolve a unique destination path.");
    }

    private static FileMoveErrorCode MapErrorCode(Exception exception)
    {
        return exception switch
        {
            FileNotFoundException => FileMoveErrorCode.SourceMissing,
            IOException ioException when IsFileLocked(ioException) => FileMoveErrorCode.FileLocked,
            UnauthorizedAccessException => FileMoveErrorCode.AccessDenied,
            PathTooLongException => FileMoveErrorCode.PathTooLong,
            DirectoryNotFoundException => FileMoveErrorCode.InvalidPath,
            DriveNotFoundException => FileMoveErrorCode.InvalidPath,
            ArgumentException => FileMoveErrorCode.InvalidPath,
            NotSupportedException => FileMoveErrorCode.InvalidPath,
            IOException => FileMoveErrorCode.IoFailure,
            _ => FileMoveErrorCode.Unknown
        };
    }

    private static bool IsFileLocked(IOException exception)
    {
        var errorCode = exception.HResult & 0x0000FFFF;
        return errorCode is ErrorSharingViolation or ErrorLockViolation;
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
