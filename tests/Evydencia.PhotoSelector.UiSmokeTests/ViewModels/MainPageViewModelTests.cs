using Evydencia.PhotoSelector.App.ViewModels;
using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.UiSmokeTests.ViewModels;

[TestClass]
public sealed class MainPageViewModelTests
{
    [TestMethod]
    public async Task LoadInitialSessionAsyncWithNullTaskShowsNoFolderState()
    {
        var viewModel = new MainPageViewModel();

        await viewModel.LoadInitialSessionAsync(null);

        Assert.AreEqual("Nenhuma pasta carregada", viewModel.StatusText);
        Assert.AreEqual("Aguardando pasta", viewModel.DetailText);
        Assert.AreEqual("0 JPEGs", viewModel.PhotoCountText);
        Assert.IsFalse(viewModel.HasSession);
        Assert.IsFalse(viewModel.IsLoading);
    }

    [TestMethod]
    public async Task LoadInitialSessionAsyncWithOpenedSessionShowsSessionSummary()
    {
        var viewModel = new MainPageViewModel();
        var result = OpenFolderFromArgumentsResult.Opened(
            new FolderLaunchArguments("C:\\Sessao Cliente", "launcher"),
            CreateSessionResult("IMG_0001.jpg", "IMG_0002.jpg"));

        await viewModel.LoadInitialSessionAsync(Task.FromResult(result));

        Assert.AreEqual("Sessao carregada", viewModel.StatusText);
        Assert.AreEqual("Sessao Cliente", viewModel.DetailText);
        Assert.AreEqual("2 JPEGs", viewModel.PhotoCountText);
        Assert.IsTrue(viewModel.HasSession);
        Assert.IsFalse(viewModel.IsLoading);
    }

    [TestMethod]
    public async Task LoadInitialSessionAsyncWithFailureShowsFailureState()
    {
        var viewModel = new MainPageViewModel();
        var result = OpenFolderFromArgumentsResult.Failed(
            new FolderLaunchArguments("C:\\NaoExiste", "launcher"),
            new DirectoryNotFoundException("folder not found"));

        await viewModel.LoadInitialSessionAsync(Task.FromResult(result));

        Assert.AreEqual("Falha ao abrir pasta", viewModel.StatusText);
        Assert.AreEqual("folder not found", viewModel.DetailText);
        Assert.AreEqual("0 JPEGs", viewModel.PhotoCountText);
        Assert.IsFalse(viewModel.HasSession);
        Assert.IsFalse(viewModel.IsLoading);
    }

    private static OpenSessionResult CreateSessionResult(params string[] fileNames)
    {
        var candidates = fileNames.Select((fileName, index) => new PhotoFileCandidate(
            fileName,
            $"C:\\Sessao Cliente\\{fileName}",
            "C:\\Sessao Cliente",
            fileName[fileName.LastIndexOf('.')..],
            1024,
            DateTimeOffset.UtcNow,
            index));
        var session = new PhotoSessionFactory().Create("C:\\Sessao Cliente", candidates);
        return new OpenSessionResult(session, session.Photos[0]);
    }
}
