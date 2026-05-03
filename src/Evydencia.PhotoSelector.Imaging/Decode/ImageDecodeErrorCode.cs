namespace Evydencia.PhotoSelector.Imaging.Decode;

public enum ImageDecodeErrorCode
{
    None = 0,
    FileMissing,
    AccessDenied,
    UnsupportedOrNotJpeg,
    CorruptJpeg,
    DecodeCanceled,
    FileLocked,
    Unknown
}
