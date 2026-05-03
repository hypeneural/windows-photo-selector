using Evydencia.PhotoSelector.App.Display;
using Evydencia.PhotoSelector.App.Imaging;
using Evydencia.PhotoSelector.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Evydencia.PhotoSelector.App.Composition;

public static class AppCompositionRoot
{
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddEvydenciaPhotoSelector();
        services.AddSingleton<WindowsDisplayContextService>();
        services.AddSingleton<ViewerImageSourceFactory>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
