using Evydencia.PhotoSelector.Application.Activation;

namespace Evydencia.PhotoSelector.Application.Tests.Activation;

[TestClass]
public sealed class FolderLaunchArgumentsParserTests
{
    private readonly FolderLaunchArgumentsParser _parser = new();

    [TestMethod]
    public void ParseRawEmptyReturnsEmptyArguments()
    {
        var result = _parser.ParseRaw("");

        Assert.IsFalse(result.HasFolder);
        Assert.IsNull(result.FolderPath);
        Assert.IsNull(result.Source);
    }

    [TestMethod]
    public void ParseRawSeparatedFolderReturnsFolderPath()
    {
        var result = _parser.ParseRaw("--folder C:\\Fotos");

        Assert.IsTrue(result.HasFolder);
        Assert.AreEqual("C:\\Fotos", result.FolderPath);
    }

    [TestMethod]
    public void ParseRawInlineFolderReturnsFolderPath()
    {
        var result = _parser.ParseRaw("--folder=C:\\Fotos");

        Assert.AreEqual("C:\\Fotos", result.FolderPath);
    }

    [TestMethod]
    public void ParseRawQuotedFolderKeepsSpaces()
    {
        var result = _parser.ParseRaw("--folder \"C:\\Sessao Cliente 01\"");

        Assert.AreEqual("C:\\Sessao Cliente 01", result.FolderPath);
    }

    [TestMethod]
    public void ParseRawSlashFolderReturnsFolderPath()
    {
        var result = _parser.ParseRaw("/folder \"C:\\Sessao Cliente 02\"");

        Assert.AreEqual("C:\\Sessao Cliente 02", result.FolderPath);
    }

    [TestMethod]
    public void ParseRawSourceExplorerReturnsSource()
    {
        var result = _parser.ParseRaw("--folder C:\\Fotos --source explorer");

        Assert.AreEqual("C:\\Fotos", result.FolderPath);
        Assert.AreEqual("explorer", result.Source);
    }

    [TestMethod]
    public void ParseRawUnknownArgumentsAreIgnored()
    {
        var result = _parser.ParseRaw("--unknown value --folder C:\\Fotos");

        Assert.AreEqual("C:\\Fotos", result.FolderPath);
        Assert.IsNull(result.Source);
    }

    [TestMethod]
    public void ParseCommandLineArgumentsUsesProvidedTokens()
    {
        var result = _parser.Parse(["--folder", "C:\\Fotos", "--source", "launcher"]);

        Assert.AreEqual("C:\\Fotos", result.FolderPath);
        Assert.AreEqual("launcher", result.Source);
    }
}
