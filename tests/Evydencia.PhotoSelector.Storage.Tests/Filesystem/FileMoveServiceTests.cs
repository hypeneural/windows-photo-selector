using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Storage.Filesystem;

namespace Evydencia.PhotoSelector.Storage.Tests.Filesystem;

[TestClass]
public sealed class FileMoveServiceTests
{
    [TestMethod]
    public async Task MoveToDeletedFolderAsyncMovesFileToDeletedFolder()
    {
        using var folder = TemporaryFolder.Create();
        var sourcePath = folder.WriteFile("IMG_0001.jpg", "jpeg");
        var service = new FileMoveService();

        var result = await service.MoveToDeletedFolderAsync(sourcePath, folder.Path);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(FileMoveErrorCode.None, result.ErrorCode);
        Assert.IsFalse(File.Exists(sourcePath));
        Assert.IsNotNull(result.ActualDestinationPath);
        Assert.IsTrue(File.Exists(result.ActualDestinationPath));
        Assert.AreEqual(
            Path.Combine(folder.Path, FolderScanPolicy.DeletedFolderName, "IMG_0001.jpg"),
            result.ActualDestinationPath);
        Assert.IsFalse(result.CollisionResolved);
    }

    [TestMethod]
    public async Task MoveToDeletedFolderAsyncWhenDestinationExistsUsesUniqueName()
    {
        using var folder = TemporaryFolder.Create();
        var sourcePath = folder.WriteFile("IMG_0001.jpg", "new");
        var existingPath = folder.WriteFile($"{FolderScanPolicy.DeletedFolderName}\\IMG_0001.jpg", "existing");
        var service = new FileMoveService();

        var result = await service.MoveToDeletedFolderAsync(sourcePath, folder.Path);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.CollisionResolved);
        Assert.IsFalse(File.Exists(sourcePath));
        Assert.IsTrue(File.Exists(existingPath));
        Assert.IsNotNull(result.ActualDestinationPath);
        Assert.IsTrue(File.Exists(result.ActualDestinationPath));
        Assert.AreNotEqual(existingPath, result.ActualDestinationPath);
        StringAssert.Contains(Path.GetFileName(result.ActualDestinationPath), "__deleted_");
    }

    [TestMethod]
    public async Task RestoreAsyncMovesDeletedFileBackToOriginalPath()
    {
        using var folder = TemporaryFolder.Create();
        var deletedPath = folder.WriteFile($"{FolderScanPolicy.DeletedFolderName}\\IMG_0001.jpg", "jpeg");
        var originalPath = Path.Combine(folder.Path, "IMG_0001.jpg");
        var service = new FileMoveService();

        var result = await service.RestoreAsync(deletedPath, originalPath);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(File.Exists(deletedPath));
        Assert.IsTrue(File.Exists(originalPath));
        Assert.AreEqual(originalPath, result.ActualDestinationPath);
        Assert.IsFalse(result.CollisionResolved);
    }

    [TestMethod]
    public async Task RestoreAsyncWhenOriginalExistsUsesUniqueName()
    {
        using var folder = TemporaryFolder.Create();
        var originalPath = folder.WriteFile("IMG_0001.jpg", "current");
        var deletedPath = folder.WriteFile($"{FolderScanPolicy.DeletedFolderName}\\IMG_0001.jpg", "deleted");
        var service = new FileMoveService();

        var result = await service.RestoreAsync(deletedPath, originalPath);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.CollisionResolved);
        Assert.IsTrue(File.Exists(originalPath));
        Assert.IsFalse(File.Exists(deletedPath));
        Assert.IsNotNull(result.ActualDestinationPath);
        Assert.IsTrue(File.Exists(result.ActualDestinationPath));
        Assert.AreNotEqual(originalPath, result.ActualDestinationPath);
        StringAssert.Contains(Path.GetFileName(result.ActualDestinationPath), "__restored_");
    }

    [TestMethod]
    public async Task MoveToDeletedFolderAsyncWhenSourceMissingReturnsFailure()
    {
        using var folder = TemporaryFolder.Create();
        var sourcePath = Path.Combine(folder.Path, "missing.jpg");
        var service = new FileMoveService();

        var result = await service.MoveToDeletedFolderAsync(sourcePath, folder.Path);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FileMoveErrorCode.SourceMissing, result.ErrorCode);
        Assert.IsNull(result.ActualDestinationPath);
    }

    [TestMethod]
    public async Task MoveAndRestorePreserveLastWriteTimeUtc()
    {
        using var folder = TemporaryFolder.Create();
        var sourcePath = folder.WriteFile("IMG_0001.jpg", "jpeg");
        var lastWriteTimeUtc = new DateTime(2026, 5, 3, 15, 30, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(sourcePath, lastWriteTimeUtc);
        var service = new FileMoveService();

        var moveResult = await service.MoveToDeletedFolderAsync(sourcePath, folder.Path);
        var restoreResult = await service.RestoreAsync(moveResult.ActualDestinationPath!, sourcePath);

        Assert.IsTrue(moveResult.IsSuccess);
        Assert.IsTrue(restoreResult.IsSuccess);
        Assert.AreEqual(lastWriteTimeUtc, File.GetLastWriteTimeUtc(sourcePath));
    }

    [TestMethod]
    public async Task MoveAndRestoreReadOnlyFile()
    {
        using var folder = TemporaryFolder.Create();
        var sourcePath = folder.WriteFile("IMG_0001.jpg", "jpeg");
        File.SetAttributes(sourcePath, File.GetAttributes(sourcePath) | FileAttributes.ReadOnly);
        var service = new FileMoveService();

        var moveResult = await service.MoveToDeletedFolderAsync(sourcePath, folder.Path);
        var restoreResult = await service.RestoreAsync(moveResult.ActualDestinationPath!, sourcePath);

        Assert.IsTrue(moveResult.IsSuccess);
        Assert.IsTrue(restoreResult.IsSuccess);
        Assert.IsTrue(File.Exists(sourcePath));
        Assert.IsTrue(File.GetAttributes(sourcePath).HasFlag(FileAttributes.ReadOnly));
    }
}
