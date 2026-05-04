using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;

namespace Evydencia.PhotoSelector.Application.Tests.Fakes;

internal sealed class FakeFileMoveService : IFileMoveService
{
    private readonly FileMoveResult _moveResult;
    private readonly FileMoveResult? _restoreResult;
    private readonly bool _cancelMove;
    private readonly bool _cancelRestore;

    public FakeFileMoveService(
        FileMoveResult moveResult,
        FileMoveResult? restoreResult = null,
        bool cancelMove = false,
        bool cancelRestore = false)
    {
        _moveResult = moveResult;
        _restoreResult = restoreResult;
        _cancelMove = cancelMove;
        _cancelRestore = cancelRestore;
    }

    public string? LastSourcePath { get; private set; }

    public string? LastSessionFolderPath { get; private set; }

    public string? LastDeletedPath { get; private set; }

    public string? LastOriginalPath { get; private set; }

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
        LastDeletedPath = deletedPath;
        LastOriginalPath = originalPath;

        if (_cancelRestore)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return Task.FromResult(_restoreResult ?? _moveResult);
    }
}
