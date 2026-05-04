using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Imaging.Prefetch;

namespace Evydencia.PhotoSelector.Imaging.Tests.Prefetch;

[TestClass]
public sealed class PrefetchSchedulerTests
{
    private static readonly string[] MiddlePrefetchOrder =
    [
        "photo-4.jpg",
        "photo-5.jpg",
        "photo-6.jpg",
        "photo-2.jpg",
        "photo-1.jpg"
    ];

    private static readonly string[] EdgePrefetchOrder =
    [
        "photo-1.jpg",
        "photo-2.jpg"
    ];

    [TestMethod]
    public void BuildPrefetchListReturnsNextThreeThenPreviousTwo()
    {
        var photos = Enumerable
            .Range(0, 8)
            .Select(index => CreatePhoto(index))
            .ToList();
        var session = new PhotoSession(
            Guid.NewGuid(),
            Path.GetTempPath(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            photos,
            currentIndex: 3);

        var prefetch = PrefetchScheduler.BuildPrefetchList(session, photos[3]);

        CollectionAssert.AreEqual(MiddlePrefetchOrder, prefetch.Select(photo => photo.FileName).ToArray());
    }

    [TestMethod]
    public void BuildPrefetchListStopsAtSessionEdges()
    {
        var photos = Enumerable
            .Range(0, 3)
            .Select(index => CreatePhoto(index))
            .ToList();
        var session = new PhotoSession(
            Guid.NewGuid(),
            Path.GetTempPath(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            photos);

        var prefetch = PrefetchScheduler.BuildPrefetchList(session, photos[0]);

        CollectionAssert.AreEqual(EdgePrefetchOrder, prefetch.Select(photo => photo.FileName).ToArray());
    }

    private static PhotoItem CreatePhoto(int index)
    {
        var fileName = $"photo-{index}.jpg";
        return new PhotoItem(
            Guid.NewGuid(),
            fileName,
            Path.Combine(Path.GetTempPath(), fileName),
            Path.GetTempPath(),
            ".jpg",
            sizeBytes: 10,
            DateTimeOffset.UtcNow,
            index);
    }
}
