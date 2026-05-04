using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Imaging.Sizing;
using Evydencia.PhotoSelector.Infrastructure.DependencyInjection;
using Evydencia.PhotoSelector.Storage.Filesystem;

namespace Evydencia.PhotoSelector.IntegrationTests.Composition;

[TestClass]
public sealed class DependencyInjectionTests
{
    [TestMethod]
    public void CreateRegistersCurrentViewerServices()
    {
        var provider = EvydenciaServiceProviderFactory.Create();

        Assert.IsInstanceOfType<OpenSessionUseCase>(provider.GetService(typeof(OpenSessionUseCase)));
        Assert.IsInstanceOfType<NavigateNextPhotoUseCase>(provider.GetService(typeof(NavigateNextPhotoUseCase)));
        Assert.IsInstanceOfType<NavigatePreviousPhotoUseCase>(provider.GetService(typeof(NavigatePreviousPhotoUseCase)));
        Assert.IsInstanceOfType<DeleteCurrentPhotoUseCase>(provider.GetService(typeof(DeleteCurrentPhotoUseCase)));
        Assert.IsInstanceOfType<UndoLastDeleteUseCase>(provider.GetService(typeof(UndoLastDeleteUseCase)));
        Assert.IsInstanceOfType<FileSystemFolderScanner>(provider.GetService(typeof(IFolderScanner)));
        Assert.IsInstanceOfType<FileMoveService>(provider.GetService(typeof(IFileMoveService)));
        Assert.IsInstanceOfType<DecodeTargetCalculator>(provider.GetService(typeof(DecodeTargetCalculator)));
        Assert.IsInstanceOfType<FolderLaunchArgumentsParser>(provider.GetService(typeof(FolderLaunchArgumentsParser)));
        Assert.IsInstanceOfType<OpenFolderFromArgumentsUseCase>(provider.GetService(typeof(OpenFolderFromArgumentsUseCase)));
    }
}
