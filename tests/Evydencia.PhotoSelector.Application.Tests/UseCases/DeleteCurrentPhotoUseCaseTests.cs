using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.Tests.Fakes;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;

namespace Evydencia.PhotoSelector.Application.Tests.UseCases;

[TestClass]
public sealed class DeleteCurrentPhotoUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsyncWhenMoveSucceedsMarksPhotoDeletedAndMovesToNextPhoto()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        var fileMoveService = new FakeFileMoveService(SuccessMoveResult(session.Photos[0]));
        var journalStore = new FakeSessionJournalStore();
        var undoManager = new UndoManager();
        var useCase = new DeleteCurrentPhotoUseCase(new DeleteManager(), fileMoveService, undoManager, journalStore);

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(DeleteCurrentPhotoStatus.Deleted, result.Status);
        Assert.AreEqual("IMG_0001.jpg", result.DeletedPhoto?.FileName);
        Assert.AreEqual(PhotoStatus.Deleted, result.DeletedPhoto?.Status);
        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(2, result.ActiveCount);
        Assert.AreEqual(1, result.DeletedCount);
        Assert.AreEqual(session.Photos[0].FullPath, fileMoveService.LastSourcePath);
        Assert.AreEqual(session.FolderPath, fileMoveService.LastSessionFolderPath);
        Assert.IsTrue(undoManager.CanUndo(session));
        CollectionAssert.AreEqual(
            new[] { SessionJournalEventType.DeleteRequested, SessionJournalEventType.Deleted },
            journalStore.Events.Select(journalEvent => journalEvent.EventType).ToArray());
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenDeletingLastPhotoMovesCurrentToPreviousPhoto()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        new NavigateNextPhotoUseCase().Execute(session);
        new NavigateNextPhotoUseCase().Execute(session);
        var fileMoveService = new FakeFileMoveService(SuccessMoveResult(session.Photos[2]));
        var useCase = new DeleteCurrentPhotoUseCase(
            new DeleteManager(),
            fileMoveService,
            new UndoManager(),
            new FakeSessionJournalStore());

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(DeleteCurrentPhotoStatus.Deleted, result.Status);
        Assert.AreEqual("IMG_0003.jpg", result.DeletedPhoto?.FileName);
        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(1, result.CurrentIndex);
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenMoveFailsMarksPhotoDeleteFailedAndPreservesNextPhoto()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        var fileMoveService = new FakeFileMoveService(FailedMoveResult(session.Photos[0]));
        var journalStore = new FakeSessionJournalStore();
        var undoManager = new UndoManager();
        var useCase = new DeleteCurrentPhotoUseCase(new DeleteManager(), fileMoveService, undoManager, journalStore);

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(DeleteCurrentPhotoStatus.DeleteFailed, result.Status);
        Assert.AreEqual(PhotoStatus.DeleteFailed, result.DeletedPhoto?.Status);
        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(3, result.ActiveCount);
        Assert.AreEqual(0, result.DeletedCount);
        Assert.AreEqual(1, result.CurrentIndex);
        Assert.AreEqual(FileMoveErrorCode.IoFailure, result.FileMoveResult?.ErrorCode);
        Assert.IsFalse(undoManager.CanUndo(session));
        CollectionAssert.AreEqual(
            new[] { SessionJournalEventType.DeleteRequested, SessionJournalEventType.DeleteFailed },
            journalStore.Events.Select(journalEvent => journalEvent.EventType).ToArray());
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenNoActivePhotoReturnsNoCurrentPhoto()
    {
        var session = new PhotoSession(
            Guid.NewGuid(),
            "C:\\sessao",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [
                new PhotoItem(
                    Guid.NewGuid(),
                    "IMG_0001.jpg",
                    "C:\\sessao\\IMG_0001.jpg",
                    "C:\\sessao",
                    ".jpg",
                    1024,
                    DateTimeOffset.UtcNow,
                    0,
                    PhotoStatus.Deleted)
            ]);
        var fileMoveService = new FakeFileMoveService(FailedMoveResult(session.Photos[0]));
        var useCase = new DeleteCurrentPhotoUseCase(
            new DeleteManager(),
            fileMoveService,
            new UndoManager(),
            new FakeSessionJournalStore());

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(DeleteCurrentPhotoStatus.NoCurrentPhoto, result.Status);
        Assert.IsNull(result.DeletedPhoto);
        Assert.IsNull(result.CurrentPhoto);
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenMoveIsCanceledDoesNotLeavePendingDelete()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg");
        var fileMoveService = new FakeFileMoveService(SuccessMoveResult(session.Photos[0]), cancelMove: true);
        var journalStore = new FakeSessionJournalStore();
        var undoManager = new UndoManager();
        var useCase = new DeleteCurrentPhotoUseCase(new DeleteManager(), fileMoveService, undoManager, journalStore);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => useCase.ExecuteAsync(session));

        Assert.AreEqual(PhotoStatus.DeleteFailed, session.Photos[0].Status);
        Assert.AreEqual(2, session.ActiveCount);
        Assert.AreEqual(0, session.DeletedCount);
        Assert.AreEqual(1, session.CurrentIndex);
        Assert.IsFalse(undoManager.CanUndo(session));
        CollectionAssert.AreEqual(
            new[] { SessionJournalEventType.DeleteRequested },
            journalStore.Events.Select(journalEvent => journalEvent.EventType).ToArray());
    }

    private static FileMoveResult SuccessMoveResult(PhotoItem photo)
    {
        var deletedPath = $"C:\\sessao\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Success(
            photo.FullPath,
            deletedPath,
            deletedPath,
            collisionResolved: false,
            DateTimeOffset.UtcNow);
    }

    private static FileMoveResult FailedMoveResult(PhotoItem photo)
    {
        var deletedPath = $"C:\\sessao\\_deletadas_evydencia\\{photo.FileName}";
        return FileMoveResult.Failure(
            photo.FullPath,
            deletedPath,
            FileMoveErrorCode.IoFailure,
            "move failed");
    }
}
