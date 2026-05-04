using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Storage.Journal;

public sealed class JsonlSessionJournalStore : ISessionJournalStore, IDisposable
{
    public const string JournalFileName = "evydencia-session-journal.jsonl";

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly SemaphoreSlim _appendGate = new(1, 1);

    public string GetJournalPath(PhotoSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return Path.Combine(
            session.FolderPath,
            FolderScanPolicy.DeletedFolderName,
            JournalFileName);
    }

    public async Task AppendAsync(
        PhotoSession session,
        SessionJournalEvent journalEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(journalEvent);
        cancellationToken.ThrowIfCancellationRequested();

        var journalPath = GetJournalPath(session);
        var journalDirectory = Path.GetDirectoryName(journalPath)
            ?? throw new InvalidOperationException("Journal directory could not be resolved.");
        Directory.CreateDirectory(journalDirectory);

        var json = JsonSerializer.Serialize(journalEvent, SerializerOptions);
        var line = json + Environment.NewLine;

        await _appendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await File.AppendAllTextAsync(
                    journalPath,
                    line,
                    Utf8NoBom,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _appendGate.Release();
        }
    }

    public async IAsyncEnumerable<SessionJournalEvent> ReadEventsAsync(
        PhotoSession session,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        var journalPath = GetJournalPath(session);
        if (!File.Exists(journalPath))
        {
            yield break;
        }

        await foreach (var line in File.ReadLinesAsync(journalPath, Utf8NoBom, cancellationToken)
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            SessionJournalEvent? journalEvent;
            try
            {
                journalEvent = JsonSerializer.Deserialize<SessionJournalEvent>(line, SerializerOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (journalEvent is not null)
            {
                yield return journalEvent;
            }
        }
    }

    public void Dispose()
    {
        _appendGate.Dispose();
    }
}
