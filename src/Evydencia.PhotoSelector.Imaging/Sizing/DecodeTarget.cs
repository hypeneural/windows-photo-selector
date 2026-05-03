namespace Evydencia.PhotoSelector.Imaging.Sizing;

public sealed class DecodeTarget
{
    public DecodeTarget(
        int orientedImageWidth,
        int orientedImageHeight,
        int fitWidth,
        int fitHeight,
        int orientedTargetWidth,
        int orientedTargetHeight,
        int decodePixelWidth,
        int decodePixelHeight,
        bool orientationSwapsDimensions,
        double qualityMargin)
    {
        OrientedImageWidth = orientedImageWidth;
        OrientedImageHeight = orientedImageHeight;
        FitWidth = fitWidth;
        FitHeight = fitHeight;
        OrientedTargetWidth = orientedTargetWidth;
        OrientedTargetHeight = orientedTargetHeight;
        DecodePixelWidth = decodePixelWidth;
        DecodePixelHeight = decodePixelHeight;
        OrientationSwapsDimensions = orientationSwapsDimensions;
        QualityMargin = qualityMargin;
    }

    public int OrientedImageWidth { get; }

    public int OrientedImageHeight { get; }

    public int FitWidth { get; }

    public int FitHeight { get; }

    public int OrientedTargetWidth { get; }

    public int OrientedTargetHeight { get; }

    public int DecodePixelWidth { get; }

    public int DecodePixelHeight { get; }

    public bool OrientationSwapsDimensions { get; }

    public double QualityMargin { get; }

    public DecodeTargetPrimaryDimension PrimaryDimension =>
        DecodePixelWidth >= DecodePixelHeight
            ? DecodeTargetPrimaryDimension.Width
            : DecodeTargetPrimaryDimension.Height;

    public int PrimaryDecodePixels =>
        PrimaryDimension == DecodeTargetPrimaryDimension.Width ? DecodePixelWidth : DecodePixelHeight;

    public bool UsesOriginalPixels =>
        DecodePixelWidth == (OrientationSwapsDimensions ? OrientedImageHeight : OrientedImageWidth)
        && DecodePixelHeight == (OrientationSwapsDimensions ? OrientedImageWidth : OrientedImageHeight);
}
