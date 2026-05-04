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
}
