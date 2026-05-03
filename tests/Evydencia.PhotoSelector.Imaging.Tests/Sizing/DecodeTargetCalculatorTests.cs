using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Imaging.Sizing;

namespace Evydencia.PhotoSelector.Imaging.Tests.Sizing;

[TestClass]
public sealed class DecodeTargetCalculatorTests
{
    [TestMethod]
    public void CalculatesLandscapeFitWithQualityMargin()
    {
        var target = new DecodeTargetCalculator().Calculate(new DecodeTargetRequest(
            originalWidth: 6000,
            originalHeight: 4000,
            exifOrientation: 1,
            displayContext: Display(widthPixels: 1920, heightPixels: 1080)));

        Assert.AreEqual(6000, target.OrientedImageWidth);
        Assert.AreEqual(4000, target.OrientedImageHeight);
        Assert.AreEqual(1620, target.FitWidth);
        Assert.AreEqual(1080, target.FitHeight);
        Assert.AreEqual(2160, target.OrientedTargetWidth);
        Assert.AreEqual(1440, target.OrientedTargetHeight);
        Assert.AreEqual(2160, target.DecodePixelWidth);
        Assert.AreEqual(1440, target.DecodePixelHeight);
        Assert.AreEqual(DecodeTargetPrimaryDimension.Width, target.PrimaryDimension);
        Assert.AreEqual(2160, target.PrimaryDecodePixels);
        Assert.IsFalse(target.UsesOriginalPixels);
    }

    [TestMethod]
    public void AppliesExifOrientationBeforeFitAndMapsBackToDecodeDimensions()
    {
        var target = new DecodeTargetCalculator().Calculate(new DecodeTargetRequest(
            originalWidth: 6000,
            originalHeight: 4000,
            exifOrientation: 6,
            displayContext: Display(widthPixels: 1920, heightPixels: 1080)));

        Assert.IsTrue(target.OrientationSwapsDimensions);
        Assert.AreEqual(4000, target.OrientedImageWidth);
        Assert.AreEqual(6000, target.OrientedImageHeight);
        Assert.AreEqual(720, target.FitWidth);
        Assert.AreEqual(1080, target.FitHeight);
        Assert.AreEqual(960, target.OrientedTargetWidth);
        Assert.AreEqual(1440, target.OrientedTargetHeight);
        Assert.AreEqual(1440, target.DecodePixelWidth);
        Assert.AreEqual(960, target.DecodePixelHeight);
    }

    [TestMethod]
    public void DoesNotUpscaleSmallImagesForDecode()
    {
        var target = new DecodeTargetCalculator().Calculate(new DecodeTargetRequest(
            originalWidth: 800,
            originalHeight: 600,
            exifOrientation: 1,
            displayContext: Display(widthPixels: 1920, heightPixels: 1080)));

        Assert.AreEqual(800, target.FitWidth);
        Assert.AreEqual(600, target.FitHeight);
        Assert.AreEqual(800, target.DecodePixelWidth);
        Assert.AreEqual(600, target.DecodePixelHeight);
        Assert.IsTrue(target.UsesOriginalPixels);
    }

    [TestMethod]
    public void UsesPhysicalPixelsFromDisplayContext()
    {
        var target = new DecodeTargetCalculator().Calculate(new DecodeTargetRequest(
            originalWidth: 4000,
            originalHeight: 2000,
            exifOrientation: 1,
            displayContext: new DisplayContextSnapshot(
                "display-2",
                effectiveWidthDips: 1000,
                effectiveHeightDips: 500,
                viewerUsableWidthDips: 1000,
                viewerUsableHeightDips: 500,
                rasterizationScale: 2,
                isFullscreen: false)));

        Assert.AreEqual(2000, target.FitWidth);
        Assert.AreEqual(1000, target.FitHeight);
        Assert.AreEqual(2667, target.DecodePixelWidth);
        Assert.AreEqual(1334, target.DecodePixelHeight);
    }

    [TestMethod]
    public void RejectsQualityMarginOutsideAllowedRange()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DecodeTargetRequest(
            originalWidth: 4000,
            originalHeight: 2000,
            exifOrientation: 1,
            displayContext: Display(widthPixels: 1920, heightPixels: 1080),
            qualityMargin: 1.5));
    }

    private static DisplayContextSnapshot Display(int widthPixels, int heightPixels)
    {
        return new DisplayContextSnapshot(
            "display-1",
            effectiveWidthDips: widthPixels,
            effectiveHeightDips: heightPixels,
            viewerUsableWidthDips: widthPixels,
            viewerUsableHeightDips: heightPixels,
            rasterizationScale: 1,
            isFullscreen: true);
    }
}
