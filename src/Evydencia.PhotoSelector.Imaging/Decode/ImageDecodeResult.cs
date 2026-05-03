namespace Evydencia.PhotoSelector.Imaging.Decode;

public sealed class ImageDecodeResult
{
    private ImageDecodeResult(
        bool isSuccess,
        string filePath,
        int pixelWidth,
        int pixelHeight,
        byte[] pixelData,
        ImageDecodeErrorCode errorCode,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        FilePath = filePath;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        PixelData = pixelData;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string FilePath { get; }

    public int PixelWidth { get; }

    public int PixelHeight { get; }

    public byte[] PixelData { get; }

    public ImageDecodeErrorCode ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static ImageDecodeResult Success(
        string filePath,
        int pixelWidth,
        int pixelHeight,
        byte[] pixelData)
    {
        ArgumentNullException.ThrowIfNull(pixelData);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pixelWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pixelHeight);

        return new ImageDecodeResult(
            true,
            filePath,
            pixelWidth,
            pixelHeight,
            pixelData,
            ImageDecodeErrorCode.None,
            null);
    }

    public static ImageDecodeResult Failure(
        string filePath,
        ImageDecodeErrorCode errorCode,
        string? errorMessage = null)
    {
        if (errorCode == ImageDecodeErrorCode.None)
        {
            throw new ArgumentException("Failure requires an error code.", nameof(errorCode));
        }

        return new ImageDecodeResult(false, filePath, 0, 0, [], errorCode, errorMessage);
    }
}
