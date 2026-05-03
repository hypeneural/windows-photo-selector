using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Storage.Filesystem;

namespace Evydencia.PhotoSelector.Storage.Tests.Filesystem;

[TestClass]
public sealed class FileSystemFolderScannerTests
{
    [TestMethod]
    public async Task ScanAsyncReturnsOnlyTopLevelJpegs()
    {
        using var folder = TemporaryFolder.Create();
        folder.WriteFile("IMG_0001.jpg");
        folder.WriteFile("IMG_0002.JPEG");
        folder.WriteFile("IMG_0003.png");
        folder.WriteFile("nested\\IMG_0004.jpg");
        folder.WriteFile($"{FolderScanPolicy.DeletedFolderName}\\IMG_0005.jpg");

        var scanner = new FileSystemFolderScanner();

        var candidates = await ScanAsync(scanner, folder.Path);

        CollectionAssert.AreEqual(
            new List<string> { "IMG_0001.jpg", "IMG_0002.JPEG" },
            candidates.Select(candidate => candidate.FileName).OrderBy(name => name).ToList());
    }

    [TestMethod]
    public async Task ScanAsyncMapsCheapFileInfo()
    {
        using var folder = TemporaryFolder.Create();
        var path = folder.WriteFile("IMG_0001.jpg", "jpeg bytes");

        var scanner = new FileSystemFolderScanner();

        var candidate = (await ScanAsync(scanner, folder.Path)).Single();

        Assert.AreEqual("IMG_0001.jpg", candidate.FileName);
        Assert.AreEqual(path, candidate.FullPath);
        Assert.AreEqual(folder.Path, candidate.OriginalDirectory);
        Assert.AreEqual(".jpg", candidate.Extension);
        Assert.IsGreaterThan(0L, candidate.SizeBytes);
        Assert.AreEqual(0, candidate.SortIndex);
    }

    [TestMethod]
    public async Task ScanAsyncThrowsForMissingFolder()
    {
        var scanner = new FileSystemFolderScanner();
        var missing = Path.Combine(Path.GetTempPath(), $"evydencia-missing-{Guid.NewGuid():N}");

        await Assert.ThrowsExactlyAsync<DirectoryNotFoundException>(async () =>
        {
            await foreach (var _ in scanner.ScanAsync(new FolderOpenRequest(missing)))
            {
            }
        });
    }

    private static async Task<List<PhotoFileCandidate>> ScanAsync(FileSystemFolderScanner scanner, string folderPath)
    {
        var candidates = new List<PhotoFileCandidate>();
        await foreach (var candidate in scanner.ScanAsync(new FolderOpenRequest(folderPath)))
        {
            candidates.Add(candidate);
        }

        return candidates;
    }
}
