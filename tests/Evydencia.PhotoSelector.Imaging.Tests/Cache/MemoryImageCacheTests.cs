using Evydencia.PhotoSelector.Imaging.Cache;
using Evydencia.PhotoSelector.Imaging.Decode;

namespace Evydencia.PhotoSelector.Imaging.Tests.Cache;

[TestClass]
public sealed class MemoryImageCacheTests
{
    [TestMethod]
    public void SetEvictsLeastRecentlyUsedEntry()
    {
        var cache = new MemoryImageCache(new MemoryImageCacheOptions(maxBytes: 12));
        var first = CreateKey("first.jpg");
        var second = CreateKey("second.jpg");
        var third = CreateKey("third.jpg");

        cache.Set(first, CreateResult("first.jpg", byteCount: 4));
        cache.Set(second, CreateResult("second.jpg", byteCount: 4));
        Assert.IsTrue(cache.TryGet(first, out _));
        cache.Set(third, CreateResult("third.jpg", byteCount: 8));

        Assert.IsTrue(cache.TryGet(first, out _));
        Assert.IsFalse(cache.TryGet(second, out _));
        Assert.IsTrue(cache.TryGet(third, out _));
        Assert.AreEqual(12, cache.CurrentBytes);
    }

    [TestMethod]
    public void SetSkipsEntriesLargerThanLimit()
    {
        var cache = new MemoryImageCache(new MemoryImageCacheOptions(maxBytes: 4));
        var key = CreateKey("large.jpg");

        var stored = cache.Set(key, CreateResult("large.jpg", byteCount: 8));

        Assert.IsFalse(stored);
        Assert.IsFalse(cache.TryGet(key, out _));
        Assert.AreEqual(0, cache.Count);
        Assert.AreEqual(0, cache.CurrentBytes);
    }

    [TestMethod]
    public void ImageCacheKeyChangesWhenFileVersionChanges()
    {
        var first = CreateKey("same.jpg", sizeBytes: 10, lastWriteTicks: 20);
        var changedSize = CreateKey("same.jpg", sizeBytes: 11, lastWriteTicks: 20);
        var changedTime = CreateKey("same.jpg", sizeBytes: 10, lastWriteTicks: 21);

        Assert.AreNotEqual(first, changedSize);
        Assert.AreNotEqual(first, changedTime);
    }

    private static ImageCacheKey CreateKey(
        string fileName,
        long sizeBytes = 10,
        long lastWriteTicks = 20)
    {
        return new ImageCacheKey(
            Path.Combine(Path.GetTempPath(), fileName).ToUpperInvariant(),
            sizeBytes,
            new DateTimeOffset(lastWriteTicks, TimeSpan.Zero),
            ImageCacheMode.Preview,
            TargetWidthPixels: 16,
            TargetHeightPixels: 8,
            ImageCacheKey.CurrentAlgorithmVersion);
    }

    private static ImageDecodeResult CreateResult(string fileName, int byteCount)
    {
        return ImageDecodeResult.Success(
            Path.Combine(Path.GetTempPath(), fileName),
            pixelWidth: 1,
            pixelHeight: byteCount / 4,
            new byte[byteCount]);
    }
}
