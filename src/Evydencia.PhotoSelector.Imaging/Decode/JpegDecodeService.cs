using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;

namespace Evydencia.PhotoSelector.Imaging.Decode;

public sealed class JpegDecodeService
{
    private const int BytesPerPixel = 4;

    private readonly JpegSignatureProbe _signatureProbe;

    public JpegDecodeService()
        : this(new JpegSignatureProbe())
    {
    }

    public JpegDecodeService(JpegSignatureProbe signatureProbe)
    {
        _signatureProbe = signatureProbe ?? throw new ArgumentNullException(nameof(signatureProbe));
    }

    public async Task<ImageDecodeResult> DecodeAsync(
        ImageDecodeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var signatureError = await _signatureProbe
            .ProbeAsync(request.FilePath, cancellationToken)
            .ConfigureAwait(false);

        if (signatureError != ImageDecodeErrorCode.None)
        {
            return ImageDecodeResult.Failure(request.FilePath, signatureError);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var fileStream = JpegSignatureProbe.OpenRead(request.FilePath);
            using var randomAccessStream = fileStream.AsRandomAccessStream();

            var decoder = await BitmapDecoder
                .CreateAsync(BitmapDecoder.JpegDecoderId, randomAccessStream)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            var frame = await decoder
                .GetFrameAsync(0)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            var transform = new BitmapTransform
            {
                ScaledWidth = checked((uint)request.Target.DecodePixelWidth),
                ScaledHeight = checked((uint)request.Target.DecodePixelHeight),
                InterpolationMode = BitmapInterpolationMode.Fant
            };

            var pixelProvider = await frame
                .GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    transform,
                    ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.DoNotColorManage)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            var pixels = pixelProvider.DetachPixelData();
            var expectedWidth = request.Target.OrientedTargetWidth;
            var expectedHeight = request.Target.OrientedTargetHeight;
            var expectedBytes = checked(expectedWidth * expectedHeight * BytesPerPixel);

            if (pixels.Length != expectedBytes)
            {
                return ImageDecodeResult.Failure(
                    request.FilePath,
                    ImageDecodeErrorCode.CorruptJpeg,
                    $"Decoded pixel length {pixels.Length} did not match expected length {expectedBytes}.");
            }

            return ImageDecodeResult.Success(request.FilePath, expectedWidth, expectedHeight, pixels);
        }
        catch (Exception exception)
        {
            var errorCode = MapException(exception);
            return ImageDecodeResult.Failure(request.FilePath, errorCode, exception.Message);
        }
    }

    private static ImageDecodeErrorCode MapException(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException => ImageDecodeErrorCode.DecodeCanceled,
            FileNotFoundException => ImageDecodeErrorCode.FileMissing,
            DirectoryNotFoundException => ImageDecodeErrorCode.FileMissing,
            UnauthorizedAccessException => ImageDecodeErrorCode.AccessDenied,
            IOException => ImageDecodeErrorCode.FileLocked,
            COMException => ImageDecodeErrorCode.CorruptJpeg,
            _ => ImageDecodeErrorCode.Unknown
        };
    }
}
