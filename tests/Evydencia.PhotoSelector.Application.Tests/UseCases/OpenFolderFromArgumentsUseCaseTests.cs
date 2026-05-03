using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Tests.Fakes;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Tests.UseCases;

[TestClass]
public sealed class OpenFolderFromArgumentsUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsyncWithoutFolderDoesNotScan()
    {
        var scanner = new FakeFolderScanner([Candidate("IMG_0001.jpg")]);
        var useCase = CreateUseCase(scanner);

        var result = await useCase.ExecuteAsync(["--source", "explorer"]);

        Assert.AreEqual(OpenFolderFromArgumentsStatus.NoFolderArgument, result.Status);
        Assert.IsNull(result.SessionResult);
        Assert.AreEqual(0, scanner.ScanCount);
        Assert.AreEqual("explorer", result.LaunchArguments.Source);
    }

    [TestMethod]
    public async Task ExecuteAsyncWithFolderOpensSession()
    {
        var scanner = new FakeFolderScanner([
            Candidate("IMG_0002.jpg"),
            Candidate("IMG_0001.jpg")
        ]);
        var useCase = CreateUseCase(scanner);

        var result = await useCase.ExecuteAsync(["--folder", "C:\\Sessao", "--source", "launcher"]);

        Assert.AreEqual(OpenFolderFromArgumentsStatus.Opened, result.Status);
        Assert.AreEqual("C:\\Sessao", scanner.LastRequest?.FolderPath);
        Assert.AreEqual("launcher", result.LaunchArguments.Source);
        Assert.AreEqual(2, result.SessionResult?.Session.InitialCount);
        Assert.AreEqual("IMG_0001.jpg", result.SessionResult?.CurrentPhoto?.FileName);
    }

    [TestMethod]
    public async Task ExecuteAsyncWhenScannerFailsReturnsFailureResult()
    {
        var scanner = new FakeFolderScanner(
            [],
            new DirectoryNotFoundException("folder not found"));
        var useCase = CreateUseCase(scanner);

        var result = await useCase.ExecuteAsync(["--folder", "C:\\NaoExiste"]);

        Assert.AreEqual(OpenFolderFromArgumentsStatus.Failed, result.Status);
        Assert.IsNull(result.SessionResult);
        Assert.AreEqual("folder not found", result.ErrorMessage);
        Assert.AreEqual(1, scanner.ScanCount);
    }

    private static OpenFolderFromArgumentsUseCase CreateUseCase(FakeFolderScanner scanner)
    {
        return new OpenFolderFromArgumentsUseCase(
            new FolderLaunchArgumentsParser(),
            new OpenSessionUseCase(scanner, new PhotoSessionFactory()));
    }

    private static PhotoFileCandidate Candidate(string fileName)
    {
        return new PhotoFileCandidate(
            fileName,
            $"C:\\Sessao\\{fileName}",
            "C:\\Sessao",
            fileName[fileName.LastIndexOf('.')..],
            1024,
            DateTimeOffset.UtcNow,
            0);
    }
}
