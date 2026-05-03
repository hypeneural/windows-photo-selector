using Evydencia.PhotoSelector.Core.Navigation;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Tests.Navigation;

[TestClass]
public sealed class NavigationControllerTests
{
    [TestMethod]
    public void StartsAtFirstActivePhoto()
    {
        var controller = new NavigationController(SessionWithThreePhotos());

        Assert.AreEqual("IMG_0001.jpg", controller.Current?.FileName);
        Assert.AreEqual(0, controller.CurrentActiveIndex);
    }

    [TestMethod]
    public void NextAndPreviousRespectBoundaries()
    {
        var controller = new NavigationController(SessionWithThreePhotos());

        Assert.AreEqual("IMG_0002.jpg", controller.MoveNext()?.FileName);
        Assert.AreEqual("IMG_0003.jpg", controller.MoveNext()?.FileName);
        Assert.AreEqual("IMG_0003.jpg", controller.MoveNext()?.FileName);
        Assert.AreEqual("IMG_0002.jpg", controller.MovePrevious()?.FileName);
        Assert.AreEqual("IMG_0001.jpg", controller.MovePrevious()?.FileName);
        Assert.AreEqual("IMG_0001.jpg", controller.MovePrevious()?.FileName);
    }

    [TestMethod]
    public void HideCurrentMovesToNextPhoto()
    {
        var controller = new NavigationController(SessionWithThreePhotos());

        var current = controller.Current;
        var next = controller.HideCurrent(PhotoStatus.PendingDelete);

        Assert.AreEqual(PhotoStatus.PendingDelete, current?.Status);
        Assert.AreEqual("IMG_0002.jpg", next?.FileName);
    }

    [TestMethod]
    public void HideLastPhotoMovesToPreviousPhoto()
    {
        var controller = new NavigationController(SessionWithThreePhotos());
        controller.MoveLast();

        var next = controller.HideCurrent(PhotoStatus.PendingDelete);

        Assert.AreEqual("IMG_0002.jpg", next?.FileName);
    }

    [TestMethod]
    public void NavigationSkipsInactivePhotos()
    {
        var session = new PhotoSession(
            Guid.NewGuid(),
            "C:\\sessao",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [
                TestPhotoFactory.Photo("IMG_0001.jpg", sortIndex: 0),
                TestPhotoFactory.Photo("IMG_0002.jpg", sortIndex: 1, status: PhotoStatus.Deleted),
                TestPhotoFactory.Photo("IMG_0003.jpg", sortIndex: 2)
            ]);

        var controller = new NavigationController(session);

        Assert.AreEqual("IMG_0003.jpg", controller.MoveNext()?.FileName);
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
