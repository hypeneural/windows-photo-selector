using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class DeleteCurrentPhotoUseCase
{
    private readonly DeleteManager _deleteManager;
    private readonly IFileMoveService _fileMoveService;

    public DeleteCurrentPhotoUseCase(
        DeleteManager deleteManager,
        IFileMoveService fileMoveService)
    {
        _deleteManager = deleteManager;
        _fileMoveService = fileMoveService;
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
            var moveResult = await _fileMoveService
                .MoveToDeletedFolderAsync(request.DeletedPhoto.FullPath, session.FolderPath, cancellationToken)
                .ConfigureAwait(false);

            return moveResult.IsSuccess
                ? CompleteDelete(session, request.DeletedPhoto, moveResult)
                : FailDelete(session, request.DeletedPhoto, preferredCurrentPhotoId, moveResult);
        }
        catch (OperationCanceledException)
        {
            RestoreAfterCancellation(session, request.DeletedPhoto, preferredCurrentPhotoId);
            throw;
        }
    }

    private DeleteCurrentPhotoResult CompleteDelete(
        PhotoSession session,
        PhotoItem deletedPhoto,
        FileMoveResult moveResult)
    {
        var completion = _deleteManager.CompleteDelete(session, deletedPhoto);
        return DeleteCurrentPhotoResult.Deleted(
            session,
            completion.DeletedPhoto,
            completion.CurrentPhoto,
            moveResult);
    }

    private DeleteCurrentPhotoResult FailDelete(
        PhotoSession session,
        PhotoItem deletedPhoto,
        Guid? preferredCurrentPhotoId,
        FileMoveResult moveResult)
    {
        var completion = _deleteManager.FailDelete(session, deletedPhoto, preferredCurrentPhotoId);
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
