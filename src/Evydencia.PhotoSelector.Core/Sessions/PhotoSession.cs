using Evydencia.PhotoSelector.Core.Photos;

namespace Evydencia.PhotoSelector.Core.Sessions;

public sealed class PhotoSession
{
    private readonly List<PhotoItem> _photos;

    public PhotoSession(
        Guid id,
        string folderPath,
        DateTimeOffset startedAt,
        DateTimeOffset lastOpenedAt,
        IEnumerable<PhotoItem> photos,
        int currentIndex = 0)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Session id cannot be empty.", nameof(id));
        }

        Id = id;
        FolderPath = string.IsNullOrWhiteSpace(folderPath)
            ? throw new ArgumentException("Folder path cannot be empty.", nameof(folderPath))
            : folderPath;
        StartedAt = startedAt;
        LastOpenedAt = lastOpenedAt;
        _photos = photos
            .OrderBy(photo => photo.SortIndex)
            .ThenBy(photo => photo.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        CurrentIndex = currentIndex >= 0 ? currentIndex : throw new ArgumentOutOfRangeException(nameof(currentIndex));
    }

    public Guid Id { get; }

    public string FolderPath { get; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset LastOpenedAt { get; }

    public IReadOnlyList<PhotoItem> Photos => _photos;

    public int InitialCount => _photos.Count;

    public int ActiveCount => _photos.Count(photo => photo.IsAvailableForNavigation);

    public int DeletedCount => _photos.Count(photo => photo.CountsAsDeleted);

    public int CurrentIndex { get; internal set; }

    public IReadOnlyList<PhotoItem> ActivePhotos()
    {
        return _photos
            .Where(photo => photo.IsAvailableForNavigation)
            .OrderBy(photo => photo.SortIndex)
            .ThenBy(photo => photo.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal int IndexOfActivePhoto(Guid photoId)
    {
        var active = ActivePhotos();

        for (var index = 0; index < active.Count; index++)
        {
            if (active[index].Id == photoId)
            {
                return index;
            }
        }

        return -1;
    }
}
