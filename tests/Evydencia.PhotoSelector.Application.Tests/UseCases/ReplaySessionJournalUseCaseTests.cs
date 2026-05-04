using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.Tests.Fakes;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Photos;

namespace Evydencia.PhotoSelector.Application.Tests.UseCases;

[TestClass]
public sealed class ReplaySessionJournalUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsyncWhenDeletedFileExistsInDeletedFolderAddsRecoveredDeletedPhoto()
    {
        var session = SessionFactory.Create("IMG_0002.jpg");
        var originalPath = "C:\\sessao\\IMG_0001.jpg";
        var deletedPath = "C:\\sessao\\_deletadas_evydencia\\IMG_0001.jpg";
        var journalStore = JournalWith(Event(
            SessionJournalEventType.Deleted,
            "IMG_0001.jpg",
            originalPath,
            deletedPath,
            deletedPath));
        var useCase = new ReplaySessionJournalUseCase(
            journalStore,
            new FakeFileExistenceService(deletedPath, session.Photos[0].FullPath));

        var result = await useCase.ExecuteAsync(session);

        var recoveredPhoto = session.Photos.Single(photo => photo.FileName == "IMG_0001.jpg");
        Assert.AreEqual(PhotoStatus.Deleted, recoveredPhoto.Status);
        Assert.AreEqual(2, session.InitialCount);
        Assert.AreEqual(1, session.ActiveCount);
        Assert.AreEqual(1, session.DeletedCount);
        Assert.AreEqual(1, result.EventsRead);
        Assert.AreEqual(1, result.EventsApplied);
        Assert.AreEqual(1, result.RecoveredPhotos);
        Assert.AreEqual(0, result.MissingPhotos);
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenOriginalFileExistsAfterDeletedEventMarksPhotoRestored()
    {
        var session = SessionFactory.Create("IMG_0001.jpg");
        var originalPath = session.Photos[0].FullPath;
        var deletedPath = "C:\\sessao\\_deletadas_evydencia\\IMG_0001.jpg";
        var journalStore = JournalWith(Event(
            SessionJournalEventType.Deleted,
            "IMG_0001.jpg",
            originalPath,
            deletedPath,
            deletedPath));
        var useCase = new ReplaySessionJournalUseCase(
            journalStore,
            new FakeFileExistenceService(originalPath));

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(PhotoStatus.Restored, session.Photos[0].Status);
        Assert.AreEqual(1, session.ActiveCount);
        Assert.AreEqual(0, session.DeletedCount);
        Assert.AreEqual(1, result.EventsApplied);
        Assert.AreEqual(0, result.RecoveredPhotos);
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenRestoredEventPathExistsMarksPhotoRestored()
    {
        var session = SessionFactory.Create("IMG_0001.jpg");
        session.Photos[0].SetStatus(PhotoStatus.Deleted);
        var originalPath = session.Photos[0].FullPath;
        var deletedPath = "C:\\sessao\\_deletadas_evydencia\\IMG_0001.jpg";
        var journalStore = JournalWith(Event(
            SessionJournalEventType.Restored,
            "IMG_0001.jpg",
            originalPath,
            deletedPath,
            originalPath));
        var useCase = new ReplaySessionJournalUseCase(
            journalStore,
            new FakeFileExistenceService(originalPath));

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(PhotoStatus.Restored, session.Photos[0].Status);
        Assert.AreEqual(1, session.ActiveCount);
        Assert.AreEqual(0, session.DeletedCount);
        Assert.AreEqual(1, result.EventsApplied);
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenNoReferencedFileExistsMarksPhotoMissing()
    {
        var session = SessionFactory.Create("IMG_0001.jpg");
        var originalPath = session.Photos[0].FullPath;
        var deletedPath = "C:\\sessao\\_deletadas_evydencia\\IMG_0001.jpg";
        var journalStore = JournalWith(Event(
            SessionJournalEventType.Deleted,
            "IMG_0001.jpg",
            originalPath,
            deletedPath,
            deletedPath));
        var useCase = new ReplaySessionJournalUseCase(
            journalStore,
            new FakeFileExistenceService());

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(PhotoStatus.Missing, session.Photos[0].Status);
        Assert.AreEqual(0, session.ActiveCount);
        Assert.AreEqual(0, session.DeletedCount);
        Assert.AreEqual(1, result.EventsApplied);
        Assert.AreEqual(1, result.MissingPhotos);
    }

    [TestMethod]
    public async Task ExecuteAsyncIgnoresRequestedEventsForFinalState()
    {
        var session = SessionFactory.Create("IMG_0001.jpg");
        var originalPath = session.Photos[0].FullPath;
        var journalStore = JournalWith(Event(
            SessionJournalEventType.DeleteRequested,
            "IMG_0001.jpg",
            originalPath,
            deletedPath: null,
            actualPath: null));
        var useCase = new ReplaySessionJournalUseCase(
            journalStore,
            new FakeFileExistenceService(originalPath));

        var result = await useCase.ExecuteAsync(session);

        Assert.AreEqual(PhotoStatus.Active, session.Photos[0].Status);
        Assert.AreEqual(1, result.EventsRead);
        Assert.AreEqual(0, result.EventsApplied);
    }

    private static FakeSessionJournalStore JournalWith(params SessionJournalEvent[] journalEvents)
    {
        var journalStore = new FakeSessionJournalStore();
        journalStore.Events.AddRange(journalEvents);
        return journalStore;
    }

    private static SessionJournalEvent Event(
        string eventType,
        string fileName,
        string? originalPath,
        string? deletedPath,
        string? actualPath)
    {
        return new SessionJournalEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            eventType,
            Guid.NewGuid(),
            fileName,
            originalPath,
            deletedPath,
            actualPath,
            "Test",
            undoable: eventType == SessionJournalEventType.Deleted,
            errorCode: null,
            errorMessage: null,
            DateTimeOffset.UtcNow);
    }
}
