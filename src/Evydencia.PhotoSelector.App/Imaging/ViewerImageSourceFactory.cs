using System.Runtime.InteropServices.WindowsRuntime;
using Evydencia.PhotoSelector.Imaging.Decode;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

namespace Evydencia.PhotoSelector.App.Imaging;

internal sealed class ViewerImageSourceFactory
{
    public async Task<ImageSource?> CreateAsync(
        ImageDecodeResult decodeResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decodeResult);

        if (!decodeResult.IsSuccess)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
            decodeResult.PixelData.AsBuffer(),
            BitmapPixelFormat.Bgra8,
            decodeResult.PixelWidth,
            decodeResult.PixelHeight,
            BitmapAlphaMode.Premultiplied);

        var imageSource = new SoftwareBitmapSource();
        await imageSource.SetBitmapAsync(softwareBitmap).AsTask(cancellationToken);
        return imageSource;
    }
}
