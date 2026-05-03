using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Imaging.Decode;
using Evydencia.PhotoSelector.Imaging.Sizing;
using Evydencia.PhotoSelector.Storage.Filesystem;
using Microsoft.Extensions.DependencyInjection;

namespace Evydencia.PhotoSelector.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvydenciaPhotoSelector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<PhotoSessionFactory>();
        services.AddSingleton<DecodeTargetCalculator>();
        services.AddSingleton<JpegDecodeService>();
        services.AddSingleton<FolderLaunchArgumentsParser>();

        services.AddTransient<IFolderScanner, FileSystemFolderScanner>();
        services.AddTransient<OpenFolderFromArgumentsUseCase>();
        services.AddTransient<OpenSessionUseCase>();
        services.AddTransient<NavigateNextPhotoUseCase>();
        services.AddTransient<NavigatePreviousPhotoUseCase>();

        return services;
    }
}
