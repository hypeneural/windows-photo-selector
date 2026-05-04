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
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Add)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(MainKeyboardPlusKey)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Subtract)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(MainKeyboardMinusKey)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Number0)");
        StringAssert.Contains(source, "AddViewerKeyboardAccelerator(VirtualKey.Number1)");
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

    [TestMethod]
    public void MainPageHidesKeyboardAcceleratorTooltips()
    {
        var xaml = File.ReadAllText(GetMainPageXamlPath());

        StringAssert.Contains(xaml, "KeyboardAcceleratorPlacementMode=\"Hidden\"");
    }

    [TestMethod]
    public void MainPageSupportsMouseWheelZoom()
    {
        var source = File.ReadAllText(GetMainPageSourcePath());
        var xaml = File.ReadAllText(GetMainPageXamlPath());

        StringAssert.Contains(xaml, "PointerWheelChanged=\"OnViewerPointerWheelChanged\"");
        StringAssert.Contains(xaml, "IsHitTestVisible=\"False\"");
        StringAssert.Contains(xaml, "CompositeTransform x:Name=\"CurrentPhotoTransform\"");
        StringAssert.Contains(source, "MouseWheelDelta");
        StringAssert.Contains(source, "ApplyViewerZoom");
        StringAssert.Contains(source, "ResetViewerZoom");
    }

    [TestMethod]
    public void MainPageSupportsPanningWhenZoomed()
    {
        var source = File.ReadAllText(GetMainPageSourcePath());
        var xaml = File.ReadAllText(GetMainPageXamlPath());

        StringAssert.Contains(xaml, "PointerPressed=\"OnViewerPointerPressed\"");
        StringAssert.Contains(xaml, "PointerMoved=\"OnViewerPointerMoved\"");
        StringAssert.Contains(xaml, "PointerReleased=\"OnViewerPointerReleased\"");
        StringAssert.Contains(xaml, "PointerCanceled=\"OnViewerPointerCanceled\"");
        StringAssert.Contains(xaml, "PointerCaptureLost=\"OnViewerPointerCaptureLost\"");
        StringAssert.Contains(source, "CapturePointer(e.Pointer)");
        StringAssert.Contains(source, "ReleasePointerCaptures()");
        StringAssert.Contains(source, "CurrentPhotoTransform.TranslateX");
        StringAssert.Contains(source, "CurrentPhotoTransform.TranslateY");
        StringAssert.Contains(source, "ClampViewerPan");
    }

    [TestMethod]
    public void MainPageSupportsDoubleTapZoomReset()
    {
        var source = File.ReadAllText(GetMainPageSourcePath());
        var xaml = File.ReadAllText(GetMainPageXamlPath());

        StringAssert.Contains(xaml, "DoubleTapped=\"OnViewerDoubleTapped\"");
        StringAssert.Contains(source, "DoubleTappedRoutedEventArgs");
        StringAssert.Contains(source, "TryResetViewerZoomFromPointerDoubleClick");
        StringAssert.Contains(source, "ViewerPointerDoubleClickThreshold");
        StringAssert.Contains(source, "ResetViewerToFitAsync(app, showStatus: true)");
        StringAssert.Contains(source, "Ajustado a tela");
    }

    [TestMethod]
    public void MainPageUsesTimedViewerOverlay()
    {
        var source = File.ReadAllText(GetMainPageSourcePath());
        var xaml = File.ReadAllText(GetMainPageXamlPath());

        StringAssert.Contains(xaml, "x:Name=\"ViewerOverlay\"");
        StringAssert.Contains(source, "DispatcherTimer");
        StringAssert.Contains(source, "ViewerOverlayVisibleDuration");
        StringAssert.Contains(source, "OnViewerOverlayHideTimerTick");
        StringAssert.Contains(source, "ShowViewerOverlay");
        StringAssert.Contains(source, "ViewerOverlay.Visibility = Visibility.Collapsed");
    }

    [TestMethod]
    public void MainPageSupportsActualSizeZoomShortcut()
    {
        var source = File.ReadAllText(GetMainPageSourcePath());

        StringAssert.Contains(source, "VirtualKey.Number1");
        StringAssert.Contains(source, "LoadActualSizePhotoAsync");
        StringAssert.Contains(source, "DecodeActualSizeAsync");
        StringAssert.Contains(source, "CalculateActualSizeBaseZoomFactor");
        StringAssert.Contains(source, "Zoom 100%");
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
