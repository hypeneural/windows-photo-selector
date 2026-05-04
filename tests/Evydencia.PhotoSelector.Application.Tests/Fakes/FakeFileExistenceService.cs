using Evydencia.PhotoSelector.Application.Abstractions;

namespace Evydencia.PhotoSelector.Application.Tests.Fakes;

internal sealed class FakeFileExistenceService : IFileExistenceService
{
    private readonly HashSet<string> _existingPaths = new(StringComparer.OrdinalIgnoreCase);

    public FakeFileExistenceService(params string[] existingPaths)
    {
        foreach (var path in existingPaths)
        {
            _existingPaths.Add(path);
        }
    }

    public bool Exists(string? path)
    {
        return !string.IsNullOrWhiteSpace(path) && _existingPaths.Contains(path);
    }
}
