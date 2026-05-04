using Evydencia.PhotoSelector.Application.Models;

namespace Evydencia.PhotoSelector.Application.Abstractions;

public interface IFileMoveService
{
    Task<FileMoveResult> MoveToDeletedFolderAsync(
        string sourcePath,
        string sessionFolderPath,
        CancellationToken cancellationToken = default);

    Task<FileMoveResult> RestoreAsync(
        string deletedPath,
        string originalPath,
        CancellationToken cancellationToken = default);
}
