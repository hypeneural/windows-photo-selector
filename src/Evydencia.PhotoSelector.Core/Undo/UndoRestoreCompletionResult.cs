using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Undo;

public sealed class UndoRestoreCompletionResult
{
    public UndoRestoreCompletionResult(
        PhotoSession session,
        UndoRestoreCompletionStatus status,
        UndoDeleteOperation operation,
        PhotoItem restoredPhoto,
        PhotoItem? currentPhoto)
    {
        Session = session;
        Status = status;
        Operation = operation;
        RestoredPhoto = restoredPhoto;
        CurrentPhoto = currentPhoto;
    }

    public PhotoSession Session { get; }

    public UndoRestoreCompletionStatus Status { get; }

    public UndoDeleteOperation Operation { get; }

    public PhotoItem RestoredPhoto { get; }

    public PhotoItem? CurrentPhoto { get; }

    public int CurrentIndex => Session.CurrentIndex;

    public int ActiveCount => Session.ActiveCount;

    public int DeletedCount => Session.DeletedCount;
}
