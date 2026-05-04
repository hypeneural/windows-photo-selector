using Microsoft.Windows.AppLifecycle;

namespace Evydencia.PhotoSelector.App.Activation;

internal static class AppInstanceCoordinator
{
    private const string MainInstanceKey = "Evydencia.PhotoSelector.Main";
    private static AppInstance? _mainInstance;

    public static AppInstance MainInstance => _mainInstance ??= AppInstance.FindOrRegisterForKey(MainInstanceKey);

    public static async Task<bool> RedirectActivationIfNeededAsync()
    {
        var mainInstance = AppInstance.FindOrRegisterForKey(MainInstanceKey);
        if (mainInstance.IsCurrent)
        {
            _mainInstance = mainInstance;
            return false;
        }

        var activationArguments = AppInstance.GetCurrent().GetActivatedEventArgs();
        await mainInstance.RedirectActivationToAsync(activationArguments);
        return true;
    }
}
