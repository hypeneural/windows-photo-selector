using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Models;

public sealed class UndoLastDeleteResult
{
    private UndoLastDeleteResult(
        PhotoSession session,
        UndoLastDeleteStatus status,
        PhotoItem? restoredPhoto,
        PhotoItem? currentPhoto,
        FileMoveResult? fileMoveResult)
    {
        Session = session;
        Status = status;
        RestoredPhoto = restoredPhoto;
        CurrentPhoto = currentPhoto;
        FileMoveResult = fileMoveResult;
    }

    public PhotoSession Session { get; }

    public UndoLastDeleteStatus Status { get; }

    public PhotoItem? RestoredPhoto { get; }

    public PhotoItem? CurrentPhoto { get; }

    public FileMoveResult? FileMoveResult { get; }

    public int CurrentIndex => Session.CurrentIndex;

    public int ActiveCount => Session.ActiveCount;

    public int DeletedCount => Session.DeletedCount;

    public static UndoLastDeleteResult NoUndoAvailable(PhotoSession session)
    {
        return new UndoLastDeleteResult(
            session,
            UndoLastDeleteStatus.NoUndoAvailable,
            restoredPhoto: null,
            currentPhoto: null,
            fileMoveResult: null);
    }

    public static UndoLastDeleteResult Restored(
        PhotoSession session,
        PhotoItem restoredPhoto,
        PhotoItem? currentPhoto,
        FileMoveResult fileMoveResult)
    {
        return new UndoLastDeleteResult(
            session,
            UndoLastDeleteStatus.Restored,
            restoredPhoto,
            currentPhoto,
            fileMoveResult);
    }

    public static UndoLastDeleteResult RestoreFailed(
        PhotoSession session,
        PhotoItem restoredPhoto,
        PhotoItem? currentPhoto,
        FileMoveResult fileMoveResult)
    {
        return new UndoLastDeleteResult(
            session,
            UndoLastDeleteStatus.RestoreFailed,
            restoredPhoto,
            currentPhoto,
            fileMoveResult);
    }
}
