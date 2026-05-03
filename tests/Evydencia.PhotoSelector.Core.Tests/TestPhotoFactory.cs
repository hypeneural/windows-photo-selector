using Evydencia.PhotoSelector.Core.Photos;

namespace Evydencia.PhotoSelector.Core.Tests;

internal static class TestPhotoFactory
{
    public static PhotoItem Photo(
        string fileName = "IMG_0001.jpg",
        int sortIndex = 0,
        PhotoStatus status = PhotoStatus.Active)
    {
        return new PhotoItem(
            Guid.NewGuid(),
            fileName,
            $"C:\\sessao\\{fileName}",
            "C:\\sessao",
            ExtensionFrom(fileName),
            1024,
            DateTimeOffset.UtcNow,
            sortIndex,
            status);
    }

    private static string ExtensionFrom(string fileName)
    {
        var dot = fileName.LastIndexOf('.');
        return dot < 0 ? string.Empty : fileName[dot..];
    }
}
