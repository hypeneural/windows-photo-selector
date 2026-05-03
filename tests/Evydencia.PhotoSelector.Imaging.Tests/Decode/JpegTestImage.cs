using Windows.Foundation;
using Windows.Graphics.Imaging;

namespace Evydencia.PhotoSelector.Imaging.Tests.Decode;

internal static class JpegTestImage
{
    public static async Task WriteAsync(string filePath, uint width, uint height)
    {
        var pixels = CreatePixels(width, height);

        await WriteAsync(filePath, width, height, pixels, exifOrientation: null);
    }

    public static async Task WriteQuadrantsWithOrientationAsync(
        string filePath,
        uint width,
        uint height,
        ushort exifOrientation)
    {
        var pixels = CreateQuadrantPixels(width, height);

        await WriteAsync(filePath, width, height, pixels, exifOrientation);
    }

    private static async Task WriteAsync(
        string filePath,
        uint width,
        uint height,
        byte[] pixels,
        ushort? exifOrientation)
    {
        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var randomAccessStream = fileStream.AsRandomAccessStream();

        var encoder = await BitmapEncoder
            .CreateAsync(BitmapEncoder.JpegEncoderId, randomAccessStream)
            .AsTask()
            .ConfigureAwait(false);

        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Ignore,
            width,
            height,
            dpiX: 96,
            dpiY: 96,
            pixels);

        if (exifOrientation.HasValue)
        {
            var properties = new BitmapPropertySet
            {
                {
                    "System.Photo.Orientation",
                    new BitmapTypedValue(exifOrientation.Value, PropertyType.UInt16)
                }
            };
            await encoder.BitmapProperties.SetPropertiesAsync(properties).AsTask().ConfigureAwait(false);
        }

        await encoder.FlushAsync().AsTask().ConfigureAwait(false);
    }

    private static byte[] CreatePixels(uint width, uint height)
    {
        var pixels = new byte[checked((int)(width * height * 4))];

        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                var offset = checked((int)((y * width + x) * 4));
                pixels[offset] = (byte)(x % 256);
                pixels[offset + 1] = (byte)(y % 256);
                pixels[offset + 2] = 128;
                pixels[offset + 3] = 255;
            }
        }

        return pixels;
    }

    private static byte[] CreateQuadrantPixels(uint width, uint height)
    {
        var pixels = new byte[checked((int)(width * height * 4))];
        var halfWidth = width / 2;
        var halfHeight = height / 2;

        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                if (x < halfWidth && y < halfHeight)
                {
                    SetPixel(pixels, width, x, y, r: 230, g: 20, b: 20);
                }
                else if (x >= halfWidth && y < halfHeight)
                {
                    SetPixel(pixels, width, x, y, r: 20, g: 230, b: 20);
                }
                else if (x < halfWidth)
                {
                    SetPixel(pixels, width, x, y, r: 20, g: 20, b: 230);
                }
                else
                {
                    SetPixel(pixels, width, x, y, r: 230, g: 230, b: 20);
                }
            }
        }

        return pixels;
    }

    private static void SetPixel(
        byte[] pixels,
        uint width,
        uint x,
        uint y,
        byte r,
        byte g,
        byte b)
    {
        var offset = checked((int)((y * width + x) * 4));
        pixels[offset] = b;
        pixels[offset + 1] = g;
        pixels[offset + 2] = r;
        pixels[offset + 3] = 255;
    }
}
