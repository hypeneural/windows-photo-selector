using Evydencia.PhotoSelector.Core.Scanning;

namespace Evydencia.PhotoSelector.Core.Tests.Scanning;

[TestClass]
public sealed class FolderScanPolicyTests
{
    [TestMethod]
    [DataRow("IMG_0001.jpg")]
    [DataRow("IMG_0001.jpeg")]
    [DataRow("IMG_0001.JPG")]
    [DataRow("C:\\sessao\\IMG_0001.JPEG")]
    public void AcceptsJpegExtensionsCaseInsensitive(string fileName)
    {
        Assert.IsTrue(FolderScanPolicy.IsAcceptedPhotoFile(fileName));
    }

    [TestMethod]
    [DataRow("IMG_0001.png")]
    [DataRow("IMG_0001.raw")]
    [DataRow("IMG_0001")]
    [DataRow("")]
    public void RejectsNonJpegFiles(string fileName)
    {
        Assert.IsFalse(FolderScanPolicy.IsAcceptedPhotoFile(fileName));
    }

    [TestMethod]
    public void IgnoresDeletedFolder()
    {
        Assert.IsTrue(FolderScanPolicy.ShouldIgnoreDirectoryName("_deletadas_evydencia"));
        Assert.IsTrue(FolderScanPolicy.ShouldIgnoreDirectoryName("_DELETADAS_EVYDENCIA"));
    }

    [TestMethod]
    public void RejectsCandidateInsideDeletedFolder()
    {
        Assert.IsFalse(FolderScanPolicy.ShouldIncludeCandidate("IMG_0001.jpg", "_deletadas_evydencia"));
    }
}
