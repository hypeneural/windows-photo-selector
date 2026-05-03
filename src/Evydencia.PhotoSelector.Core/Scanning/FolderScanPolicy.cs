namespace Evydencia.PhotoSelector.Core.Scanning;

public static class FolderScanPolicy
{
    public const string DeletedFolderName = "_deletadas_evydencia";

    public static bool IsAcceptedPhotoFile(string fileNameOrPath)
    {
        if (string.IsNullOrWhiteSpace(fileNameOrPath))
        {
            return false;
        }

        var fileName = LastSegment(fileNameOrPath);
        var extensionStart = fileName.LastIndexOf('.');
        if (extensionStart < 0)
        {
            return false;
        }

        var extension = fileName[extensionStart..];
        return extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ShouldIgnoreDirectoryName(string directoryName)
    {
        return directoryName.Equals(DeletedFolderName, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ShouldIncludeCandidate(string fileNameOrPath, string? parentDirectoryName)
    {
        return IsAcceptedPhotoFile(fileNameOrPath)
            && (parentDirectoryName is null || !ShouldIgnoreDirectoryName(parentDirectoryName));
    }

    private static string LastSegment(string value)
    {
        var lastSlash = value.LastIndexOfAny(['\\', '/']);
        return lastSlash < 0 ? value : value[(lastSlash + 1)..];
    }
}
