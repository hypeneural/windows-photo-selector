using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Imaging.Decode;
using Evydencia.PhotoSelector.Imaging.Sizing;

namespace Evydencia.PhotoSelector.Imaging.Tests.Decode;

[TestClass]
public sealed class JpegDecodeServiceTests
{
    [TestMethod]
    public async Task DecodeAsyncDecodesToTargetSize()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 32, height: 16);
        var target = CalculateTarget(originalWidth: 32, originalHeight: 16, displayWidth: 16, displayHeight: 8);

        var result = await new JpegDecodeService().DecodeAsync(new ImageDecodeRequest(filePath, target));

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.AreEqual(target.OrientedTargetWidth, result.PixelWidth);
        Assert.AreEqual(target.OrientedTargetHeight, result.PixelHeight);
        Assert.HasCount(target.OrientedTargetWidth * target.OrientedTargetHeight * 4, result.PixelData);
        Assert.IsFalse(target.UsesOriginalPixels);
    }

    [TestMethod]
    public async Task DecodeAsyncDoesNotKeepFileHandleAfterDecode()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        var movedPath = Path.Combine(folder.Path, "photo-moved.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 24, height: 12);
        var target = CalculateTarget(originalWidth: 24, originalHeight: 12, displayWidth: 12, displayHeight: 6);

        var result = await new JpegDecodeService().DecodeAsync(new ImageDecodeRequest(filePath, target));
        File.Move(filePath, movedPath);

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.IsFalse(File.Exists(filePath));
        Assert.IsTrue(File.Exists(movedPath));
    }

    [TestMethod]
    public async Task DecodeAsyncReturnsControlledErrorForNonJpegFile()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        await File.WriteAllTextAsync(filePath, "not a jpeg");
        var target = CalculateTarget(originalWidth: 24, originalHeight: 12, displayWidth: 12, displayHeight: 6);

        var result = await new JpegDecodeService().DecodeAsync(new ImageDecodeRequest(filePath, target));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ImageDecodeErrorCode.UnsupportedOrNotJpeg, result.ErrorCode);
        Assert.IsEmpty(result.PixelData);
    }

    [TestMethod]
    public async Task DecodeForDisplayAsyncUsesDecoderDimensionsToAvoidFullRes()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "photo.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 80, height: 40);
        var displayContext = CreateDisplayContext(displayWidth: 20, displayHeight: 10);

        var result = await new JpegDecodeService().DecodeForDisplayAsync(filePath, displayContext);

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.IsLessThan(80, result.PixelWidth);
        Assert.IsLessThan(40, result.PixelHeight);
        Assert.HasCount(result.PixelWidth * result.PixelHeight * 4, result.PixelData);
    }

    [TestMethod]
    public async Task DecodeActualSizeAsyncDecodesOriginalDimensions()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "actual-size.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 80, height: 40);

        var result = await new JpegDecodeService().DecodeActualSizeAsync(filePath);

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.AreEqual(80, result.PixelWidth);
        Assert.AreEqual(40, result.PixelHeight);
        Assert.HasCount(80 * 40 * 4, result.PixelData);
    }

    [TestMethod]
    public async Task DecodeActualSizeAsyncDoesNotKeepFileHandleAfterDecode()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "actual-size-handle.jpg");
        var movedPath = Path.Combine(folder.Path, "actual-size-handle-moved.jpg");
        await JpegTestImage.WriteAsync(filePath, width: 24, height: 12);

        var result = await new JpegDecodeService().DecodeActualSizeAsync(filePath);
        File.Move(filePath, movedPath);

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.IsFalse(File.Exists(filePath));
        Assert.IsTrue(File.Exists(movedPath));
    }

    [TestMethod]
    public async Task DecodeActualSizeAsyncRespectsExifOrientation6()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "actual-size-orientation-6.jpg");
        await JpegTestImage.WriteQuadrantsWithOrientationAsync(filePath, width: 80, height: 40, exifOrientation: 6);

        var result = await new JpegDecodeService().DecodeActualSizeAsync(filePath);

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.AreEqual(40, result.PixelWidth);
        Assert.AreEqual(80, result.PixelHeight);
        Assert.AreEqual(TestQuadrantColor.Blue, Sample(result, 0.25, 0.25));
        Assert.AreEqual(TestQuadrantColor.Red, Sample(result, 0.75, 0.25));
        Assert.AreEqual(TestQuadrantColor.Yellow, Sample(result, 0.25, 0.75));
        Assert.AreEqual(TestQuadrantColor.Green, Sample(result, 0.75, 0.75));
    }

    [TestMethod]
    public async Task DecodeForDisplayAsyncRespectsExifOrientation6WithoutDoubleApplyingDimensions()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "orientation-display-6.jpg");
        await JpegTestImage.WriteQuadrantsWithOrientationAsync(filePath, width: 80, height: 40, exifOrientation: 6);
        var displayContext = CreateDisplayContext(displayWidth: 20, displayHeight: 40);

        var result = await new JpegDecodeService().DecodeForDisplayAsync(filePath, displayContext);

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.IsLessThan(80, result.PixelWidth);
        Assert.IsLessThanOrEqualTo(80, result.PixelHeight);
        Assert.AreEqual(TestQuadrantColor.Blue, Sample(result, 0.25, 0.25));
        Assert.AreEqual(TestQuadrantColor.Red, Sample(result, 0.75, 0.25));
        Assert.AreEqual(TestQuadrantColor.Yellow, Sample(result, 0.25, 0.75));
        Assert.AreEqual(TestQuadrantColor.Green, Sample(result, 0.75, 0.75));
    }

    [TestMethod]
    public async Task DecodeAsyncRespectsExifOrientation6WithoutDoubleApplyingDimensions()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "orientation-6.jpg");
        await JpegTestImage.WriteQuadrantsWithOrientationAsync(filePath, width: 80, height: 40, exifOrientation: 6);
        var target = CalculateTarget(
            originalWidth: 80,
            originalHeight: 40,
            exifOrientation: 6,
            displayWidth: 20,
            displayHeight: 40);

        var result = await new JpegDecodeService().DecodeAsync(new ImageDecodeRequest(filePath, target));

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.AreEqual(target.OrientedTargetWidth, result.PixelWidth);
        Assert.AreEqual(target.OrientedTargetHeight, result.PixelHeight);
        Assert.AreEqual(TestQuadrantColor.Blue, Sample(result, 0.25, 0.25));
        Assert.AreEqual(TestQuadrantColor.Red, Sample(result, 0.75, 0.25));
        Assert.AreEqual(TestQuadrantColor.Yellow, Sample(result, 0.25, 0.75));
        Assert.AreEqual(TestQuadrantColor.Green, Sample(result, 0.75, 0.75));
    }

    [TestMethod]
    public async Task DecodeAsyncRespectsExifOrientation8WithoutDoubleApplyingDimensions()
    {
        using var folder = TemporaryFolder.Create();
        var filePath = Path.Combine(folder.Path, "orientation-8.jpg");
        await JpegTestImage.WriteQuadrantsWithOrientationAsync(filePath, width: 80, height: 40, exifOrientation: 8);
        var target = CalculateTarget(
            originalWidth: 80,
            originalHeight: 40,
            exifOrientation: 8,
            displayWidth: 20,
            displayHeight: 40);

        var result = await new JpegDecodeService().DecodeAsync(new ImageDecodeRequest(filePath, target));

        Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
        Assert.AreEqual(target.OrientedTargetWidth, result.PixelWidth);
        Assert.AreEqual(target.OrientedTargetHeight, result.PixelHeight);
        Assert.AreEqual(TestQuadrantColor.Green, Sample(result, 0.25, 0.25));
        Assert.AreEqual(TestQuadrantColor.Yellow, Sample(result, 0.75, 0.25));
        Assert.AreEqual(TestQuadrantColor.Red, Sample(result, 0.25, 0.75));
        Assert.AreEqual(TestQuadrantColor.Blue, Sample(result, 0.75, 0.75));
    }

    private static DecodeTarget CalculateTarget(
        int originalWidth,
        int originalHeight,
        int displayWidth,
        int displayHeight)
    {
        return CalculateTarget(
            originalWidth,
            originalHeight,
            exifOrientation: 1,
            displayWidth,
            displayHeight);
    }

    private static DecodeTarget CalculateTarget(
        int originalWidth,
        int originalHeight,
        int exifOrientation,
        int displayWidth,
        int displayHeight)
    {
        return new DecodeTargetCalculator().Calculate(new DecodeTargetRequest(
            originalWidth,
            originalHeight,
            exifOrientation,
            displayContext: new DisplayContextSnapshot(
                "test-display",
                effectiveWidthDips: displayWidth,
                effectiveHeightDips: displayHeight,
                viewerUsableWidthDips: displayWidth,
                viewerUsableHeightDips: displayHeight,
                rasterizationScale: 1,
                isFullscreen: true)));
    }

    private static DisplayContextSnapshot CreateDisplayContext(int displayWidth, int displayHeight)
    {
        return new DisplayContextSnapshot(
            "test-display",
            effectiveWidthDips: displayWidth,
            effectiveHeightDips: displayHeight,
            viewerUsableWidthDips: displayWidth,
            viewerUsableHeightDips: displayHeight,
            rasterizationScale: 1,
            isFullscreen: true);
    }

    private static TestQuadrantColor Sample(ImageDecodeResult result, double xRatio, double yRatio)
    {
        var x = Math.Clamp((int)Math.Round((result.PixelWidth - 1) * xRatio), 0, result.PixelWidth - 1);
        var y = Math.Clamp((int)Math.Round((result.PixelHeight - 1) * yRatio), 0, result.PixelHeight - 1);
        var offset = checked((y * result.PixelWidth + x) * 4);
        var b = result.PixelData[offset];
        var g = result.PixelData[offset + 1];
        var r = result.PixelData[offset + 2];

        return NearestColor(r, g, b);
    }

    private static TestQuadrantColor NearestColor(byte r, byte g, byte b)
    {
        var redDistance = Distance(r, g, b, expectedR: 230, expectedG: 20, expectedB: 20);
        var greenDistance = Distance(r, g, b, expectedR: 20, expectedG: 230, expectedB: 20);
        var blueDistance = Distance(r, g, b, expectedR: 20, expectedG: 20, expectedB: 230);
        var yellowDistance = Distance(r, g, b, expectedR: 230, expectedG: 230, expectedB: 20);
        var min = Math.Min(Math.Min(redDistance, greenDistance), Math.Min(blueDistance, yellowDistance));

        if (min == redDistance)
        {
            return TestQuadrantColor.Red;
        }

        if (min == greenDistance)
        {
            return TestQuadrantColor.Green;
        }

        return min == blueDistance ? TestQuadrantColor.Blue : TestQuadrantColor.Yellow;
    }

    private static int Distance(
        byte r,
        byte g,
        byte b,
        int expectedR,
        int expectedG,
        int expectedB)
    {
        var deltaR = r - expectedR;
        var deltaG = g - expectedG;
        var deltaB = b - expectedB;
        return (deltaR * deltaR) + (deltaG * deltaG) + (deltaB * deltaB);
    }

    private enum TestQuadrantColor
    {
        Red,
        Green,
        Blue,
        Yellow
    }
}
