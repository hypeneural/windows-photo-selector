using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;

namespace Evydencia.PhotoSelector.Application.Abstractions;

public interface IFolderScanner
{
    IAsyncEnumerable<PhotoFileCandidate> ScanAsync(
        FolderOpenRequest request,
        CancellationToken cancellationToken = default);
}
