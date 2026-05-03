using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Models;

public sealed class OpenSessionResult
{
    public OpenSessionResult(PhotoSession session, PhotoItem? currentPhoto)
    {
        Session = session;
        CurrentPhoto = currentPhoto;
    }

    public PhotoSession Session { get; }

    public PhotoItem? CurrentPhoto { get; }
}
