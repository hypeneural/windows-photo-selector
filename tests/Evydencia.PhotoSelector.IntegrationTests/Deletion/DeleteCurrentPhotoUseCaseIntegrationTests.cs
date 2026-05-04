using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Deletion;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Core.Undo;
using Evydencia.PhotoSelector.Storage.Filesystem;

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
            var useCase = new DeleteCurrentPhotoUseCase(new DeleteManager(), new FileMoveService(), new UndoManager());

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
            var deleteUseCase = new DeleteCurrentPhotoUseCase(new DeleteManager(), fileMoveService, undoManager);
            var undoUseCase = new UndoLastDeleteUseCase(undoManager, fileMoveService);

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
