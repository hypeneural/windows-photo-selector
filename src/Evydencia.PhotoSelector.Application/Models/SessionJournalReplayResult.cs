namespace Evydencia.PhotoSelector.Application.Models;

public sealed class SessionJournalReplayResult
{
    public SessionJournalReplayResult(
        int eventsRead,
        int eventsApplied,
        int recoveredPhotos,
        int missingPhotos)
    {
        EventsRead = eventsRead;
        EventsApplied = eventsApplied;
        RecoveredPhotos = recoveredPhotos;
        MissingPhotos = missingPhotos;
    }

    public int EventsRead { get; }

    public int EventsApplied { get; }

    public int RecoveredPhotos { get; }

    public int MissingPhotos { get; }
}
