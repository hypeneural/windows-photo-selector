using Evydencia.PhotoSelector.Application.UseCases;

namespace Evydencia.PhotoSelector.Application.Tests.UseCases;

[TestClass]
public sealed class NavigationUseCaseTests
{
    [TestMethod]
    public void NavigateNextUpdatesSessionCurrentIndex()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        var useCase = new NavigateNextPhotoUseCase();

        var result = useCase.Execute(session);

        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(1, result.CurrentIndex);
    }

    [TestMethod]
    public void NavigatePreviousUsesSessionCurrentIndex()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg", "IMG_0003.jpg");
        var next = new NavigateNextPhotoUseCase();
        next.Execute(session);
        next.Execute(session);
        var useCase = new NavigatePreviousPhotoUseCase();

        var result = useCase.Execute(session);

        Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(1, result.CurrentIndex);
    }

    [TestMethod]
    public void NavigatePreviousStaysAtFirstPhoto()
    {
        var session = SessionFactory.Create("IMG_0001.jpg", "IMG_0002.jpg");
        var useCase = new NavigatePreviousPhotoUseCase();

        var result = useCase.Execute(session);

        Assert.AreEqual("IMG_0001.jpg", result.CurrentPhoto?.FileName);
        Assert.AreEqual(0, result.CurrentIndex);
    }
}
