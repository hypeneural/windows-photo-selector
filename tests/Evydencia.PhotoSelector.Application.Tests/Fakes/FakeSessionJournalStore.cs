using System.Runtime.CompilerServices;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Tests.Fakes;

internal sealed class FakeSessionJournalStore : ISessionJournalStore
{
    public List<SessionJournalEvent> Events { get; } = [];

    public string GetJournalPath(PhotoSession session)
    {
        return Path.Combine(session.FolderPath, "_deletadas_evydencia", "evydencia-session-journal.jsonl");
    }

    public Task AppendAsync(
        PhotoSession session,
        SessionJournalEvent journalEvent,
        CancellationToken cancellationToken = default)
    {
        Events.Add(journalEvent);
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<SessionJournalEvent> ReadEventsAsync(
        PhotoSession session,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var journalEvent in Events)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return journalEvent;
            await Task.Yield();
        }
    }
}
