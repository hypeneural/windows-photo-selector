using Evydencia.PhotoSelector.App.ViewModels;
using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
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
        Assert.IsTrue(viewModel.IsHomeVisible);
        Assert.IsFalse(viewModel.IsViewerVisible);
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
        Assert.AreEqual("IMG_0001.jpg", viewModel.CurrentPhoto?.FileName);
        Assert.AreEqual("IMG_0001.jpg", viewModel.CurrentFileName);
        Assert.AreEqual("1 / 2", viewModel.ViewerCounterText);
        Assert.AreEqual("Aguardando imagem", viewModel.ViewerStatusText);
        Assert.IsFalse(viewModel.IsHomeVisible);
        Assert.IsTrue(viewModel.IsViewerVisible);
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
        Assert.IsNull(viewModel.CurrentPhoto);
        Assert.IsTrue(viewModel.IsHomeVisible);
        Assert.IsFalse(viewModel.IsViewerVisible);
    }

    [TestMethod]
    public async Task ApplyNavigationUpdatesCurrentPhotoAndCounter()
    {
        var viewModel = new MainPageViewModel();
        var sessionResult = CreateSessionResult("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        var result = OpenFolderFromArgumentsResult.Opened(
            new FolderLaunchArguments("C:\\Sessao Cliente", "launcher"),
            sessionResult);
        await viewModel.LoadInitialSessionAsync(Task.FromResult(result));

        var navigationResult = new NavigateNextPhotoUseCase().Execute(sessionResult.Session);

        viewModel.ApplyNavigation(navigationResult);

        Assert.AreEqual("IMG_0002.jpg", viewModel.CurrentPhoto?.FileName);
        Assert.AreEqual("IMG_0002.jpg", viewModel.CurrentFileName);
        Assert.AreEqual("2 / 3", viewModel.ViewerCounterText);
        Assert.AreEqual("Aguardando imagem", viewModel.ViewerStatusText);
        Assert.IsFalse(viewModel.HasCurrentImage);
    }

    [TestMethod]
    public void SetFullscreenUpdatesFullscreenState()
    {
        var viewModel = new MainPageViewModel();

        viewModel.SetFullscreen(isFullscreen: true);

        Assert.IsTrue(viewModel.IsFullscreen);

        viewModel.SetFullscreen(isFullscreen: false);

        Assert.IsFalse(viewModel.IsFullscreen);
    }

    [TestMethod]
    public async Task ApplyDeletePendingUpdatesCurrentPhotoAndCounter()
    {
        var viewModel = new MainPageViewModel();
        var sessionResult = CreateSessionResult("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        await LoadOpenedSessionAsync(viewModel, sessionResult);
        var request = new DeleteManager().RequestDeleteCurrent(sessionResult.Session);

        viewModel.ApplyDeletePending(
            request.CurrentPhoto,
            sessionResult.Session.CurrentIndex,
            sessionResult.Session.ActiveCount);

        Assert.AreEqual("IMG_0002.jpg", viewModel.CurrentPhoto?.FileName);
        Assert.AreEqual("1 / 2", viewModel.ViewerCounterText);
        Assert.AreEqual("Excluindo foto", viewModel.ViewerStatusText);
        Assert.IsFalse(viewModel.HasCurrentImage);
    }

    [TestMethod]
    public async Task ApplyDeleteResultPreservesLoadedCurrentImageWhenCurrentPhotoDidNotChange()
    {
        var viewModel = new MainPageViewModel();
        var sessionResult = CreateSessionResult("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        await LoadOpenedSessionAsync(viewModel, sessionResult);
        var deleteManager = new DeleteManager();
        var request = deleteManager.RequestDeleteCurrent(sessionResult.Session);
        viewModel.ApplyDeletePending(
            request.CurrentPhoto,
            sessionResult.Session.CurrentIndex,
            sessionResult.Session.ActiveCount);
        viewModel.CompleteImageLoad();
        var completion = deleteManager.CompleteDelete(sessionResult.Session, request.DeletedPhoto!);
        var result = DeleteCurrentPhotoResult.Deleted(
            sessionResult.Session,
            completion.DeletedPhoto,
            completion.CurrentPhoto,
            SuccessMoveResult(completion.DeletedPhoto));

        viewModel.ApplyDeleteResult(result);

        Assert.AreEqual("IMG_0002.jpg", viewModel.CurrentPhoto?.FileName);
        Assert.AreEqual("1 / 2", viewModel.ViewerCounterText);
        Assert.AreEqual("Foto removida", viewModel.ViewerStatusText);
        Assert.IsTrue(viewModel.HasCurrentImage);
    }

    [TestMethod]
    public async Task ApplyDeleteResultWhenMissingShowsMissingStatus()
    {
        var viewModel = new MainPageViewModel();
        var sessionResult = CreateSessionResult("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        await LoadOpenedSessionAsync(viewModel, sessionResult);
        var deleteManager = new DeleteManager();
        var request = deleteManager.RequestDeleteCurrent(sessionResult.Session);
        var completion = deleteManager.MarkMissing(sessionResult.Session, request.DeletedPhoto!, request.CurrentPhoto?.Id);
        var result = DeleteCurrentPhotoResult.Missing(
            sessionResult.Session,
            completion.DeletedPhoto,
            completion.CurrentPhoto,
            FailedMoveResult(completion.DeletedPhoto, FileMoveErrorCode.SourceMissing));

        viewModel.ApplyDeleteResult(result);

        Assert.AreEqual("IMG_0002.jpg", viewModel.CurrentPhoto?.FileName);
        Assert.AreEqual("1 / 2", viewModel.ViewerCounterText);
        Assert.AreEqual("Arquivo ausente", viewModel.ViewerStatusText);
        Assert.IsFalse(viewModel.HasCurrentImage);
    }

    [TestMethod]
    public async Task ApplyUndoResultWhenRestoredShowsRestoredPhoto()
    {
        var viewModel = new MainPageViewModel();
        var sessionResult = CreateSessionResult("IMG_0001.jpg", "IMG_0002.jpg");
        await LoadOpenedSessionAsync(viewModel, sessionResult);
        sessionResult.Session.Photos[0].SetStatus(PhotoStatus.Restored);
        var result = UndoLastDeleteResult.Restored(
            sessionResult.Session,
            sessionResult.Session.Photos[0],
            sessionResult.Session.Photos[0],
            SuccessRestoreResult(sessionResult.Session.Photos[0]));

        viewModel.ApplyUndoResult(result);

        Assert.AreEqual("IMG_0001.jpg", viewModel.CurrentPhoto?.FileName);
        Assert.AreEqual("1 / 2", viewModel.ViewerCounterText);
        Assert.AreEqual("Foto restaurada", viewModel.ViewerStatusText);
        Assert.IsFalse(viewModel.HasCurrentImage);
    }

    [TestMethod]
    public async Task ApplyUndoResultWhenNoUndoAvailablePreservesCurrentPhoto()
    {
        var viewModel = new MainPageViewModel();
        var sessionResult = CreateSessionResult("IMG_0001.jpg", "IMG_0002.jpg");
        await LoadOpenedSessionAsync(viewModel, sessionResult);
        viewModel.CompleteImageLoad();

        viewModel.ApplyUndoResult(UndoLastDeleteResult.NoUndoAvailable(sessionResult.Session));

        Assert.AreEqual("IMG_0001.jpg", viewModel.CurrentPhoto?.FileName);
        Assert.AreEqual("1 / 2", viewModel.ViewerCounterText);
        Assert.AreEqual("Nada para desfazer", viewModel.ViewerStatusText);
        Assert.IsTrue(viewModel.HasCurrentImage);
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

    private static Task<OpenFolderFromArgumentsResult?> LoadOpenedSessionAsync(
        MainPageViewModel viewModel,
        OpenSessionResult sessionResult)
    {
        var result = OpenFolderFromArgumentsResult.Opened(
            new FolderLaunchArguments("C:\\Sessao Cliente", "launcher"),
            sessionResult);
        return viewModel.LoadInitialSessionAsync(Task.FromResult(result));
    }

    private static FileMoveResult SuccessMoveResult(PhotoItem photo)
    {
        var deletedPath = $"C:\\Sessao Cliente\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Success(
            photo.FullPath,
            deletedPath,
            deletedPath,
            collisionResolved: false,
            DateTimeOffset.UtcNow);
    }

    private static FileMoveResult SuccessRestoreResult(PhotoItem photo)
    {
        var deletedPath = $"C:\\Sessao Cliente\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Success(
            deletedPath,
            photo.FullPath,
            photo.FullPath,
            collisionResolved: false,
            DateTimeOffset.UtcNow);
    }

    private static FileMoveResult FailedMoveResult(PhotoItem photo, FileMoveErrorCode errorCode)
    {
        var deletedPath = $"C:\\Sessao Cliente\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Failure(
            photo.FullPath,
            deletedPath,
            errorCode,
            "move failed");
    }
}
