using System.Globalization;
using Evydencia.PhotoSelector.Application.Display;
using Microsoft.UI.Xaml;

namespace Evydencia.PhotoSelector.App.Display;

internal sealed class WindowsDisplayContextService
{
    public DisplayContextSnapshot Capture(
        FrameworkElement viewerElement,
        bool isFullscreen,
        DisplayRole role = DisplayRole.Customer)
    {
        ArgumentNullException.ThrowIfNull(viewerElement);

        var xamlRoot = viewerElement.XamlRoot
            ?? throw new InvalidOperationException("The viewer element is not attached to a XamlRoot.");

        var rootSize = xamlRoot.Size;
        var effectiveWidthDips = ResolvePositiveSize(rootSize.Width, viewerElement.ActualWidth);
        var effectiveHeightDips = ResolvePositiveSize(rootSize.Height, viewerElement.ActualHeight);
        var viewerUsableWidthDips = ResolvePositiveSize(viewerElement.ActualWidth, effectiveWidthDips);
        var viewerUsableHeightDips = ResolvePositiveSize(viewerElement.ActualHeight, effectiveHeightDips);
        var rasterizationScale = ResolvePositiveSize(xamlRoot.RasterizationScale, 1.0);
        var displayId = CreateDisplayId(xamlRoot);

        return new DisplayContextSnapshot(
            displayId,
            effectiveWidthDips,
            effectiveHeightDips,
            viewerUsableWidthDips,
            viewerUsableHeightDips,
            rasterizationScale,
            isFullscreen,
            role);
    }

    private static string CreateDisplayId(XamlRoot xamlRoot)
    {
        var appWindowId = xamlRoot.ContentIslandEnvironment.AppWindowId;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"appwindow:{appWindowId.Value}");
    }

    private static double ResolvePositiveSize(double preferred, double fallback)
    {
        if (IsUsable(preferred))
        {
            return preferred;
        }

        if (IsUsable(fallback))
        {
            return fallback;
        }

        return 1.0;
    }

    private static bool IsUsable(double value)
    {
        return value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
