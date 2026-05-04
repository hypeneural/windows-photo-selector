using Evydencia.PhotoSelector.Imaging.Decode;

namespace Evydencia.PhotoSelector.Imaging.Cache;

public sealed class MemoryImageCache
{
    private readonly Dictionary<ImageCacheKey, LinkedListNode<MemoryImageCacheEntry>> _entries = [];
    private readonly LinkedList<MemoryImageCacheEntry> _lru = [];
    private readonly object _sync = new();
    private long _currentBytes;

    public MemoryImageCache()
        : this(MemoryImageCacheOptions.CreateDefault())
    {
    }

    public MemoryImageCache(MemoryImageCacheOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public MemoryImageCacheOptions Options { get; }

    public int Count
    {
        get
        {
            lock (_sync)
            {
                return _entries.Count;
            }
        }
    }

    public long CurrentBytes
    {
        get
        {
            lock (_sync)
            {
                return _currentBytes;
            }
        }
    }

    public bool TryGet(ImageCacheKey key, out ImageDecodeResult result)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_sync)
        {
            if (!_entries.TryGetValue(key, out var node))
            {
                result = ImageDecodeResult.Failure(
                    key.NormalizedPath,
                    ImageDecodeErrorCode.FileMissing,
                    "Cache miss.");
                return false;
            }

            _lru.Remove(node);
            _lru.AddFirst(node);
            result = node.Value.Result;
            return true;
        }
    }

    public bool Set(ImageCacheKey key, ImageDecodeResult result)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsSuccess)
        {
            return false;
        }

        var sizeBytes = result.PixelData.LongLength;
        if (sizeBytes <= 0 || sizeBytes > Options.MaxBytes)
        {
            return false;
        }

        lock (_sync)
        {
            RemoveCore(key);

            var entry = new MemoryImageCacheEntry(key, result, sizeBytes);
            var node = new LinkedListNode<MemoryImageCacheEntry>(entry);
            _lru.AddFirst(node);
            _entries[key] = node;
            _currentBytes += sizeBytes;
            EvictUntilWithinLimit();
            return true;
        }
    }

    public void Remove(ImageCacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_sync)
        {
            RemoveCore(key);
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _entries.Clear();
            _lru.Clear();
            _currentBytes = 0;
        }
    }

    private void EvictUntilWithinLimit()
    {
        while (_currentBytes > Options.MaxBytes && _lru.Last is not null)
        {
            RemoveCore(_lru.Last.Value.Key);
        }
    }

    private void RemoveCore(ImageCacheKey key)
    {
        if (!_entries.Remove(key, out var node))
        {
            return;
        }

        _lru.Remove(node);
        _currentBytes -= node.Value.SizeBytes;
    }

    private sealed record MemoryImageCacheEntry(
        ImageCacheKey Key,
        ImageDecodeResult Result,
        long SizeBytes);
}
