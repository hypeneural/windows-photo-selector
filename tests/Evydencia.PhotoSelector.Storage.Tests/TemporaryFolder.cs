namespace Evydencia.PhotoSelector.Storage.Tests;

internal sealed class TemporaryFolder : IDisposable
{
    private TemporaryFolder(string path)
    {
        Path = path;
        Directory.CreateDirectory(path);
    }

    public string Path { get; }

    public static TemporaryFolder Create()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"evydencia-photo-selector-{Guid.NewGuid():N}");
        return new TemporaryFolder(path);
    }

    public string WriteFile(string relativePath, string content = "content")
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath);
        var directory = System.IO.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
