namespace Evydencia.PhotoSelector.Application.Models;

public sealed class FolderOpenRequest
{
    public FolderOpenRequest(string folderPath)
    {
        FolderPath = string.IsNullOrWhiteSpace(folderPath)
            ? throw new ArgumentException("Folder path cannot be empty.", nameof(folderPath))
            : folderPath;
    }

    public string FolderPath { get; }
}
