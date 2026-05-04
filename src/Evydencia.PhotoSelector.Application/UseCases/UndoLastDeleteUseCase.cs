using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class UndoLastDeleteUseCase
{
    private readonly IFileMoveService _fileMoveService;
    private readonly UndoManager _undoManager;

    public UndoLastDeleteUseCase(
        UndoManager undoManager,
        IFileMoveService fileMoveService)
    {
        _undoManager = undoManager;
        _fileMoveService = fileMoveService;
    }

    public async Task<UndoLastDeleteResult> ExecuteAsync(
        PhotoSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        var request = _undoManager.RequestRestoreLast(session);
        if (request.Status == UndoRestoreRequestStatus.NoUndoAvailable
            || request.Operation is null
            || request.RestoredPhoto is null)
        {
            return UndoLastDeleteResult.NoUndoAvailable(session);
        }

        var preferredCurrentPhotoId = request.PreferredCurrentPhoto?.Id;
        try
        {
            var moveResult = await _fileMoveService
                .RestoreAsync(request.Operation.DeletedPath, request.Operation.OriginalPath, cancellationToken)
                .ConfigureAwait(false);

            return moveResult.IsSuccess
                ? CompleteRestore(session, request, moveResult)
                : FailRestore(session, request, preferredCurrentPhotoId, moveResult);
        }
        catch (OperationCanceledException)
        {
            RestoreAfterCancellation(session, request, preferredCurrentPhotoId);
            throw;
        }
    }

    private UndoLastDeleteResult CompleteRestore(
        PhotoSession session,
        UndoRestoreRequestResult request,
        FileMoveResult moveResult)
    {
        if (!string.IsNullOrWhiteSpace(moveResult.ActualDestinationPath))
        {
            ApplyRestoredLocation(request.RestoredPhoto!, moveResult.ActualDestinationPath);
        }

        var completion = _undoManager.CompleteRestore(session, request.Operation!, request.RestoredPhoto!);
        return UndoLastDeleteResult.Restored(
            session,
            completion.RestoredPhoto,
            completion.CurrentPhoto,
            moveResult);
    }

    private UndoLastDeleteResult FailRestore(
        PhotoSession session,
        UndoRestoreRequestResult request,
        Guid? preferredCurrentPhotoId,
        FileMoveResult moveResult)
    {
        var completion = _undoManager.FailRestore(
            session,
            request.Operation!,
            request.RestoredPhoto!,
            preferredCurrentPhotoId);
        return UndoLastDeleteResult.RestoreFailed(
            session,
            completion.RestoredPhoto,
            completion.CurrentPhoto,
            moveResult);
    }

    private void RestoreAfterCancellation(
        PhotoSession session,
        UndoRestoreRequestResult request,
        Guid? preferredCurrentPhotoId)
    {
        if (request.RestoredPhoto?.Status == PhotoStatus.PendingRestore)
        {
            _undoManager.FailRestore(
                session,
                request.Operation!,
                request.RestoredPhoto,
                preferredCurrentPhotoId);
        }
    }

    private static void ApplyRestoredLocation(PhotoItem photo, string actualDestinationPath)
    {
        var fileName = Path.GetFileName(actualDestinationPath);
        var directory = Path.GetDirectoryName(actualDestinationPath);
        var extension = Path.GetExtension(actualDestinationPath);

        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        photo.SetFileLocation(fileName, actualDestinationPath, directory, extension);
    }
}
