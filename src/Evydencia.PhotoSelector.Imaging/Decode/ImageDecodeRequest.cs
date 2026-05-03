using Evydencia.PhotoSelector.Imaging.Sizing;

namespace Evydencia.PhotoSelector.Imaging.Decode;

public sealed class ImageDecodeRequest
{
    public ImageDecodeRequest(string filePath, DecodeTarget target)
    {
        FilePath = string.IsNullOrWhiteSpace(filePath)
            ? throw new ArgumentException("File path cannot be empty.", nameof(filePath))
            : filePath;
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public string FilePath { get; }

    public DecodeTarget Target { get; }
}
