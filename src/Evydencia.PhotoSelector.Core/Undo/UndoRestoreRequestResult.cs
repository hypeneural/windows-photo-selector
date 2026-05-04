using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Undo;

public sealed class UndoRestoreRequestResult
{
    private UndoRestoreRequestResult(
        PhotoSession session,
        UndoRestoreRequestStatus status,
        UndoDeleteOperation? operation,
        PhotoItem? restoredPhoto,
        PhotoItem? preferredCurrentPhoto)
    {
        Session = session;
        Status = status;
        Operation = operation;
        RestoredPhoto = restoredPhoto;
        PreferredCurrentPhoto = preferredCurrentPhoto;
    }

    public PhotoSession Session { get; }

    public UndoRestoreRequestStatus Status { get; }

    public UndoDeleteOperation? Operation { get; }

    public PhotoItem? RestoredPhoto { get; }

    public PhotoItem? PreferredCurrentPhoto { get; }

    public int CurrentIndex => Session.CurrentIndex;

    public int ActiveCount => Session.ActiveCount;

    public int DeletedCount => Session.DeletedCount;

    public static UndoRestoreRequestResult NoUndoAvailable(PhotoSession session)
    {
        return new UndoRestoreRequestResult(
            session,
            UndoRestoreRequestStatus.NoUndoAvailable,
            operation: null,
            restoredPhoto: null,
            preferredCurrentPhoto: null);
    }

    public static UndoRestoreRequestResult PendingRestore(
        PhotoSession session,
        UndoDeleteOperation operation,
        PhotoItem restoredPhoto,
        PhotoItem? preferredCurrentPhoto)
    {
        return new UndoRestoreRequestResult(
            session,
            UndoRestoreRequestStatus.PendingRestore,
            operation,
            restoredPhoto,
            preferredCurrentPhoto);
    }
}
