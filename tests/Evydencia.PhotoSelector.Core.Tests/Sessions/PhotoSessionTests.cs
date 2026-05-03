using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Tests.Sessions;

[TestClass]
public sealed class PhotoSessionTests
{
    [TestMethod]
    public void CountsAreDerivedFromPhotoStatus()
    {
        var session = new PhotoSession(
            Guid.NewGuid(),
            "C:\\sessao",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [
                TestPhotoFactory.Photo("IMG_0001.jpg", sortIndex: 0, status: PhotoStatus.Active),
                TestPhotoFactory.Photo("IMG_0002.jpg", sortIndex: 1, status: PhotoStatus.Deleted),
                TestPhotoFactory.Photo("IMG_0003.jpg", sortIndex: 2, status: PhotoStatus.PendingDelete),
                TestPhotoFactory.Photo("IMG_0004.jpg", sortIndex: 3, status: PhotoStatus.Missing),
                TestPhotoFactory.Photo("IMG_0005.jpg", sortIndex: 4, status: PhotoStatus.Restored)
            ]);

        Assert.AreEqual(5, session.InitialCount);
        Assert.AreEqual(2, session.ActiveCount);
        Assert.AreEqual(2, session.DeletedCount);
    }

    [TestMethod]
    public void ActivePhotosAreOrderedBySortIndex()
    {
        var session = new PhotoSession(
            Guid.NewGuid(),
            "C:\\sessao",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [
                TestPhotoFactory.Photo("IMG_0003.jpg", sortIndex: 2),
                TestPhotoFactory.Photo("IMG_0001.jpg", sortIndex: 0),
                TestPhotoFactory.Photo("IMG_0002.jpg", sortIndex: 1)
            ]);

        CollectionAssert.AreEqual(
            new List<string> { "IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg" },
            session.ActivePhotos().Select(photo => photo.FileName).ToList());
    }
}
