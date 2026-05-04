using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Tests.Deletion;

[TestClass]
public sealed class DeleteManagerTests
{
    [TestMethod]
    public void RequestDeleteCurrentMarksCurrentPendingAndMovesToNextPhoto()
    {
        var session = SessionWithThreePhotos();
        var manager = new DeleteManager();

        var result = manager.RequestDeleteCurrent(session);

        Assert.AreEqual(DeleteRequestStatus.PendingDelete, result.Status);
        Assert.AreEqual("IMG_0001.jpg", result.DeletedPhoto?.FileName);
        Assert.AreEqual(PhotoStatus.PendingDelete, result.DeletedPhoto?.Status);
        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(2, result.ActiveCount);
        Assert.AreEqual(1, result.DeletedCount);
        Assert.AreEqual(0, result.CurrentIndex);
    }

    [TestMethod]
    public void RequestDeleteCurrentWhenLastPhotoMovesToPreviousPhoto()
    {
        var session = SessionWithThreePhotos(currentIndex: 2);
        var manager = new DeleteManager();

        var result = manager.RequestDeleteCurrent(session);

        Assert.AreEqual("IMG_0003.jpg", result.DeletedPhoto?.FileName);
        Assert.AreEqual(PhotoStatus.PendingDelete, result.DeletedPhoto?.Status);
        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(1, result.CurrentIndex);
    }

    [TestMethod]
    public void CompleteDeleteMarksPhotoDeletedAndKeepsCurrentPhoto()
    {
        var session = SessionWithThreePhotos();
        var manager = new DeleteManager();
        var request = manager.RequestDeleteCurrent(session);

        var completion = manager.CompleteDelete(session, request.DeletedPhoto!);

        Assert.AreEqual(DeleteCompletionStatus.Deleted, completion.Status);
        Assert.AreEqual(PhotoStatus.Deleted, completion.DeletedPhoto.Status);
        Assert.AreEqual("IMG_0002.jpg", completion.CurrentPhoto?.FileName);
        Assert.AreEqual(2, completion.ActiveCount);
        Assert.AreEqual(1, completion.DeletedCount);
    }

    [TestMethod]
    public void FailDeleteMarksPhotoFailedAndPreservesPreferredCurrentPhoto()
    {
        var session = SessionWithThreePhotos();
        var manager = new DeleteManager();
        var request = manager.RequestDeleteCurrent(session);

        var completion = manager.FailDelete(session, request.DeletedPhoto!, request.CurrentPhoto?.Id);

        Assert.AreEqual(DeleteCompletionStatus.DeleteFailed, completion.Status);
        Assert.AreEqual(PhotoStatus.DeleteFailed, completion.DeletedPhoto.Status);
        Assert.AreEqual("IMG_0002.jpg", completion.CurrentPhoto?.FileName);
        Assert.AreEqual(3, completion.ActiveCount);
        Assert.AreEqual(0, completion.DeletedCount);
        Assert.AreEqual(1, completion.CurrentIndex);
    }

    [TestMethod]
    public void RequestDeleteCurrentWhenNoActivePhotoReturnsNoCurrentPhoto()
    {
        var session = new PhotoSession(
            Guid.NewGuid(),
            "C:\\sessao",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [
                TestPhotoFactory.Photo("IMG_0001.jpg", sortIndex: 0, status: PhotoStatus.Deleted),
                TestPhotoFactory.Photo("IMG_0002.jpg", sortIndex: 1, status: PhotoStatus.Missing)
            ]);
        var manager = new DeleteManager();

        var result = manager.RequestDeleteCurrent(session);

        Assert.AreEqual(DeleteRequestStatus.NoCurrentPhoto, result.Status);
        Assert.IsNull(result.DeletedPhoto);
        Assert.IsNull(result.CurrentPhoto);
        Assert.AreEqual(0, result.ActiveCount);
    }

    private static PhotoSession SessionWithThreePhotos(int currentIndex = 0)
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
            ],
            currentIndex);
    }
}
