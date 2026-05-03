using Evydencia.PhotoSelector.Application.Display;

namespace Evydencia.PhotoSelector.Application.Tests.Display;

[TestClass]
public sealed class DisplayContextSnapshotTests
{
    [TestMethod]
    public void ConvertsDipsToPhysicalPixels()
    {
        var context = new DisplayContextSnapshot(
            "display-1",
            effectiveWidthDips: 1920,
            effectiveHeightDips: 1080,
            viewerUsableWidthDips: 1600,
            viewerUsableHeightDips: 900,
            rasterizationScale: 1.5,
            isFullscreen: true,
            role: DisplayRole.Customer);

        Assert.AreEqual(2880, context.EffectiveWidthPixels);
        Assert.AreEqual(1620, context.EffectiveHeightPixels);
        Assert.AreEqual(2400, context.ViewerUsableWidthPixels);
        Assert.AreEqual(1350, context.ViewerUsableHeightPixels);
        Assert.IsTrue(context.IsFullscreen);
        Assert.AreEqual(DisplayRole.Customer, context.Role);
    }

    [TestMethod]
    public void RejectsNonPositiveSizes()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DisplayContextSnapshot(
            "display-1",
            effectiveWidthDips: 0,
            effectiveHeightDips: 1080,
            viewerUsableWidthDips: 1600,
            viewerUsableHeightDips: 900,
            rasterizationScale: 1,
            isFullscreen: false));
    }
}
