using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Scanning;

namespace Evydencia.PhotoSelector.Core.Sessions;

public sealed class PhotoSessionFactory
{
    public PhotoSession Create(
        string folderPath,
        IEnumerable<PhotoFileCandidate> candidates,
        DateTimeOffset? openedAt = null)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new ArgumentException("Folder path cannot be empty.", nameof(folderPath));
        }

        var timestamp = openedAt ?? DateTimeOffset.UtcNow;
        var photos = candidates
            .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.FullPath, StringComparer.OrdinalIgnoreCase)
            .Select((candidate, index) => new PhotoItem(
                Guid.NewGuid(),
                candidate.FileName,
                candidate.FullPath,
                candidate.OriginalDirectory,
                candidate.Extension,
                candidate.SizeBytes,
                candidate.LastWriteTimeUtc,
                index))
            .ToList();

        return new PhotoSession(Guid.NewGuid(), folderPath, timestamp, timestamp, photos);
    }
}
