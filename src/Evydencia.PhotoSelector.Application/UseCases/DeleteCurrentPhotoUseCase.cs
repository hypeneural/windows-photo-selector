using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class DeleteCurrentPhotoUseCase
{
    private readonly DeleteManager _deleteManager;
    private readonly IFileMoveService _fileMoveService;
    private readonly ISessionJournalStore _journalStore;
    private readonly UndoManager _undoManager;

    public DeleteCurrentPhotoUseCase(
        DeleteManager deleteManager,
        IFileMoveService fileMoveService,
        UndoManager undoManager,
        ISessionJournalStore journalStore)
    {
        _deleteManager = deleteManager;
        _fileMoveService = fileMoveService;
        _undoManager = undoManager;
        _journalStore = journalStore;
    }

    public async Task<DeleteCurrentPhotoResult> ExecuteAsync(
        PhotoSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        var request = _deleteManager.RequestDeleteCurrent(session);
        if (request.Status == DeleteRequestStatus.NoCurrentPhoto || request.DeletedPhoto is null)
        {
            return DeleteCurrentPhotoResult.NoCurrentPhoto(session);
        }

        var preferredCurrentPhotoId = request.CurrentPhoto?.Id;
        try
        {
            await _journalStore
                .AppendAsync(
                    session,
                    SessionJournalEvent.DeleteRequested(session, request.DeletedPhoto),
                    cancellationToken)
                .ConfigureAwait(false);

            var moveResult = await _fileMoveService
                .MoveToDeletedFolderAsync(request.DeletedPhoto.FullPath, session.FolderPath, cancellationToken)
                .ConfigureAwait(false);

            return moveResult.IsSuccess
                ? await CompleteDeleteAsync(session, request.DeletedPhoto, moveResult, cancellationToken)
                    .ConfigureAwait(false)
                : await FailDeleteAsync(
                        session,
                        request.DeletedPhoto,
                        preferredCurrentPhotoId,
                        moveResult,
                        cancellationToken)
                    .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            RestoreAfterCancellation(session, request.DeletedPhoto, preferredCurrentPhotoId);
            throw;
        }
        catch
        {
            RestoreAfterCancellation(session, request.DeletedPhoto, preferredCurrentPhotoId);
            throw;
        }
    }

    private async Task<DeleteCurrentPhotoResult> CompleteDeleteAsync(
        PhotoSession session,
        PhotoItem deletedPhoto,
        FileMoveResult moveResult,
        CancellationToken cancellationToken)
    {
        var completion = _deleteManager.CompleteDelete(session, deletedPhoto);
        _undoManager.RegisterDeletedPhoto(
            session,
            completion.DeletedPhoto,
            moveResult.ActualDestinationPath!);
        await _journalStore
            .AppendAsync(
                session,
                SessionJournalEvent.Deleted(session, completion.DeletedPhoto, moveResult),
                cancellationToken)
            .ConfigureAwait(false);
        return DeleteCurrentPhotoResult.Deleted(
            session,
            completion.DeletedPhoto,
            completion.CurrentPhoto,
            moveResult);
    }

    private async Task<DeleteCurrentPhotoResult> FailDeleteAsync(
        PhotoSession session,
        PhotoItem deletedPhoto,
        Guid? preferredCurrentPhotoId,
        FileMoveResult moveResult,
        CancellationToken cancellationToken)
    {
        var completion = _deleteManager.FailDelete(session, deletedPhoto, preferredCurrentPhotoId);
        await _journalStore
            .AppendAsync(
                session,
                SessionJournalEvent.DeleteFailed(session, completion.DeletedPhoto, moveResult),
                cancellationToken)
            .ConfigureAwait(false);
        return DeleteCurrentPhotoResult.DeleteFailed(
            session,
            completion.DeletedPhoto,
            completion.CurrentPhoto,
            moveResult);
    }

    private void RestoreAfterCancellation(
        PhotoSession session,
        PhotoItem deletedPhoto,
        Guid? preferredCurrentPhotoId)
    {
        if (deletedPhoto.Status == PhotoStatus.PendingDelete)
        {
            _deleteManager.FailDelete(session, deletedPhoto, preferredCurrentPhotoId);
        }
    }
}
