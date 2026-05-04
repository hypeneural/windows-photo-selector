using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Abstractions;

public interface ISessionJournalStore
{
    string GetJournalPath(PhotoSession session);

    Task AppendAsync(
        PhotoSession session,
        SessionJournalEvent journalEvent,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<SessionJournalEvent> ReadEventsAsync(
        PhotoSession session,
        CancellationToken cancellationToken = default);
}
