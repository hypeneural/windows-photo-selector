namespace Evydencia.PhotoSelector.Core.Photos;

public sealed class PhotoItem
{
    public PhotoItem(
        Guid id,
        string fileName,
        string fullPath,
        string originalDirectory,
        string extension,
        long sizeBytes,
        DateTimeOffset lastWriteTimeUtc,
        int sortIndex,
        PhotoStatus status = PhotoStatus.Active)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Photo id cannot be empty.", nameof(id));
        }

        Id = id;
        FileName = Required(fileName, nameof(fileName));
        FullPath = Required(fullPath, nameof(fullPath));
        OriginalDirectory = Required(originalDirectory, nameof(originalDirectory));
        Extension = Required(extension, nameof(extension));
        SizeBytes = sizeBytes >= 0 ? sizeBytes : throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        LastWriteTimeUtc = lastWriteTimeUtc;
        SortIndex = sortIndex >= 0 ? sortIndex : throw new ArgumentOutOfRangeException(nameof(sortIndex));
        Status = status;
    }

    public Guid Id { get; }

    public string FileName { get; private set; }

    public string FullPath { get; private set; }

    public string OriginalDirectory { get; private set; }

    public string Extension { get; private set; }

    public long SizeBytes { get; }

    public DateTimeOffset LastWriteTimeUtc { get; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public int? ExifOrientation { get; private set; }

    public DateTimeOffset? CaptureDate { get; private set; }

    public PhotoStatus Status { get; private set; }

    public string? PreviewCachePath { get; private set; }

    public string? ThumbnailCachePath { get; private set; }

    public int SortIndex { get; }

    public bool IsAvailableForNavigation => Status is PhotoStatus.Active or PhotoStatus.Restored or PhotoStatus.DeleteFailed;

    public bool CountsAsDeleted => Status is PhotoStatus.PendingDelete or PhotoStatus.Deleted;

    public void SetStatus(PhotoStatus status)
    {
        Status = status;
    }

    public void SetFileLocation(
        string fileName,
        string fullPath,
        string originalDirectory,
        string extension)
    {
        FileName = Required(fileName, nameof(fileName));
        FullPath = Required(fullPath, nameof(fullPath));
        OriginalDirectory = Required(originalDirectory, nameof(originalDirectory));
        Extension = Required(extension, nameof(extension));
    }

    public void SetImageMetadata(int width, int height, int? exifOrientation, DateTimeOffset? captureDate)
    {
        Width = width > 0 ? width : throw new ArgumentOutOfRangeException(nameof(width));
        Height = height > 0 ? height : throw new ArgumentOutOfRangeException(nameof(height));
        ExifOrientation = exifOrientation;
        CaptureDate = captureDate;
    }

    public void SetCachePaths(string? previewCachePath, string? thumbnailCachePath)
    {
        PreviewCachePath = string.IsNullOrWhiteSpace(previewCachePath) ? null : previewCachePath;
        ThumbnailCachePath = string.IsNullOrWhiteSpace(thumbnailCachePath) ? null : thumbnailCachePath;
    }

    private static string Required(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be empty.", parameterName)
            : value;
    }
}
