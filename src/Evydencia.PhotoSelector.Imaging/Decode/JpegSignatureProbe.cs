namespace Evydencia.PhotoSelector.Imaging.Decode;

public sealed class JpegSignatureProbe
{
    private const byte JpegStartOfImageFirstByte = 0xFF;
    private const byte JpegStartOfImageSecondByte = 0xD8;

    public async Task<ImageDecodeErrorCode> ProbeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ImageDecodeErrorCode.FileMissing;
        }

        try
        {
            await using var stream = OpenRead(filePath);
            var header = new byte[2];
            var bytesRead = await stream.ReadAsync(header, cancellationToken).ConfigureAwait(false);

            return bytesRead == 2
                && header[0] == JpegStartOfImageFirstByte
                && header[1] == JpegStartOfImageSecondByte
                    ? ImageDecodeErrorCode.None
                    : ImageDecodeErrorCode.UnsupportedOrNotJpeg;
        }
        catch (OperationCanceledException)
        {
            return ImageDecodeErrorCode.DecodeCanceled;
        }
        catch (FileNotFoundException)
        {
            return ImageDecodeErrorCode.FileMissing;
        }
        catch (DirectoryNotFoundException)
        {
            return ImageDecodeErrorCode.FileMissing;
        }
        catch (UnauthorizedAccessException)
        {
            return ImageDecodeErrorCode.AccessDenied;
        }
        catch (IOException)
        {
            return ImageDecodeErrorCode.FileLocked;
        }
    }

    internal static FileStream OpenRead(string filePath)
    {
        return new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
    }
}
