using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;
using Evydencia.PhotoSelector.Storage.Filesystem;
using Evydencia.PhotoSelector.Storage.Journal;

namespace Evydencia.PhotoSelector.IntegrationTests.Deletion;

[TestClass]
public sealed class DeleteCurrentPhotoUseCaseIntegrationTests
{
    [TestMethod]
    public async Task ExecuteAsyncWithFileMoveServiceMovesFileAndMarksDeleted()
    {
        var folderPath = CreateTemporaryFolder();
        try
        {
            var firstPath = WriteFile(folderPath, "IMG_0001.jpg");
            WriteFile(folderPath, "IMG_0002.jpg");
            var session = CreateSession(folderPath, "IMG_0001.jpg", "IMG_0002.jpg");
            var journalStore = new JsonlSessionJournalStore();
            var useCase = new DeleteCurrentPhotoUseCase(
                new DeleteManager(),
                new FileMoveService(),
                new UndoManager(),
                journalStore);

            var result = await useCase.ExecuteAsync(session);

            Assert.AreEqual(DeleteCurrentPhotoStatus.Deleted, result.Status);
            Assert.AreEqual(PhotoStatus.Deleted, result.DeletedPhoto?.Status);
            Assert.AreEqual("IMG_0002.jpg", result.CurrentPhoto?.FileName);
            Assert.AreEqual(1, result.ActiveCount);
            Assert.AreEqual(1, result.DeletedCount);
            Assert.IsFalse(File.Exists(firstPath));
            Assert.IsNotNull(result.FileMoveResult?.ActualDestinationPath);
            Assert.IsTrue(File.Exists(result.FileMoveResult.ActualDestinationPath));
            Assert.AreEqual(
                Path.Combine(folderPath, FolderScanPolicy.DeletedFolderName, "IMG_0001.jpg"),
                result.FileMoveResult.ActualDestinationPath);
            var journalLines = await File.ReadAllLinesAsync(journalStore.GetJournalPath(session));
            Assert.HasCount(2, journalLines);
            StringAssert.Contains(journalLines[0], SessionJournalEventType.DeleteRequested);
            StringAssert.Contains(journalLines[1], SessionJournalEventType.Deleted);
        }
        finally
        {
            DeleteTemporaryFolder(folderPath);
        }
    }

    [TestMethod]
    public async Task DeleteThenUndoWithFileMoveServiceRestoresFileAndMarksRestored()
    {
        var folderPath = CreateTemporaryFolder();
        try
        {
            var firstPath = WriteFile(folderPath, "IMG_0001.jpg");
            WriteFile(folderPath, "IMG_0002.jpg");
            var session = CreateSession(folderPath, "IMG_0001.jpg", "IMG_0002.jpg");
            var undoManager = new UndoManager();
            var fileMoveService = new FileMoveService();
            var journalStore = new JsonlSessionJournalStore();
            var deleteUseCase = new DeleteCurrentPhotoUseCase(
                new DeleteManager(),
                fileMoveService,
                undoManager,
                journalStore);
            var undoUseCase = new UndoLastDeleteUseCase(undoManager, fileMoveService, journalStore);

            var deleteResult = await deleteUseCase.ExecuteAsync(session);
            var undoResult = await undoUseCase.ExecuteAsync(session);

            Assert.AreEqual(DeleteCurrentPhotoStatus.Deleted, deleteResult.Status);
            Assert.AreEqual(UndoLastDeleteStatus.Restored, undoResult.Status);
            Assert.AreEqual(PhotoStatus.Restored, undoResult.RestoredPhoto?.Status);
            Assert.AreEqual("IMG_0001.jpg", undoResult.CurrentPhoto?.FileName);
            Assert.AreEqual(2, undoResult.ActiveCount);
            Assert.AreEqual(0, undoResult.DeletedCount);
            Assert.IsTrue(File.Exists(firstPath));
            Assert.IsFalse(File.Exists(deleteResult.FileMoveResult?.ActualDestinationPath));
            Assert.AreEqual(firstPath, undoResult.FileMoveResult?.ActualDestinationPath);
            var journalLines = await File.ReadAllLinesAsync(journalStore.GetJournalPath(session));
            Assert.HasCount(4, journalLines);
            StringAssert.Contains(journalLines[0], SessionJournalEventType.DeleteRequested);
            StringAssert.Contains(journalLines[1], SessionJournalEventType.Deleted);
            StringAssert.Contains(journalLines[2], SessionJournalEventType.UndoRequested);
            StringAssert.Contains(journalLines[3], SessionJournalEventType.Restored);
        }
        finally
        {
            DeleteTemporaryFolder(folderPath);
        }
    }

    [TestMethod]
    public async Task ReplaySessionJournalAfterReopenRecoversDeletedPhotoState()
    {
        var folderPath = CreateTemporaryFolder();
        try
        {
            WriteFile(folderPath, "IMG_0001.jpg");
            WriteFile(folderPath, "IMG_0002.jpg");
            var originalSession = CreateSession(folderPath, "IMG_0001.jpg", "IMG_0002.jpg");
            var journalStore = new JsonlSessionJournalStore();
            var deleteUseCase = new DeleteCurrentPhotoUseCase(
                new DeleteManager(),
                new FileMoveService(),
                new UndoManager(),
                journalStore);

            var deleteResult = await deleteUseCase.ExecuteAsync(originalSession);
            var reopenedSession = CreateSession(folderPath, "IMG_0002.jpg");
            var replayUseCase = new ReplaySessionJournalUseCase(
                journalStore,
                new FileSystemFileExistenceService());

            var replayResult = await replayUseCase.ExecuteAsync(reopenedSession);

            Assert.AreEqual(DeleteCurrentPhotoStatus.Deleted, deleteResult.Status);
            Assert.AreEqual(2, replayResult.EventsRead);
            Assert.AreEqual(1, replayResult.EventsApplied);
            Assert.AreEqual(1, replayResult.RecoveredPhotos);
            Assert.AreEqual(2, reopenedSession.InitialCount);
            Assert.AreEqual(1, reopenedSession.ActiveCount);
            Assert.AreEqual(1, reopenedSession.DeletedCount);
            var recoveredPhoto = reopenedSession.Photos.Single(photo => photo.FileName == "IMG_0001.jpg");
            Assert.AreEqual(PhotoStatus.Deleted, recoveredPhoto.Status);
        }
        finally
        {
            DeleteTemporaryFolder(folderPath);
        }
    }

    private static PhotoSession CreateSession(string folderPath, params string[] fileNames)
    {
        var candidates = fileNames.Select((fileName, index) =>
        {
            var fullPath = Path.Combine(folderPath, fileName);
            var fileInfo = new FileInfo(fullPath);
            return new PhotoFileCandidate(
                fileName,
                fullPath,
                folderPath,
                Path.GetExtension(fileName),
                fileInfo.Length,
                new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero),
                index);
        });

        return new PhotoSessionFactory().Create(folderPath, candidates);
    }

    private static string CreateTemporaryFolder()
    {
        var folderPath = Path.Combine(Path.GetTempPath(), $"evydencia-photo-selector-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    private static string WriteFile(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        File.WriteAllText(path, "jpeg");
        return path;
    }

    private static void DeleteTemporaryFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, recursive: true);
        }
    }
}
