namespace Evydencia.PhotoSelector.Application.Display;

public sealed class DisplayContextSnapshot
{
    public DisplayContextSnapshot(
        string displayId,
        double effectiveWidthDips,
        double effectiveHeightDips,
        double viewerUsableWidthDips,
        double viewerUsableHeightDips,
        double rasterizationScale,
        bool isFullscreen,
        DisplayRole role = DisplayRole.Unknown)
    {
        DisplayId = string.IsNullOrWhiteSpace(displayId) ? "unknown" : displayId;
        EffectiveWidthDips = Positive(effectiveWidthDips, nameof(effectiveWidthDips));
        EffectiveHeightDips = Positive(effectiveHeightDips, nameof(effectiveHeightDips));
        ViewerUsableWidthDips = Positive(viewerUsableWidthDips, nameof(viewerUsableWidthDips));
        ViewerUsableHeightDips = Positive(viewerUsableHeightDips, nameof(viewerUsableHeightDips));
        RasterizationScale = Positive(rasterizationScale, nameof(rasterizationScale));
        IsFullscreen = isFullscreen;
        Role = role;
    }

    public string DisplayId { get; }

    public double EffectiveWidthDips { get; }

    public double EffectiveHeightDips { get; }

    public double ViewerUsableWidthDips { get; }

    public double ViewerUsableHeightDips { get; }

    public double RasterizationScale { get; }

    public bool IsFullscreen { get; }

    public DisplayRole Role { get; }

    public int EffectiveWidthPixels => ToPhysicalPixels(EffectiveWidthDips);

    public int EffectiveHeightPixels => ToPhysicalPixels(EffectiveHeightDips);

    public int ViewerUsableWidthPixels => ToPhysicalPixels(ViewerUsableWidthDips);

    public int ViewerUsableHeightPixels => ToPhysicalPixels(ViewerUsableHeightDips);

    private int ToPhysicalPixels(double dips)
    {
        return Math.Max(1, (int)Math.Round(dips * RasterizationScale, MidpointRounding.AwayFromZero));
    }

    private static double Positive(double value, string parameterName)
    {
        return value > 0 ? value : throw new ArgumentOutOfRangeException(parameterName);
    }
}
