using Evydencia.PhotoSelector.Imaging.Decode;

namespace Evydencia.PhotoSelector.Imaging.Tests.Decode;

[TestClass]
public sealed class JpegSignatureProbeTests
{
    [TestMethod]
    public async Task ProbeAsyncReturnsNoneForJpegSignature()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 8, height: 6);

        var result = await new JpegSignatureProbe().ProbeAsync(filePath);

        Assert.AreEqual(ImageDecodeErrorCode.None, result);
    }

    [TestMethod]
    public async Task ProbeAsyncReturnsUnsupportedForNonJpegContent()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        await File.WriteAllTextAsync(filePath, "not a jpeg");

        var result = await new JpegSignatureProbe().ProbeAsync(filePath);

        Assert.AreEqual(ImageDecodeErrorCode.UnsupportedOrNotJpeg, result);
    }

    [TestMethod]
    public async Task ProbeAsyncReturnsFileMissingForMissingFile()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "missing.jpg");

        var result = await new JpegSignatureProbe().ProbeAsync(filePath);

        Assert.AreEqual(ImageDecodeErrorCode.FileMissing, result);
    }
}
