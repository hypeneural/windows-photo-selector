namespace Evydencia.PhotoSelector.Core.Scanning;

public sealed class PhotoFileCandidate
{
    public PhotoFileCandidate(
        string fileName,
        string fullPath,
        string originalDirectory,
        string extension,
        long sizeBytes,
        DateTimeOffset lastWriteTimeUtc,
        int sortIndex)
    {
        FileName = Required(fileName, nameof(fileName));
        FullPath = Required(fullPath, nameof(fullPath));
        OriginalDirectory = Required(originalDirectory, nameof(originalDirectory));
        Extension = Required(extension, nameof(extension));
        SizeBytes = sizeBytes >= 0 ? sizeBytes : throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        LastWriteTimeUtc = lastWriteTimeUtc;
        SortIndex = sortIndex >= 0 ? sortIndex : throw new ArgumentOutOfRangeException(nameof(sortIndex));
    }

    public string FileName { get; }

    public string FullPath { get; }

    public string OriginalDirectory { get; }

    public string Extension { get; }

    public long SizeBytes { get; }

    public DateTimeOffset LastWriteTimeUtc { get; }

    public int SortIndex { get; }

    private static string Required(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be empty.", parameterName)
            : value;
    }
}
