using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Imaging.Decode;

namespace Evydencia.PhotoSelector.Imaging.Cache;

public interface IPreviewCacheService
{
    Task<ImageDecodeResult> GetOrDecodePreviewAsync(
        PhotoItem photo,
        DisplayContextSnapshot displayContext,
        CancellationToken cancellationToken = default);

    Task<ImageDecodeResult> GetOrDecodeActualSizeAsync(
        PhotoItem photo,
        CancellationToken cancellationToken = default);
}
