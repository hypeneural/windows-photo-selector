using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Models;

public sealed class DeleteCurrentPhotoResult
{
    private DeleteCurrentPhotoResult(
        PhotoSession session,
        DeleteCurrentPhotoStatus status,
        PhotoItem? deletedPhoto,
        PhotoItem? currentPhoto,
        FileMoveResult? fileMoveResult)
    {
        Session = session;
        Status = status;
        DeletedPhoto = deletedPhoto;
        CurrentPhoto = currentPhoto;
        FileMoveResult = fileMoveResult;
    }

    public PhotoSession Session { get; }

    public DeleteCurrentPhotoStatus Status { get; }

    public PhotoItem? DeletedPhoto { get; }

    public PhotoItem? CurrentPhoto { get; }

    public FileMoveResult? FileMoveResult { get; }

    public int CurrentIndex => Session.CurrentIndex;

    public int ActiveCount => Session.ActiveCount;

    public int DeletedCount => Session.DeletedCount;

    public static DeleteCurrentPhotoResult NoCurrentPhoto(PhotoSession session)
    {
        return new DeleteCurrentPhotoResult(
            session,
            DeleteCurrentPhotoStatus.NoCurrentPhoto,
            deletedPhoto: null,
            currentPhoto: null,
            fileMoveResult: null);
    }

    public static DeleteCurrentPhotoResult Deleted(
        PhotoSession session,
        PhotoItem deletedPhoto,
        PhotoItem? currentPhoto,
        FileMoveResult fileMoveResult)
    {
        return new DeleteCurrentPhotoResult(
            session,
            DeleteCurrentPhotoStatus.Deleted,
            deletedPhoto,
            currentPhoto,
            fileMoveResult);
    }

    public static DeleteCurrentPhotoResult DeleteFailed(
        PhotoSession session,
        PhotoItem deletedPhoto,
        PhotoItem? currentPhoto,
        FileMoveResult fileMoveResult)
    {
        return new DeleteCurrentPhotoResult(
            session,
            DeleteCurrentPhotoStatus.DeleteFailed,
            deletedPhoto,
            currentPhoto,
            fileMoveResult);
    }
}
