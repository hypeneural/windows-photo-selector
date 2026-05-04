using Evydencia.PhotoSelector.Core.Navigation;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Deletion;

public sealed class DeleteManager
{
    public DeleteRequestResult RequestDeleteCurrent(PhotoSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var controller = new NavigationController(session);
        var deletedPhoto = controller.Current;
        if (deletedPhoto is null)
        {
            return DeleteRequestResult.NoCurrentPhoto(session);
        }

        var currentPhoto = controller.HideCurrent(PhotoStatus.PendingDelete);
        return DeleteRequestResult.PendingDelete(session, deletedPhoto, currentPhoto);
    }

    public DeleteCompletionResult CompleteDelete(PhotoSession session, PhotoItem deletedPhoto)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(deletedPhoto);

        EnsurePhotoBelongsToSession(session, deletedPhoto);
        EnsurePendingDelete(deletedPhoto);

        deletedPhoto.SetStatus(PhotoStatus.Deleted);
        return new DeleteCompletionResult(
            session,
            DeleteCompletionStatus.Deleted,
            deletedPhoto,
            ResolveCurrentPhoto(session));
    }

    public DeleteCompletionResult FailDelete(
        PhotoSession session,
        PhotoItem deletedPhoto,
        Guid? preferredCurrentPhotoId)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(deletedPhoto);

        EnsurePhotoBelongsToSession(session, deletedPhoto);
        EnsurePendingDelete(deletedPhoto);

        deletedPhoto.SetStatus(PhotoStatus.DeleteFailed);
        if (preferredCurrentPhotoId.HasValue)
        {
            SyncCurrentIndex(session, preferredCurrentPhotoId.Value);
        }

        return new DeleteCompletionResult(
            session,
            DeleteCompletionStatus.DeleteFailed,
            deletedPhoto,
            ResolveCurrentPhoto(session));
    }

    private static void EnsurePhotoBelongsToSession(PhotoSession session, PhotoItem photo)
    {
        if (!session.Photos.Any(item => item.Id == photo.Id))
        {
            throw new InvalidOperationException("Photo does not belong to the session.");
        }
    }

    private static void EnsurePendingDelete(PhotoItem photo)
    {
        if (photo.Status != PhotoStatus.PendingDelete)
        {
            throw new InvalidOperationException("Photo must be pending delete.");
        }
    }

    private static PhotoItem? ResolveCurrentPhoto(PhotoSession session)
    {
        var active = session.ActivePhotos();
        if (active.Count == 0)
        {
            session.CurrentIndex = 0;
            return null;
        }

        if (session.CurrentIndex >= active.Count)
        {
            session.CurrentIndex = active.Count - 1;
        }

        return active[session.CurrentIndex];
    }

    private static void SyncCurrentIndex(PhotoSession session, Guid preferredCurrentPhotoId)
    {
        var index = session.IndexOfActivePhoto(preferredCurrentPhotoId);
        if (index >= 0)
        {
            session.CurrentIndex = index;
        }
    }
}
