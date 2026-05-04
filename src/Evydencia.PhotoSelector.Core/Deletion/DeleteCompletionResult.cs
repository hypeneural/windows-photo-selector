using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Deletion;

public sealed class DeleteCompletionResult
{
    public DeleteCompletionResult(
        PhotoSession session,
        DeleteCompletionStatus status,
        PhotoItem deletedPhoto,
        PhotoItem? currentPhoto)
    {
        Session = session;
        Status = status;
        DeletedPhoto = deletedPhoto;
        CurrentPhoto = currentPhoto;
    }

    public PhotoSession Session { get; }

    public DeleteCompletionStatus Status { get; }

    public PhotoItem DeletedPhoto { get; }

    public PhotoItem? CurrentPhoto { get; }

    public int CurrentIndex => Session.CurrentIndex;

    public int ActiveCount => Session.ActiveCount;

    public int DeletedCount => Session.DeletedCount;
}
