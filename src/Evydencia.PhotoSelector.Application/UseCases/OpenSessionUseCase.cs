using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Navigation;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class OpenSessionUseCase
{
    private readonly IFolderScanner _folderScanner;
    private readonly PhotoSessionFactory _sessionFactory;

    public OpenSessionUseCase(IFolderScanner folderScanner, PhotoSessionFactory sessionFactory)
    {
        _folderScanner = folderScanner;
        _sessionFactory = sessionFactory;
    }

    public async Task<OpenSessionResult> ExecuteAsync(
        OpenSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var candidates = new List<PhotoFileCandidate>();
        await foreach (var candidate in _folderScanner.ScanAsync(
            new FolderOpenRequest(command.FolderPath),
            cancellationToken))
        {
            candidates.Add(candidate);
        }

        var session = _sessionFactory.Create(command.FolderPath, candidates);
        var controller = new NavigationController(session);
        return new OpenSessionResult(session, controller.Current);
    }
}
