namespace Evydencia.PhotoSelector.Launcher.Tests;

[TestClass]
public sealed class LauncherArgumentsParserTests
{
    [TestMethod]
    public void ParseWithFolderAppAndSourceReturnsOptions()
    {
        var parser = new LauncherArgumentsParser();

        var result = parser.Parse([
            "--folder",
            "C:\\Sessao Cliente",
            "--app",
            "C:\\App\\Evydencia.PhotoSelector.App.exe",
            "--source",
            "explorer"
        ]);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("C:\\Sessao Cliente", result.Options?.FolderPath);
        Assert.AreEqual("C:\\App\\Evydencia.PhotoSelector.App.exe", result.Options?.AppPath);
        Assert.AreEqual("explorer", result.Options?.Source);
    }

    [TestMethod]
    public void ParseWithPositionalFolderUsesItAsFolder()
    {
        var parser = new LauncherArgumentsParser();

        var result = parser.Parse(["C:\\Sessao"]);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("C:\\Sessao", result.Options?.FolderPath);
        Assert.AreEqual(LauncherOptions.DefaultSource, result.Options?.Source);
    }

    [TestMethod]
    public void ParseWithUnknownArgumentFails()
    {
        var parser = new LauncherArgumentsParser();

        var result = parser.Parse(["--unexpected"]);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.ErrorMessage, "--unexpected");
    }

    [TestMethod]
    public void ParseWithMissingFolderValueFails()
    {
        var parser = new LauncherArgumentsParser();

        var result = parser.Parse(["--folder"]);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.ErrorMessage, "--folder");
    }

    [TestMethod]
    public void ParseWithSwitchAfterFolderOptionFails()
    {
        var parser = new LauncherArgumentsParser();

        var result = parser.Parse(["--folder", "--source", "explorer"]);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.ErrorMessage, "--folder");
    }

    [TestMethod]
    public void ParseWithMultiplePositionalFoldersFails()
    {
        var parser = new LauncherArgumentsParser();

        var result = parser.Parse(["C:\\Sessao 1", "C:\\Sessao 2"]);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.ErrorMessage, "apenas uma pasta");
    }
}
