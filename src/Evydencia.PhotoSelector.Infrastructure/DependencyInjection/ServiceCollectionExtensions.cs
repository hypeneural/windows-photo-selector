using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;
using Evydencia.PhotoSelector.Imaging.Cache;
using Evydencia.PhotoSelector.Imaging.Decode;
using Evydencia.PhotoSelector.Imaging.Prefetch;
using Evydencia.PhotoSelector.Imaging.Sizing;
using Evydencia.PhotoSelector.Storage.Filesystem;
using Evydencia.PhotoSelector.Storage.Journal;
using Microsoft.Extensions.DependencyInjection;

namespace Evydencia.PhotoSelector.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvydenciaPhotoSelector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<PhotoSessionFactory>();
        services.AddSingleton<DeleteManager>();
        services.AddSingleton<UndoManager>();
        services.AddSingleton<DecodeTargetCalculator>();
        services.AddSingleton<JpegDecodeService>();
        services.AddSingleton<MemoryImageCache>();
        services.AddSingleton<IPreviewCacheService, PreviewCacheService>();
        services.AddSingleton<PrefetchScheduler>();
        services.AddSingleton<FolderLaunchArgumentsParser>();

        services.AddTransient<IFolderScanner, FileSystemFolderScanner>();
        services.AddTransient<IFileMoveService, FileMoveService>();
        services.AddSingleton<IFileExistenceService, FileSystemFileExistenceService>();
        services.AddSingleton<ISessionJournalStore, JsonlSessionJournalStore>();
        services.AddTransient<OpenFolderFromArgumentsUseCase>();
        services.AddTransient<OpenSessionUseCase>();
        services.AddTransient<NavigateFirstPhotoUseCase>();
        services.AddTransient<NavigateNextPhotoUseCase>();
        services.AddTransient<NavigatePreviousPhotoUseCase>();
        services.AddTransient<NavigateLastPhotoUseCase>();
        services.AddTransient<DeleteCurrentPhotoUseCase>();
        services.AddTransient<UndoLastDeleteUseCase>();
        services.AddTransient<ReplaySessionJournalUseCase>();

        return services;
    }
}
