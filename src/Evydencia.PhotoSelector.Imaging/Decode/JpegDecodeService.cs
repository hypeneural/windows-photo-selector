using System.Runtime.InteropServices;
using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Imaging.Sizing;
using Windows.Graphics.Imaging;

namespace Evydencia.PhotoSelector.Imaging.Decode;

public sealed class JpegDecodeService
{
    private const int BytesPerPixel = 4;

    private readonly JpegSignatureProbe _signatureProbe;
    private readonly DecodeTargetCalculator _targetCalculator;

    public JpegDecodeService()
        : this(new JpegSignatureProbe(), new DecodeTargetCalculator())
    {
    }

    public JpegDecodeService(JpegSignatureProbe signatureProbe)
        : this(signatureProbe, new DecodeTargetCalculator())
    {
    }

    public JpegDecodeService(JpegSignatureProbe signatureProbe, DecodeTargetCalculator targetCalculator)
    {
        _signatureProbe = signatureProbe ?? throw new ArgumentNullException(nameof(signatureProbe));
        _targetCalculator = targetCalculator ?? throw new ArgumentNullException(nameof(targetCalculator));
    }

    public async Task<ImageDecodeResult> DecodeAsync(
        ImageDecodeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var signatureError = await ProbeAsync(request.FilePath, cancellationToken).ConfigureAwait(false);
            if (signatureError != ImageDecodeErrorCode.None)
            {
                return ImageDecodeResult.Failure(request.FilePath, signatureError);
            }

            return await DecodeWithTargetAsync(request.FilePath, request.Target, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var errorCode = MapException(exception);
            return ImageDecodeResult.Failure(request.FilePath, errorCode, exception.Message);
        }
    }

    public async Task<ImageDecodeResult> DecodeForDisplayAsync(
        string filePath,
        DisplayContextSnapshot displayContext,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        }

        ArgumentNullException.ThrowIfNull(displayContext);

        try
        {
            var signatureError = await ProbeAsync(filePath, cancellationToken).ConfigureAwait(false);
            if (signatureError != ImageDecodeErrorCode.None)
            {
                return ImageDecodeResult.Failure(filePath, signatureError);
            }

            await using var fileStream = JpegSignatureProbe.OpenRead(filePath);
            using var randomAccessStream = fileStream.AsRandomAccessStream();

            var decoder = await BitmapDecoder
                .CreateAsync(BitmapDecoder.JpegDecoderId, randomAccessStream)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            var target = CalculateDisplayTarget(decoder, displayContext);
            var frame = await decoder
                .GetFrameAsync(0)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            return await DecodeFrameAsync(filePath, frame, target, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var errorCode = MapException(exception);
            return ImageDecodeResult.Failure(filePath, errorCode, exception.Message);
        }
    }

    public async Task<ImageDecodeResult> DecodeActualSizeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        }

        try
        {
            var signatureError = await ProbeAsync(filePath, cancellationToken).ConfigureAwait(false);
            if (signatureError != ImageDecodeErrorCode.None)
            {
                return ImageDecodeResult.Failure(filePath, signatureError);
            }

            await using var fileStream = JpegSignatureProbe.OpenRead(filePath);
            using var randomAccessStream = fileStream.AsRandomAccessStream();

            var decoder = await BitmapDecoder
                .CreateAsync(BitmapDecoder.JpegDecoderId, randomAccessStream)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            var target = CalculateActualSizeTarget(decoder);
            var frame = await decoder
                .GetFrameAsync(0)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            return await DecodeFrameAsync(filePath, frame, target, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var errorCode = MapException(exception);
            return ImageDecodeResult.Failure(filePath, errorCode, exception.Message);
        }
    }

    private async Task<ImageDecodeResult> DecodeWithTargetAsync(
        string filePath,
        DecodeTarget target,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var fileStream = JpegSignatureProbe.OpenRead(filePath);
        using var randomAccessStream = fileStream.AsRandomAccessStream();

        var decoder = await BitmapDecoder
            .CreateAsync(BitmapDecoder.JpegDecoderId, randomAccessStream)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);

        var frame = await decoder
            .GetFrameAsync(0)
            .AsTask(cancellationToken)
            .ConfigureAwait(false);

        return await DecodeFrameAsync(filePath, frame, target, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ImageDecodeResult> DecodeFrameAsync(
        string filePath,
        BitmapFrame frame,
        DecodeTarget target,
        CancellationToken cancellationToken)
    {
        var transform = new BitmapTransform
        {
            ScaledWidth = checked((uint)target.DecodePixelWidth),
            ScaledHeight = checked((uint)target.DecodePixelHeight),
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
        var expectedWidth = target.OrientedTargetWidth;
        var expectedHeight = target.OrientedTargetHeight;
        var expectedBytes = checked(expectedWidth * expectedHeight * BytesPerPixel);

        if (pixels.Length != expectedBytes)
        {
            return ImageDecodeResult.Failure(
                filePath,
                ImageDecodeErrorCode.CorruptJpeg,
                $"Decoded pixel length {pixels.Length} did not match expected length {expectedBytes}.");
        }

        return ImageDecodeResult.Success(filePath, expectedWidth, expectedHeight, pixels);
    }

    private DecodeTarget CalculateDisplayTarget(BitmapDecoder decoder, DisplayContextSnapshot displayContext)
    {
        var rawWidth = checked((int)decoder.PixelWidth);
        var rawHeight = checked((int)decoder.PixelHeight);
        var exifOrientationForSizing = DecoderSwapsDimensions(decoder) ? 6 : 1;

        return _targetCalculator.Calculate(new DecodeTargetRequest(
            rawWidth,
            rawHeight,
            exifOrientationForSizing,
            displayContext));
    }

    private static DecodeTarget CalculateActualSizeTarget(BitmapDecoder decoder)
    {
        var rawWidth = checked((int)decoder.PixelWidth);
        var rawHeight = checked((int)decoder.PixelHeight);
        var orientedWidth = checked((int)decoder.OrientedPixelWidth);
        var orientedHeight = checked((int)decoder.OrientedPixelHeight);

        return new DecodeTarget(
            orientedWidth,
            orientedHeight,
            fitWidth: orientedWidth,
            fitHeight: orientedHeight,
            orientedTargetWidth: orientedWidth,
            orientedTargetHeight: orientedHeight,
            decodePixelWidth: rawWidth,
            decodePixelHeight: rawHeight,
            orientationSwapsDimensions: DecoderSwapsDimensions(decoder),
            qualityMargin: 1.0);
    }

    private static bool DecoderSwapsDimensions(BitmapDecoder decoder)
    {
        return decoder.PixelWidth == decoder.OrientedPixelHeight
            && decoder.PixelHeight == decoder.OrientedPixelWidth
            && decoder.PixelWidth != decoder.OrientedPixelWidth;
    }

    private async Task<ImageDecodeErrorCode> ProbeAsync(string filePath, CancellationToken cancellationToken)
    {
        return await _signatureProbe.ProbeAsync(filePath, cancellationToken).ConfigureAwait(false);
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
