using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.Tests.Fakes;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Tests.UseCases;

[TestClass]
public sealed class OpenSessionUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsyncCreatesSessionFromScannerCandidates()
    {
        var useCase = new OpenSessionUseCase(
            new FakeFolderScanner([
                Candidate("IMG_0002.jpg"),
                Candidate("IMG_0001.jpg")
            ]),
            new PhotoSessionFactory());

        var result = await useCase.ExecuteAsync(new OpenSessionCommand("C:\\sessao"));

        Assert.AreEqual(2, result.Session.InitialCount);
        Assert.AreEqual(2, result.Session.ActiveCount);
        Assert.AreEqual("IMG_0001.jpg", result.CurrentPhoto?.FileName);
        CollectionAssert.AreEqual(
            new List<string> { "IMG_0001.jpg", "IMG_0002.jpg" },
            result.Session.Photos.Select(photo => photo.FileName).ToList());
    }

    private static PhotoFileCandidate Candidate(string fileName)
    {
        return new PhotoFileCandidate(
            fileName,
            $"C:\\sessao\\{fileName}",
            "C:\\sessao",
            fileName[fileName.LastIndexOf('.')..],
            1024,
            DateTimeOffset.UtcNow,
            0);
    }
}
