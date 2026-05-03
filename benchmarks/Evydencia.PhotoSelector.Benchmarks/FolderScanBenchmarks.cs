using BenchmarkDotNet.Attributes;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Storage.Filesystem;

namespace Evydencia.PhotoSelector.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 3)]
public class FolderScanBenchmarks
{
    private readonly FileSystemFolderScanner _scanner = new();
    private string _folderPath = string.Empty;

    [Params(500, 2000)]
    public int FileCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _folderPath = Path.Combine(Path.GetTempPath(), $"evydencia-scan-benchmark-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_folderPath);
        Directory.CreateDirectory(Path.Combine(_folderPath, "_deletadas_evydencia"));

        for (var index = 0; index < FileCount; index++)
        {
            File.WriteAllBytes(
                Path.Combine(_folderPath, $"IMG_{index:D5}.jpg"),
                [0xFF, 0xD8, 0xFF, 0xD9]);
        }

        File.WriteAllText(Path.Combine(_folderPath, "ignore.png"), "not jpeg");
        File.WriteAllBytes(
            Path.Combine(_folderPath, "_deletadas_evydencia", "IMG_deleted.jpg"),
            [0xFF, 0xD8, 0xFF, 0xD9]);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (Directory.Exists(_folderPath))
        {
            Directory.Delete(_folderPath, recursive: true);
        }
    }

    [Benchmark]
    public async Task<int> ScanJpegsAsync()
    {
        var count = 0;
        await foreach (var _ in _scanner.ScanAsync(new FolderOpenRequest(_folderPath)))
        {
            count++;
        }

        return count;
    }
}
