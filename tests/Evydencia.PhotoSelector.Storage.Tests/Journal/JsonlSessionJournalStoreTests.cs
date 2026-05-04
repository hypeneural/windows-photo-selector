using System.Text.Json;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Storage.Journal;

namespace Evydencia.PhotoSelector.Storage.Tests.Journal;

[TestClass]
public sealed class JsonlSessionJournalStoreTests
{
    [TestMethod]
    public async Task AppendAsyncCreatesDeletedFolderAndJournalFile()
    {
        using var folder = TemporaryFolder.Create();
        var session = CreateSession(folder.Path);
        var store = new JsonlSessionJournalStore();

        await store.AppendAsync(session, SessionJournalEvent.DeleteRequested(session, session.Photos[0]));

        var journalPath = store.GetJournalPath(session);
        Assert.IsTrue(File.Exists(journalPath));
        Assert.AreEqual(
            Path.Combine(folder.Path, FolderScanPolicy.DeletedFolderName, JsonlSessionJournalStore.JournalFileName),
            journalPath);
    }

    [TestMethod]
    public async Task AppendAsyncAppendsOneJsonObjectPerLine()
    {
        using var folder = TemporaryFolder.Create();
        var session = CreateSession(folder.Path);
        var store = new JsonlSessionJournalStore();

        await store.AppendAsync(session, SessionJournalEvent.DeleteRequested(session, session.Photos[0]));
        await store.AppendAsync(
            session,
            SessionJournalEvent.Deleted(
                session,
                session.Photos[0],
                FileMoveResult.Success(
                    session.Photos[0].FullPath,
                    Path.Combine(folder.Path, FolderScanPolicy.DeletedFolderName, "IMG_0001.jpg"),
                    Path.Combine(folder.Path, FolderScanPolicy.DeletedFolderName, "IMG_0001.jpg"),
                    collisionResolved: false,
                    DateTimeOffset.UtcNow)));

        var lines = await File.ReadAllLinesAsync(store.GetJournalPath(session));

        Assert.HasCount(2, lines);
        AssertEventType(lines[0], SessionJournalEventType.DeleteRequested);
        AssertEventType(lines[1], SessionJournalEventType.Deleted);
    }

    [TestMethod]
    public async Task ReadEventsAsyncReturnsJournalEventsInAppendOrder()
    {
        using var folder = TemporaryFolder.Create();
        var session = CreateSession(folder.Path);
        var store = new JsonlSessionJournalStore();
        await store.AppendAsync(session, SessionJournalEvent.DeleteRequested(session, session.Photos[0]));
        await store.AppendAsync(
            session,
            SessionJournalEvent.Deleted(
                session,
                session.Photos[0],
                FileMoveResult.Success(
                    session.Photos[0].FullPath,
                    Path.Combine(folder.Path, FolderScanPolicy.DeletedFolderName, "IMG_0001.jpg"),
                    Path.Combine(folder.Path, FolderScanPolicy.DeletedFolderName, "IMG_0001.jpg"),
                    collisionResolved: false,
                    DateTimeOffset.UtcNow)));

        var events = await ReadAllEventsAsync(store, session);

        Assert.HasCount(2, events);
        Assert.AreEqual(SessionJournalEventType.DeleteRequested, events[0].EventType);
        Assert.AreEqual(SessionJournalEventType.Deleted, events[1].EventType);
        Assert.AreEqual("IMG_0001.jpg", events[1].FileName);
    }

    [TestMethod]
    public async Task ReadEventsAsyncWhenJournalDoesNotExistReturnsNoEvents()
    {
        using var folder = TemporaryFolder.Create();
        var session = CreateSession(folder.Path);
        var store = new JsonlSessionJournalStore();

        var events = await ReadAllEventsAsync(store, session);

        Assert.HasCount(0, events);
    }

    private static PhotoSession CreateSession(string folderPath)
    {
        var photo = new PhotoItem(
            Guid.NewGuid(),
            "IMG_0001.jpg",
            Path.Combine(folderPath, "IMG_0001.jpg"),
            folderPath,
            ".jpg",
            1024,
            DateTimeOffset.UtcNow,
            sortIndex: 0);

        return new PhotoSession(
            Guid.NewGuid(),
            folderPath,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [photo]);
    }

    private static void AssertEventType(string json, string expectedEventType)
    {
        using var document = JsonDocument.Parse(json);
        var eventType = document.RootElement.GetProperty("eventType").GetString();
        Assert.AreEqual(expectedEventType, eventType);
    }

    private static async Task<List<SessionJournalEvent>> ReadAllEventsAsync(
        JsonlSessionJournalStore store,
        PhotoSession session)
    {
        var events = new List<SessionJournalEvent>();
        await foreach (var journalEvent in store.ReadEventsAsync(session))
        {
            events.Add(journalEvent);
        }

        return events;
    }
}
