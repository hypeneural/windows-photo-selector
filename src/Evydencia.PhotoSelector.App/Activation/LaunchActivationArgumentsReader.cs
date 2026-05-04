using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace Evydencia.PhotoSelector.App.Activation;

internal static class LaunchActivationArgumentsReader
{
    public static string ReadRawArguments(AppActivationArguments? activationArguments)
    {
        if (activationArguments?.Kind != ExtendedActivationKind.Launch)
        {
            return string.Empty;
        }

        return activationArguments.Data is ILaunchActivatedEventArgs launchArguments
            ? launchArguments.Arguments
            : string.Empty;
    }
}
