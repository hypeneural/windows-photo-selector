using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 3)]
public class OpenSessionUseCaseBenchmarks
{
    private OpenSessionUseCase _useCase = null!;

    [Params(500, 2000)]
    public int FileCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var candidates = Enumerable.Range(0, FileCount)
            .Select(index => new PhotoFileCandidate(
                $"IMG_{index:D5}.jpg",
                $"C:\\Sessao\\IMG_{index:D5}.jpg",
                "C:\\Sessao",
                ".jpg",
                1024,
                DateTimeOffset.UtcNow,
                index))
            .ToArray();

        _useCase = new OpenSessionUseCase(
            new InMemoryFolderScanner(candidates),
            new PhotoSessionFactory());
    }

    [Benchmark]
    public async Task<int> OpenSessionAsync()
    {
        var result = await _useCase.ExecuteAsync(new OpenSessionCommand("C:\\Sessao"));
        return result.Session.InitialCount;
    }

    private sealed class InMemoryFolderScanner : IFolderScanner
    {
        private readonly IReadOnlyList<PhotoFileCandidate> _candidates;

        public InMemoryFolderScanner(IReadOnlyList<PhotoFileCandidate> candidates)
        {
            _candidates = candidates;
        }

        public async IAsyncEnumerable<PhotoFileCandidate> ScanAsync(
            FolderOpenRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = request;
            await Task.Yield();

            foreach (var candidate in _candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return candidate;
            }
        }
    }
}
