using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Core.Photos;

namespace Evydencia.PhotoSelector.Imaging.Cache;

public sealed record ImageCacheKey(
    string NormalizedPath,
    long SizeBytes,
    DateTimeOffset LastWriteTimeUtc,
    ImageCacheMode Mode,
    int TargetWidthPixels,
    int TargetHeightPixels,
    int AlgorithmVersion)
{
    public const int CurrentAlgorithmVersion = 1;

    public static ImageCacheKey ForPreview(PhotoItem photo, DisplayContextSnapshot displayContext)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ArgumentNullException.ThrowIfNull(displayContext);

        return new ImageCacheKey(
            NormalizePath(photo.FullPath),
            photo.SizeBytes,
            photo.LastWriteTimeUtc,
            ImageCacheMode.Preview,
            displayContext.ViewerUsableWidthPixels,
            displayContext.ViewerUsableHeightPixels,
            CurrentAlgorithmVersion);
    }

    public static ImageCacheKey ForActualSize(PhotoItem photo)
    {
        ArgumentNullException.ThrowIfNull(photo);

        return new ImageCacheKey(
            NormalizePath(photo.FullPath),
            photo.SizeBytes,
            photo.LastWriteTimeUtc,
            ImageCacheMode.ActualSize,
            0,
            0,
            CurrentAlgorithmVersion);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }
}
