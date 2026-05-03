using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Tests.Sessions;

[TestClass]
public sealed class PhotoSessionFactoryTests
{
    [TestMethod]
    public void CreateOrdersPhotosByNameInitially()
    {
        var factory = new PhotoSessionFactory();

        var session = factory.Create(
            "C:\\sessao",
            [
                Candidate("IMG_0010.jpg", sortIndex: 0),
                Candidate("IMG_0002.jpg", sortIndex: 1),
                Candidate("IMG_0001.jpg", sortIndex: 2)
            ],
            new DateTimeOffset(2026, 5, 3, 10, 0, 0, TimeSpan.Zero));

        CollectionAssert.AreEqual(
            new List<string> { "IMG_0001.jpg", "IMG_0002.jpg", "IMG_0010.jpg" },
            session.Photos.Select(photo => photo.FileName).ToList());
        CollectionAssert.AreEqual(
            new List<int> { 0, 1, 2 },
            session.Photos.Select(photo => photo.SortIndex).ToList());
    }

    [TestMethod]
    public void CreatePreservesCheapFileInfo()
    {
        var lastWrite = new DateTimeOffset(2026, 5, 3, 10, 0, 0, TimeSpan.Zero);
        var factory = new PhotoSessionFactory();

        var session = factory.Create(
            "C:\\sessao",
            [Candidate("IMG_0001.jpeg", sizeBytes: 2048, lastWriteTimeUtc: lastWrite)]);

        var photo = session.Photos.Single();
        Assert.AreEqual("IMG_0001.jpeg", photo.FileName);
        Assert.AreEqual("C:\\sessao\\IMG_0001.jpeg", photo.FullPath);
        Assert.AreEqual("C:\\sessao", photo.OriginalDirectory);
        Assert.AreEqual(".jpeg", photo.Extension);
        Assert.AreEqual(2048, photo.SizeBytes);
        Assert.AreEqual(lastWrite, photo.LastWriteTimeUtc);
    }

    private static PhotoFileCandidate Candidate(
        string fileName,
        int sortIndex = 0,
        long sizeBytes = 1024,
        DateTimeOffset? lastWriteTimeUtc = null)
    {
        return new PhotoFileCandidate(
            fileName,
            $"C:\\sessao\\{fileName}",
            "C:\\sessao",
            fileName[fileName.LastIndexOf('.')..],
            sizeBytes,
            lastWriteTimeUtc ?? DateTimeOffset.UtcNow,
            sortIndex);
    }
}
