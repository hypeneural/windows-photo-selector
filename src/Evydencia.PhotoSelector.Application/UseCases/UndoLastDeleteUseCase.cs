using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class UndoLastDeleteUseCase
{
    private readonly IFileMoveService _fileMoveService;
    private readonly ISessionJournalStore _journalStore;
    private readonly UndoManager _undoManager;

    public UndoLastDeleteUseCase(
        UndoManager undoManager,
        IFileMoveService fileMoveService,
        ISessionJournalStore journalStore)
    {
        _undoManager = undoManager;
        _fileMoveService = fileMoveService;
        _journalStore = journalStore;
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
            await _journalStore
                .AppendAsync(
                    session,
                    SessionJournalEvent.UndoRequested(
                        session,
                        request.RestoredPhoto,
                        request.Operation.DeletedPath,
                        request.Operation.OriginalPath),
                    cancellationToken)
                .ConfigureAwait(false);

            var moveResult = await _fileMoveService
                .RestoreAsync(request.Operation.DeletedPath, request.Operation.OriginalPath, cancellationToken)
                .ConfigureAwait(false);

            return moveResult.IsSuccess
                ? await CompleteRestoreAsync(session, request, moveResult, cancellationToken)
                    .ConfigureAwait(false)
                : await FailRestoreAsync(
                        session,
                        request,
                        preferredCurrentPhotoId,
                        moveResult,
                        cancellationToken)
                    .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            RestoreAfterCancellation(session, request, preferredCurrentPhotoId);
            throw;
        }
        catch
        {
            RestoreAfterCancellation(session, request, preferredCurrentPhotoId);
            throw;
        }
    }

    private async Task<UndoLastDeleteResult> CompleteRestoreAsync(
        PhotoSession session,
        UndoRestoreRequestResult request,
        FileMoveResult moveResult,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(moveResult.ActualDestinationPath))
        {
            ApplyRestoredLocation(request.RestoredPhoto!, moveResult.ActualDestinationPath);
        }

        var completion = _undoManager.CompleteRestore(session, request.Operation!, request.RestoredPhoto!);
        await _journalStore
            .AppendAsync(
                session,
                SessionJournalEvent.Restored(session, completion.RestoredPhoto, moveResult),
                cancellationToken)
            .ConfigureAwait(false);
        return UndoLastDeleteResult.Restored(
            session,
            completion.RestoredPhoto,
            completion.CurrentPhoto,
            moveResult);
    }

    private async Task<UndoLastDeleteResult> FailRestoreAsync(
        PhotoSession session,
        UndoRestoreRequestResult request,
        Guid? preferredCurrentPhotoId,
        FileMoveResult moveResult,
        CancellationToken cancellationToken)
    {
        var completion = _undoManager.FailRestore(
            session,
            request.Operation!,
            request.RestoredPhoto!,
            preferredCurrentPhotoId);
        await _journalStore
            .AppendAsync(
                session,
                SessionJournalEvent.RestoreFailed(session, completion.RestoredPhoto, moveResult),
                cancellationToken)
            .ConfigureAwait(false);
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
