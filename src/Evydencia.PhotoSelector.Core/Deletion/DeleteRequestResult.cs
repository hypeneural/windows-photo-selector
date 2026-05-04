using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Deletion;

public sealed class DeleteRequestResult
{
    private DeleteRequestResult(
        PhotoSession session,
        DeleteRequestStatus status,
        PhotoItem? deletedPhoto,
        PhotoItem? currentPhoto)
    {
        Session = session;
        Status = status;
        DeletedPhoto = deletedPhoto;
        CurrentPhoto = currentPhoto;
    }

    public PhotoSession Session { get; }

    public DeleteRequestStatus Status { get; }

    public PhotoItem? DeletedPhoto { get; }

    public PhotoItem? CurrentPhoto { get; }

    public int CurrentIndex => Session.CurrentIndex;

    public int ActiveCount => Session.ActiveCount;

    public int DeletedCount => Session.DeletedCount;

    public static DeleteRequestResult NoCurrentPhoto(PhotoSession session)
    {
        return new DeleteRequestResult(session, DeleteRequestStatus.NoCurrentPhoto, deletedPhoto: null, currentPhoto: null);
    }

    public static DeleteRequestResult PendingDelete(PhotoSession session, PhotoItem deletedPhoto, PhotoItem? currentPhoto)
    {
        return new DeleteRequestResult(session, DeleteRequestStatus.PendingDelete, deletedPhoto, currentPhoto);
    }
}
