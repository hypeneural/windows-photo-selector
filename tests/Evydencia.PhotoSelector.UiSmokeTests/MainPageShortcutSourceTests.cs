namespace Evydencia.PhotoSelector.UiSmokeTests;

[TestClass]
public sealed class MainPageShortcutSourceTests
{
    [TestMethod]
    public void MainPageRegistersKeyboardAcceleratorsForViewerShortcuts()
    {
        var source = File.ReadAllText(GetMainPageSourcePath());

        StringAssert.Contains(source, "KeyboardAcceleratorInvokedEventArgs");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Right)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Left)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Space)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Delete)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Z, VirtualKeyModifiers.Control)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.F)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Escape)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Home)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.End)");
    }

    [TestMethod]
    public void MainPageKeepsKeyDownAsFallbackForViewerShortcuts()
    {
        var source = File.ReadAllText(GetMainPageSourcePath());
        var xaml = File.ReadAllText(GetMainPageXamlPath());

        StringAssert.Contains(xaml, "KeyDown=\"OnViewerKeyDown\"");
        StringAssert.Contains(source, "HandleViewerShortcutSafelyAsync(e.Key, isControlDown)");
        StringAssert.Contains(source, "HandleViewerShortcutSafelyAsync(sender.Key, isControlDown)");
    }

    private static string GetMainPageSourcePath()
    {
        return GetAppFilePath("MainPage.xaml.cs");
    }

    private static string GetMainPageXamlPath()
    {
        return GetAppFilePath("MainPage.xaml");
    }

    private static string GetAppFilePath(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "Evydencia.PhotoSelector.App",
                fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        Assert.Fail($"{fileName} was not found from the test output directory.");
        throw new InvalidOperationException("Unreachable.");
    }
}
