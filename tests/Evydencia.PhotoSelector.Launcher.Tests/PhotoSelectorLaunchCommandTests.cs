namespace Evydencia.PhotoSelector.Launcher.Tests;

[TestClass]
public sealed class PhotoSelectorLaunchCommandTests
{
    private static readonly string[] ExpectedArguments = [
        "--folder",
        "C:\\Sessao Cliente",
        "--source",
        "explorer"
    ];

    [TestMethod]
    public void CreateStartInfoPassesFolderAndSourceAsSeparateArguments()
    {
        var startInfo = PhotoSelectorLaunchCommand.CreateStartInfo(
            "C:\\App\\Evydencia.PhotoSelector.App.exe",
            "C:\\Sessao Cliente",
            "explorer");

        Assert.AreEqual("C:\\App\\Evydencia.PhotoSelector.App.exe", startInfo.FileName);
        Assert.IsFalse(startInfo.UseShellExecute);
        CollectionAssert.AreEqual(
            ExpectedArguments,
            startInfo.ArgumentList.ToArray());
    }
}
