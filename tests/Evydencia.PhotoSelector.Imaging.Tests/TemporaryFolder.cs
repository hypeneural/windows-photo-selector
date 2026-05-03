namespace Evydencia.PhotoSelector.Imaging.Tests;

internal sealed class TemporaryFolder : IDisposable
{
    private TemporaryFolder(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public static TemporaryFolder Create()
    {
        var path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "evydencia-imaging-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return new TemporaryFolder(path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
