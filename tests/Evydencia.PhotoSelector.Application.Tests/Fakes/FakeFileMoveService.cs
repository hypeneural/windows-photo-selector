using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;

namespace Evydencia.PhotoSelector.Application.Tests.Fakes;

internal sealed class FakeFileMoveService : IFileMoveService
{
    private readonly FileMoveResult _moveResult;
    private readonly bool _cancelMove;

    public FakeFileMoveService(FileMoveResult moveResult, bool cancelMove = false)
    {
        _moveResult = moveResult;
        _cancelMove = cancelMove;
    }

    public string? LastSourcePath { get; private set; }

    public string? LastSessionFolderPath { get; private set; }

    public Task<FileMoveResult> MoveToDeletedFolderAsync(
        string sourcePath,
        string sessionFolderPath,
        CancellationToken cancellationToken = default)
    {
        LastSourcePath = sourcePath;
        LastSessionFolderPath = sessionFolderPath;

        if (_cancelMove)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return Task.FromResult(_moveResult);
    }

    public Task<FileMoveResult> RestoreAsync(
        string deletedPath,
        string originalPath,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Restore is not used by DeleteCurrentPhotoUseCase tests.");
    }
}
