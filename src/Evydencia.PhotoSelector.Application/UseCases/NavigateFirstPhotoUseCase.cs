using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Navigation;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class NavigateFirstPhotoUseCase
{
    public NavigationResult Execute(PhotoSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var controller = new NavigationController(session);
        var currentPhoto = controller.MoveFirst();
        return new NavigationResult(session, currentPhoto);
    }
}
