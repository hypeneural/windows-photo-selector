namespace Evydencia.PhotoSelector.Imaging.Cache;

public sealed class MemoryImageCacheOptions
{
    public const long MinimumDefaultBytes = 512L * 1024 * 1024;
    public const long MaximumDefaultBytes = 2048L * 1024 * 1024;

    public MemoryImageCacheOptions(long maxBytes)
    {
        MaxBytes = maxBytes > 0 ? maxBytes : throw new ArgumentOutOfRangeException(nameof(maxBytes));
    }

    public long MaxBytes { get; }

    public static MemoryImageCacheOptions CreateDefault()
    {
        var availableBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var targetBytes = availableBytes > 0
            ? availableBytes / 10
            : MinimumDefaultBytes;
        var clampedBytes = Math.Clamp(targetBytes, MinimumDefaultBytes, MaximumDefaultBytes);
        return new MemoryImageCacheOptions(clampedBytes);
    }
}
