using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Navigation;

public sealed class NavigationController
{
    private readonly PhotoSession _session;

    public NavigationController(PhotoSession session)
    {
        _session = session;
        var active = _session.ActivePhotos();
        Current = active.Count == 0 ? null : active[Math.Min(_session.CurrentIndex, active.Count - 1)];
        SyncCurrentIndex();
    }

    public PhotoItem? Current { get; private set; }

    public int CurrentActiveIndex => Current is null ? -1 : _session.IndexOfActivePhoto(Current.Id);

    public PhotoItem? MoveFirst()
    {
        var active = _session.ActivePhotos();
        Current = active.Count == 0 ? null : active[0];
        SyncCurrentIndex();
        return Current;
    }

    public PhotoItem? MoveLast()
    {
        var active = _session.ActivePhotos();
        Current = active.Count == 0 ? null : active[^1];
        SyncCurrentIndex();
        return Current;
    }

    public PhotoItem? MoveNext()
    {
        var active = _session.ActivePhotos();
        if (active.Count == 0)
        {
            Current = null;
            SyncCurrentIndex();
            return Current;
        }

        var index = Current is null ? -1 : active.ToList().FindIndex(photo => photo.Id == Current.Id);
        Current = active[Math.Min(index + 1, active.Count - 1)];
        SyncCurrentIndex();
        return Current;
    }

    public PhotoItem? MovePrevious()
    {
        var active = _session.ActivePhotos();
        if (active.Count == 0)
        {
            Current = null;
            SyncCurrentIndex();
            return Current;
        }

        var index = Current is null ? 0 : active.ToList().FindIndex(photo => photo.Id == Current.Id);
        Current = active[Math.Max(index - 1, 0)];
        SyncCurrentIndex();
        return Current;
    }

    public PhotoItem? HideCurrent(PhotoStatus hiddenStatus)
    {
        if (hiddenStatus is PhotoStatus.Active or PhotoStatus.Restored or PhotoStatus.DeleteFailed)
        {
            throw new ArgumentException("Hidden status must remove the photo from active navigation.", nameof(hiddenStatus));
        }

        if (Current is null)
        {
            return null;
        }

        var removedSortIndex = Current.SortIndex;
        Current.SetStatus(hiddenStatus);

        var active = _session.ActivePhotos();
        Current = active
            .Where(photo => photo.SortIndex > removedSortIndex)
            .OrderBy(photo => photo.SortIndex)
            .FirstOrDefault()
            ?? active.OrderByDescending(photo => photo.SortIndex).FirstOrDefault();

        SyncCurrentIndex();
        return Current;
    }

    private void SyncCurrentIndex()
    {
        _session.CurrentIndex = CurrentActiveIndex < 0 ? 0 : CurrentActiveIndex;
    }
}
