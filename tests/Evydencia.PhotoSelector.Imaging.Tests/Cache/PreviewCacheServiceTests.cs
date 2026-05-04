using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Imaging.Cache;
using Evydencia.PhotoSelector.Imaging.Decode;
using Evydencia.PhotoSelector.Imaging.Tests.Decode;

namespace Evydencia.PhotoSelector.Imaging.Tests.Cache;

[TestClass]
public sealed class PreviewCacheServiceTests
{
    [TestMethod]
    public async Task GetOrDecodePreviewAsyncReturnsCachedResultWithoutReopeningFile()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        var movedPath = Path.Combine(folder.Path, "photo-moved.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 64, height: 32);
        var photo = CreatePhoto(filePath);
        var displayContext = CreateDisplayContext(width: 16, height: 8);
        var cache = new MemoryImageCache(new MemoryImageCacheOptions(maxBytes: 1024 * 1024));
        var service = new PreviewCacheService(new JpegDecodeService(), cache);

        var first = await service.GetOrDecodePreviewAsync(photo, displayContext);
        File.Move(filePath, movedPath);
        var second = await service.GetOrDecodePreviewAsync(photo, displayContext);

        Assert.IsTrue(first.IsSuccess, first.ErrorMessage);
        Assert.IsTrue(second.IsSuccess, second.ErrorMessage);
        Assert.AreSame(first, second);
        Assert.AreEqual(1, cache.Count);
    }

    [TestMethod]
    public async Task GetOrDecodeActualSizeAsyncUsesSeparateCacheEntry()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 64, height: 32);
        var photo = CreatePhoto(filePath);
        var displayContext = CreateDisplayContext(width: 16, height: 8);
        var cache = new MemoryImageCache(new MemoryImageCacheOptions(maxBytes: 1024 * 1024));
        var service = new PreviewCacheService(new JpegDecodeService(), cache);

        var preview = await service.GetOrDecodePreviewAsync(photo, displayContext);
        var actualSize = await service.GetOrDecodeActualSizeAsync(photo);

        Assert.IsTrue(preview.IsSuccess, preview.ErrorMessage);
        Assert.IsTrue(actualSize.IsSuccess, actualSize.ErrorMessage);
        Assert.AreEqual(2, cache.Count);
        Assert.AreNotEqual(preview.PixelWidth, actualSize.PixelWidth);
    }

    private static PhotoItem CreatePhoto(string filePath)
    {
        var info = new FileInfo(filePath);
        return new PhotoItem(
            Guid.NewGuid(),
            info.Name,
            info.FullName,
            info.DirectoryName ?? throw new InvalidOperationException("Test file has no directory."),
            info.Extension,
            info.Length,
            info.LastWriteTimeUtc,
            sortIndex: 0);
    }

    private static DisplayContextSnapshot CreateDisplayContext(int width, int height)
    {
        return new DisplayContextSnapshot(
            "test-display",
            effectiveWidthDips: width,
            effectiveHeightDips: height,
            viewerUsableWidthDips: width,
            viewerUsableHeightDips: height,
            rasterizationScale: 1,
            isFullscreen: true);
    }
}
