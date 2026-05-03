using Evydencia.PhotoSelector.Application.Display;

namespace Evydencia.PhotoSelector.Imaging.Sizing;

public sealed class DecodeTargetRequest
{
    public const double DefaultQualityMargin = 4.0 / 3.0;
    public const double MinimumQualityMargin = 1.15;
    public const double MaximumQualityMargin = 1.35;

    public DecodeTargetRequest(
        int originalWidth,
        int originalHeight,
        int? exifOrientation,
        DisplayContextSnapshot displayContext,
        double qualityMargin = DefaultQualityMargin)
    {
        OriginalWidth = Positive(originalWidth, nameof(originalWidth));
        OriginalHeight = Positive(originalHeight, nameof(originalHeight));
        ExifOrientation = exifOrientation;
        DisplayContext = displayContext ?? throw new ArgumentNullException(nameof(displayContext));
        QualityMargin = qualityMargin is >= MinimumQualityMargin and <= MaximumQualityMargin
            ? qualityMargin
            : throw new ArgumentOutOfRangeException(nameof(qualityMargin));
    }

    public int OriginalWidth { get; }

    public int OriginalHeight { get; }

    public int? ExifOrientation { get; }

    public DisplayContextSnapshot DisplayContext { get; }

    public double QualityMargin { get; }

    private static int Positive(int value, string parameterName)
    {
        return value > 0 ? value : throw new ArgumentOutOfRangeException(parameterName);
    }
}
