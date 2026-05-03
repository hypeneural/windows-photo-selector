using Evydencia.PhotoSelector.Core.Photos;

namespace Evydencia.PhotoSelector.Core.Tests.Photos;

[TestClass]
public sealed class PhotoItemTests
{
    [TestMethod]
    public void ConstructorRejectsEmptyId()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new PhotoItem(
            Guid.Empty,
            "IMG_0001.jpg",
            "C:\\sessao\\IMG_0001.jpg",
            "C:\\sessao",
            ".jpg",
            1024,
            DateTimeOffset.UtcNow,
            0));
    }

    [TestMethod]
    public void PendingDeleteCountsAsDeletedAndLeavesNavigation()
    {
        var photo = TestPhotoFactory.Photo(status: PhotoStatus.PendingDelete);

        Assert.IsFalse(photo.IsAvailableForNavigation);
        Assert.IsTrue(photo.CountsAsDeleted);
    }

    [TestMethod]
    public void RestoredPhotoIsAvailableForNavigation()
    {
        var photo = TestPhotoFactory.Photo(status: PhotoStatus.Restored);

        Assert.IsTrue(photo.IsAvailableForNavigation);
        Assert.IsFalse(photo.CountsAsDeleted);
    }
}
