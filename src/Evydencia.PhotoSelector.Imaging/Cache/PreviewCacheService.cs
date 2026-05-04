using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Imaging.Decode;

namespace Evydencia.PhotoSelector.Imaging.Cache;

public sealed class PreviewCacheService : IPreviewCacheService
{
    private readonly JpegDecodeService _decodeService;
    private readonly MemoryImageCache _memoryCache;

    public PreviewCacheService(JpegDecodeService decodeService, MemoryImageCache memoryCache)
    {
        _decodeService = decodeService ?? throw new ArgumentNullException(nameof(decodeService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<ImageDecodeResult> GetOrDecodePreviewAsync(
        PhotoItem photo,
        DisplayContextSnapshot displayContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ArgumentNullException.ThrowIfNull(displayContext);

        var key = ImageCacheKey.ForPreview(photo, displayContext);
        if (_memoryCache.TryGet(key, out var cachedResult))
        {
            return cachedResult;
        }

        var decodeResult = await _decodeService
            .DecodeForDisplayAsync(photo.FullPath, displayContext, cancellationToken)
            .ConfigureAwait(false);
        _memoryCache.Set(key, decodeResult);
        return decodeResult;
    }

    public async Task<ImageDecodeResult> GetOrDecodeActualSizeAsync(
        PhotoItem photo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);

        var key = ImageCacheKey.ForActualSize(photo);
        if (_memoryCache.TryGet(key, out var cachedResult))
        {
            return cachedResult;
        }

        var decodeResult = await _decodeService
            .DecodeActualSizeAsync(photo.FullPath, cancellationToken)
            .ConfigureAwait(false);
        _memoryCache.Set(key, decodeResult);
        return decodeResult;
    }
}
