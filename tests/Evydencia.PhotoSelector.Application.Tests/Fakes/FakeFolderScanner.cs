using System.Runtime.CompilerServices;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;

namespace Evydencia.PhotoSelector.Application.Tests.Fakes;

internal sealed class FakeFolderScanner : IFolderScanner
{
    private readonly IReadOnlyList<PhotoFileCandidate> _candidates;
    private readonly Exception? _exception;

    public FakeFolderScanner(IEnumerable<PhotoFileCandidate> candidates, Exception? exception = null)
    {
        _candidates = candidates.ToList();
        _exception = exception;
    }

    public int ScanCount { get; private set; }

    public FolderOpenRequest? LastRequest { get; private set; }

    public async IAsyncEnumerable<PhotoFileCandidate> ScanAsync(
        FolderOpenRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        ScanCount++;
        LastRequest = request;
        if (_exception is not null)
        {
            throw _exception;
        }

        foreach (var candidate in _candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return candidate;
        }
    }
}
