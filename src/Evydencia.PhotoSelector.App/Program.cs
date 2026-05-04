using Microsoft.UI.Dispatching;

namespace Evydencia.PhotoSelector.App;

public static class Program
{
    private static App? _app;

    [STAThread]
    public static int Main(string[] args)
    {
        _ = args;

        WinRT.ComWrappersSupport.InitializeComWrappers();

        if (Activation.AppInstanceCoordinator.RedirectActivationIfNeededAsync().GetAwaiter().GetResult())
        {
            return 0;
        }

        Microsoft.UI.Xaml.Application.Start(initializationCallbackParams =>
        {
            _ = initializationCallbackParams;
            var context = new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            _app = new App();
        });

        return 0;
    }
}
