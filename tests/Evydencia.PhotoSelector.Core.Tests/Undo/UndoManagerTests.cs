using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;

namespace Evydencia.PhotoSelector.Core.Tests.Undo;

[TestClass]
public sealed class UndoManagerTests
{
    [TestMethod]
    public void RequestRestoreLastWhenNoOperationReturnsNoUndoAvailable()
    {
        var session = SessionWithThreePhotos();
        var manager = new UndoManager();

        var result = manager.RequestRestoreLast(session);

        Assert.AreEqual(UndoRestoreRequestStatus.NoUndoAvailable, result.Status);
        Assert.IsNull(result.RestoredPhoto);
        Assert.IsFalse(manager.CanUndo(session));
    }

    [TestMethod]
    public void RequestRestoreLastMarksLastDeletedPhotoPendingRestore()
    {
        var session = SessionWithThreePhotos();
        var manager = new UndoManager();
        var deletedPhoto = DeleteCurrentAndRegister(session, manager);

        var result = manager.RequestRestoreLast(session);

        Assert.AreEqual(UndoRestoreRequestStatus.PendingRestore, result.Status);
        Assert.AreEqual(deletedPhoto.Id, result.RestoredPhoto?.Id);
        Assert.AreEqual(PhotoStatus.PendingRestore, result.RestoredPhoto?.Status);
        Assert.AreEqual("IMG_0002.jpg", result.PreferredCurrentPhoto?.FileName);
        Assert.IsTrue(manager.CanUndo(session));
    }

    [TestMethod]
    public void CompleteRestoreMarksPhotoRestoredPopsOperationAndNavigatesToRestoredPhoto()
    {
        var session = SessionWithThreePhotos();
        var manager = new UndoManager();
        DeleteCurrentAndRegister(session, manager);
        var request = manager.RequestRestoreLast(session);

        var result = manager.CompleteRestore(session, request.Operation!, request.RestoredPhoto!);

        Assert.AreEqual(UndoRestoreCompletionStatus.Restored, result.Status);
        Assert.AreEqual(PhotoStatus.Restored, result.RestoredPhoto.Status);
        Assert.AreEqual("IMG_0001.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(0, result.CurrentIndex);
        Assert.AreEqual(3, result.ActiveCount);
        Assert.AreEqual(0, result.DeletedCount);
        Assert.IsFalse(manager.CanUndo(session));
    }

    [TestMethod]
    public void FailRestoreMarksPhotoDeletedKeepsUndoAndPreservesPreferredCurrentPhoto()
    {
        var session = SessionWithThreePhotos();
        var manager = new UndoManager();
        DeleteCurrentAndRegister(session, manager);
        var request = manager.RequestRestoreLast(session);

        var result = manager.FailRestore(
            session,
            request.Operation!,
            request.RestoredPhoto!,
            request.PreferredCurrentPhoto?.Id);

        Assert.AreEqual(UndoRestoreCompletionStatus.RestoreFailed, result.Status);
        Assert.AreEqual(PhotoStatus.Deleted, result.RestoredPhoto.Status);
        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(0, result.CurrentIndex);
        Assert.AreEqual(2, result.ActiveCount);
        Assert.AreEqual(1, result.DeletedCount);
        Assert.IsTrue(manager.CanUndo(session));
    }

    [TestMethod]
    public void UndoUsesLastDeletedPhotoFirst()
    {
        var session = SessionWithThreePhotos();
        var manager = new UndoManager();
        DeleteCurrentAndRegister(session, manager);
        DeleteCurrentAndRegister(session, manager);

        var result = manager.RequestRestoreLast(session);

        Assert.AreEqual("IMG_0002.jpg", result.RestoredPhoto?.FileName);
        Assert.AreEqual(PhotoStatus.PendingRestore, result.RestoredPhoto?.Status);
    }

    private static PhotoItem DeleteCurrentAndRegister(PhotoSession session, UndoManager undoManager)
    {
        var deleteManager = new DeleteManager();
        var request = deleteManager.RequestDeleteCurrent(session);
        var completion = deleteManager.CompleteDelete(session, request.DeletedPhoto!);
        var deletedPath = $"C:\\sessao\\_deletadas_evydencia\\{completion.DeletedPhoto.FileName}";
        undoManager.RegisterDeletedPhoto(session, completion.DeletedPhoto, deletedPath);
        return completion.DeletedPhoto;
    }

    private static PhotoSession SessionWithThreePhotos()
    {
        return new PhotoSession(
            Guid.NewGuid(),
            "C:\\sessao",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [
                TestPhotoFactory.Photo("IMG_0001.jpg", sortIndex: 0),
                TestPhotoFactory.Photo("IMG_0002.jpg", sortIndex: 1),
                TestPhotoFactory.Photo("IMG_0003.jpg", sortIndex: 2)
            ]);
    }
}
