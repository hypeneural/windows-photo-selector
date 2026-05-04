using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Imaging.Cache;

namespace Evydencia.PhotoSelector.Imaging.Prefetch;

public sealed class PrefetchScheduler : IDisposable
{
    public const int DefaultNextCount = 3;
    public const int DefaultPreviousCount = 2;

    private readonly IPreviewCacheService _previewCacheService;
    private readonly SemaphoreSlim _decodeGate = new(1, 1);
    private readonly object _sync = new();
    private CancellationTokenSource? _currentRunCancellation;
    private bool _disposed;

    public PrefetchScheduler(IPreviewCacheService previewCacheService)
    {
        _previewCacheService = previewCacheService ?? throw new ArgumentNullException(nameof(previewCacheService));
    }

    public void Schedule(
        PhotoSession session,
        PhotoItem currentPhoto,
        DisplayContextSnapshot displayContext,
        int nextCount = DefaultNextCount,
        int previousCount = DefaultPreviousCount)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(currentPhoto);
        ArgumentNullException.ThrowIfNull(displayContext);

        var photos = BuildPrefetchList(session, currentPhoto, nextCount, previousCount);
        if (photos.Count == 0)
        {
            Cancel();
            return;
        }

        CancellationToken token;
        lock (_sync)
        {
            _currentRunCancellation?.Cancel();
            _currentRunCancellation?.Dispose();
            _currentRunCancellation = new CancellationTokenSource();
            token = _currentRunCancellation.Token;
        }

        _ = Task.Run(() => PrefetchAsync(photos, displayContext, token), CancellationToken.None);
    }

    public void Cancel()
    {
        lock (_sync)
        {
            _currentRunCancellation?.Cancel();
            _currentRunCancellation?.Dispose();
            _currentRunCancellation = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Cancel();
        _decodeGate.Dispose();
        _disposed = true;
    }

    public static IReadOnlyList<PhotoItem> BuildPrefetchList(
        PhotoSession session,
        PhotoItem currentPhoto,
        int nextCount = DefaultNextCount,
        int previousCount = DefaultPreviousCount)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(currentPhoto);
        ArgumentOutOfRangeException.ThrowIfNegative(nextCount);
        ArgumentOutOfRangeException.ThrowIfNegative(previousCount);

        var active = session.ActivePhotos();
        var currentIndex = active
            .Select((photo, index) => new { photo, index })
            .FirstOrDefault(item => item.photo.Id == currentPhoto.Id)
            ?.index;

        if (currentIndex is null)
        {
            return [];
        }

        var photos = new List<PhotoItem>(nextCount + previousCount);
        AddRange(active, currentIndex.Value + 1, nextCount, step: 1, photos);
        AddRange(active, currentIndex.Value - 1, previousCount, step: -1, photos);
        return photos;
    }

    private async Task PrefetchAsync(
        IReadOnlyList<PhotoItem> photos,
        DisplayContextSnapshot displayContext,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var photo in photos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _decodeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await _previewCacheService
                        .GetOrDecodePreviewAsync(photo, displayContext, cancellationToken)
                        .ConfigureAwait(false);
                }
                finally
                {
                    _decodeGate.Release();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static void AddRange(
        IReadOnlyList<PhotoItem> active,
        int startIndex,
        int count,
        int step,
        List<PhotoItem> photos)
    {
        for (var offset = 0; offset < count; offset++)
        {
            var index = startIndex + (offset * step);
            if (index < 0 || index >= active.Count)
            {
                break;
            }

            photos.Add(active[index]);
        }
    }
}
