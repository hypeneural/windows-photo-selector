namespace Evydencia.PhotoSelector.Imaging.Sizing;

public sealed class DecodeTargetCalculator
{
    public DecodeTarget Calculate(DecodeTargetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var swapsDimensions = OrientationSwapsDimensions(request.ExifOrientation);
        var orientedWidth = swapsDimensions ? request.OriginalHeight : request.OriginalWidth;
        var orientedHeight = swapsDimensions ? request.OriginalWidth : request.OriginalHeight;

        var fitScale = Math.Min(
            request.DisplayContext.ViewerUsableWidthPixels / (double)orientedWidth,
            request.DisplayContext.ViewerUsableHeightPixels / (double)orientedHeight);
        fitScale = Math.Min(1.0, fitScale);

        var fitWidth = Math.Max(1, (int)Math.Round(orientedWidth * fitScale, MidpointRounding.AwayFromZero));
        var fitHeight = Math.Max(1, (int)Math.Round(orientedHeight * fitScale, MidpointRounding.AwayFromZero));

        var orientedTargetWidth = Math.Min(
            orientedWidth,
            Math.Max(1, (int)Math.Ceiling(fitWidth * request.QualityMargin)));
        var orientedTargetHeight = Math.Min(
            orientedHeight,
            Math.Max(1, (int)Math.Ceiling(fitHeight * request.QualityMargin)));

        var decodePixelWidth = swapsDimensions ? orientedTargetHeight : orientedTargetWidth;
        var decodePixelHeight = swapsDimensions ? orientedTargetWidth : orientedTargetHeight;

        return new DecodeTarget(
            orientedWidth,
            orientedHeight,
            fitWidth,
            fitHeight,
            orientedTargetWidth,
            orientedTargetHeight,
            decodePixelWidth,
            decodePixelHeight,
            swapsDimensions,
            request.QualityMargin);
    }

    private static bool OrientationSwapsDimensions(int? exifOrientation)
    {
        return exifOrientation is 5 or 6 or 7 or 8;
    }
}
