using Microsoft.Extensions.DependencyInjection;

namespace Evydencia.PhotoSelector.Infrastructure.DependencyInjection;

public static class EvydenciaServiceProviderFactory
{
    public static IServiceProvider Create()
    {
        var services = new ServiceCollection();
        services.AddEvydenciaPhotoSelector();
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
