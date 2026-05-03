using System.Runtime.CompilerServices;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;

namespace Evydencia.PhotoSelector.Storage.Filesystem;

public sealed class FileSystemFolderScanner : IFolderScanner
{
    public async IAsyncEnumerable<PhotoFileCandidate> ScanAsync(
        FolderOpenRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var directory = new DirectoryInfo(request.FolderPath);
        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException($"Session folder was not found: {request.FolderPath}");
        }

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = false,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.Directory
        };

        var sortIndex = 0;
        foreach (var fileInfo in directory.EnumerateFiles("*", options))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var parentDirectory = fileInfo.Directory?.Name;
            if (!FolderScanPolicy.ShouldIncludeCandidate(fileInfo.Name, parentDirectory))
            {
                continue;
            }

            yield return new PhotoFileCandidate(
                fileInfo.Name,
                fileInfo.FullName,
                fileInfo.DirectoryName ?? request.FolderPath,
                fileInfo.Extension,
                fileInfo.Length,
                new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero),
                sortIndex);

            sortIndex++;
            if (sortIndex % 64 == 0)
            {
                await Task.Yield();
            }
        }
    }
}
