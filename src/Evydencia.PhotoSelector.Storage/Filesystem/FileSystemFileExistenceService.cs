using Evydencia.PhotoSelector.Application.Abstractions;

namespace Evydencia.PhotoSelector.Storage.Filesystem;

public sealed class FileSystemFileExistenceService : IFileExistenceService
{
    public bool Exists(string? path)
    {
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }
}
