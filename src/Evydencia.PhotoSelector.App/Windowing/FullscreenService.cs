using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Evydencia.PhotoSelector.App.Windowing;

internal sealed class FullscreenService
{
    public bool IsFullscreen(Window? window)
    {
        return window?.AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;
    }

    public bool ToggleFullscreen(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (IsFullscreen(window))
        {
            ExitFullscreen(window);
            return false;
        }

        EnterFullscreen(window);
        return true;
    }

    public void EnterFullscreen(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (IsFullscreen(window))
        {
            return;
        }

        window.AppWindow.SetPresenter(FullScreenPresenter.Create());
    }

    public void ExitFullscreen(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!IsFullscreen(window))
        {
            return;
        }

        window.AppWindow.SetPresenter(AppWindowPresenterKind.Default);
    }
}
