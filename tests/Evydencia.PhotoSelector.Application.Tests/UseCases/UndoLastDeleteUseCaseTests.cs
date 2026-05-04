using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.Tests.Fakes;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Undo;

namespace Evydencia.PhotoSelector.Application.Tests.UseCases;

[TestClass]
public sealed class UndoLastDeleteUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsyncWhenNoUndoOperationReturnsNoUndoAvailable()
    {
        var session = SessionFactory.Create("IMG_0001.jpg");
        var fileMoveService = new FakeFileMoveService(FailedRestoreResult(session.Photos[0]));
        var useCase = new UndoLastDeleteUseCase(new UndoManager(), fileMoveService);

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(UndoLastDeleteStatus.NoUndoAvailable, result.Status);
        Assert.IsNull(result.RestoredPhoto);
        Assert.IsNull(fileMoveService.LastDeletedPath);
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenRestoreSucceedsMarksPhotoRestoredAndMovesToRestoredPhoto()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        var undoManager = new UndoManager();
        await DeleteCurrentAsync(session, undoManager);
        var fileMoveService = new FakeFileMoveService(
            FailedRestoreResult(session.Photos[0]),
            SuccessRestoreResult(session.Photos[0]));
        var useCase = new UndoLastDeleteUseCase(undoManager, fileMoveService);

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(UndoLastDeleteStatus.Restored, result.Status);
        Assert.AreEqual("IMG_0001.jpg", result.RestoredPhoto?.FileName);
        Assert.AreEqual(PhotoStatus.Restored, result.RestoredPhoto?.Status);
        Assert.AreEqual("IMG_0001.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(0, result.CurrentIndex);
        Assert.AreEqual(3, result.ActiveCount);
        Assert.AreEqual(0, result.DeletedCount);
        Assert.AreEqual($"C:\\sessao\\_deletadas_evydencia\\IMG_0001.jpg", fileMoveService.LastDeletedPath);
        Assert.AreEqual(session.Photos[0].FullPath, fileMoveService.LastOriginalPath);
        Assert.IsFalse(undoManager.CanUndo(session));
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenRestoreFailsMarksPhotoDeletedAndKeepsUndoAvailable()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        var undoManager = new UndoManager();
        await DeleteCurrentAsync(session, undoManager);
        var fileMoveService = new FakeFileMoveService(
            FailedRestoreResult(session.Photos[0]),
            FailedRestoreResult(session.Photos[0]));
        var useCase = new UndoLastDeleteUseCase(undoManager, fileMoveService);

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(UndoLastDeleteStatus.RestoreFailed, result.Status);
        Assert.AreEqual(PhotoStatus.Deleted, result.RestoredPhoto?.Status);
        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(2, result.ActiveCount);
        Assert.AreEqual(1, result.DeletedCount);
        Assert.IsTrue(undoManager.CanUndo(session));
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenRestoreIsCanceledDoesNotLeavePendingRestore()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg");
        var undoManager = new UndoManager();
        await DeleteCurrentAsync(session, undoManager);
        var fileMoveService = new FakeFileMoveService(
            FailedRestoreResult(session.Photos[0]),
            SuccessRestoreResult(session.Photos[0]),
            cancelRestore: true);
        var useCase = new UndoLastDeleteUseCase(undoManager, fileMoveService);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => useCase.ExecuteAsync(session));

        Assert.AreEqual(PhotoStatus.Deleted, session.Photos[0].Status);
        Assert.AreEqual(1, session.ActiveCount);
        Assert.AreEqual(1, session.DeletedCount);
        Assert.IsTrue(undoManager.CanUndo(session));
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenRestoreUsesCollisionPathUpdatesPhotoLocation()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg");
        var undoManager = new UndoManager();
        await DeleteCurrentAsync(session, undoManager);
        var collisionPath = "C:\\sessao\\IMG_0001__restored_20260504_120000_001.jpg";
        var fileMoveService = new FakeFileMoveService(
            FailedRestoreResult(session.Photos[0]),
            FileMoveResult.Success(
                "C:\\sessao\\_deletadas_evydencia\\IMG_0001.jpg",
                "C:\\sessao\\IMG_0001.jpg",
                collisionPath,
                collisionResolved: true,
                DateTimeOffset.UtcNow));
        var useCase = new UndoLastDeleteUseCase(undoManager, fileMoveService);

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(UndoLastDeleteStatus.Restored, result.Status);
        Assert.AreEqual(collisionPath, result.RestoredPhoto?.FullPath);
        Assert.AreEqual(Path.GetFileName(collisionPath), result.RestoredPhoto?.FileName);
        Assert.AreEqual(Path.GetDirectoryName(collisionPath), result.RestoredPhoto?.OriginalDirectory);
        Assert.AreEqual(".jpg", result.RestoredPhoto?.Extension);
    }

    private static async Task DeleteCurrentAsync(
        Core.Sessions.PhotoSession session,
        UndoManager undoManager)
    {
        var deleteUseCase = new DeleteCurrentPhotoUseCase(
            new DeleteManager(),
            new FakeFileMoveService(SuccessDeleteResult(session.Photos[0])),
            undoManager);
        var deleteResult = await deleteUseCase.ExecuteAsync(session);

        Assert.AreEqual(DeleteCurrentPhotoStatus.Deleted, deleteResult.Status);
    }

    private static FileMoveResult SuccessDeleteResult(PhotoItem photo)
    {
        var deletedPath = $"C:\\sessao\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Success(
            photo.FullPath,
            deletedPath,
            deletedPath,
            collisionResolved: false,
            DateTimeOffset.UtcNow);
    }

    private static FileMoveResult SuccessRestoreResult(PhotoItem photo)
    {
        var deletedPath = $"C:\\sessao\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Success(
            deletedPath,
            photo.FullPath,
            photo.FullPath,
            collisionResolved: false,
            DateTimeOffset.UtcNow);
    }

    private static FileMoveResult FailedRestoreResult(PhotoItem photo)
    {
        var deletedPath = $"C:\\sessao\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Failure(
            deletedPath,
            photo.FullPath,
            FileMoveErrorCode.IoFailure,
            "restore failed");
    }
}
