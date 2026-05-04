using Evydencia.PhotoSelector.App.Activation;
using Evydencia.PhotoSelector.App.Display;
using Evydencia.PhotoSelector.App.Imaging;
using Evydencia.PhotoSelector.App.Windowing;
using Evydencia.PhotoSelector.App.ViewModels;
using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Application.UseCases;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;
using Evydencia.PhotoSelector.Imaging.Decode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;

namespace Evydencia.PhotoSelector.App;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page, IDisposable
{
    private const VirtualKey MainKeyboardMinusKey = (VirtualKey)189;
    private const VirtualKey MainKeyboardPlusKey = (VirtualKey)187;
    private const double ViewerActualSizeZoomMax = 32.0;
    private const double ViewerActualSizeZoomMin = 0.05;
    private const double ViewerDoubleClickMaxDistance = 24.0;
    private static readonly TimeSpan ViewerOverlayVisibleDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ViewerPointerDoubleClickThreshold = TimeSpan.FromMilliseconds(500);

    private const double ViewerPanResetZoomThreshold = ViewerZoomMin + 0.01;
    private const double ViewerZoomMax = 6.0;
    private const double ViewerZoomMin = 1.0;
    private const double ViewerZoomStep = 1.2;

    private readonly DispatcherTimer _viewerOverlayHideTimer = new()
    {
        Interval = ViewerOverlayVisibleDuration
    };

    private PhotoSession? _currentSession;
    private bool _fileCommandInProgress;
    private bool _folderActivationInProgress;
    private CancellationTokenSource? _imageLoadCancellation;
    private bool _isActualSizeImageLoaded;
    private bool _isViewerPanning;
    private double _actualSizeBaseZoomFactor = ViewerZoomMin;
    private DateTimeOffset? _lastViewerClickAt;
    private Point _lastViewerClickPoint;
    private Point _lastViewerPanPoint;
    private double _viewerPanX;
    private double _viewerPanY;
    private uint? _viewerPanPointerId;
    private double _viewerZoomFactor = ViewerZoomMin;

    public MainPage()
    {
        InitializeComponent();
        RegisterViewerKeyboardAccelerators();
        _viewerOverlayHideTimer.Tick += OnViewerOverlayHideTimerTick;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public MainPageViewModel ViewModel { get; } = new();

    public void Dispose()
    {
        _imageLoadCancellation?.Cancel();
        _imageLoadCancellation?.Dispose();
        _imageLoadCancellation = null;
        _viewerOverlayHideTimer.Stop();
        _viewerOverlayHideTimer.Tick -= OnViewerOverlayHideTimerTick;
        GC.SuppressFinalize(this);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (Microsoft.UI.Xaml.Application.Current is App app)
        {
            app.FolderActivationRequested += OnFolderActivationRequested;
            var result = await ViewModel.LoadInitialSessionAsync(app.InitialSessionOpenTask);
            SyncVisualState();
            if (result?.Status == OpenFolderFromArgumentsStatus.Opened)
            {
                _currentSession = result.SessionResult?.Session;
                ViewerHost.Focus(FocusState.Programmatic);
                await LoadCurrentPhotoAsync(app);
            }
        }
    }

    private async void OnFolderActivationRequested(object? sender, FolderActivationRequestedEventArgs e)
    {
        if (Microsoft.UI.Xaml.Application.Current is App app)
        {
            await OpenRequestedFolderAsync(app, e);
        }
    }

    private async void OnViewerKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var isControlDown = IsControlKeyDown();
        if (!CanRouteViewerShortcut(e.Key, isControlDown))
        {
            return;
        }

        e.Handled = true;
        await HandleViewerShortcutSafelyAsync(e.Key, isControlDown);
    }

    private void OnViewerPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsViewerVisible || ViewModel.CurrentPhoto is null)
        {
            return;
        }

        var pointerPoint = e.GetCurrentPoint(ViewerHost);
        var delta = pointerPoint.Properties.MouseWheelDelta;
        if (delta == 0)
        {
            return;
        }

        e.Handled = true;
        var scale = delta > 0 ? ViewerZoomStep : 1 / ViewerZoomStep;
        ApplyViewerZoom(_viewerZoomFactor * scale, showStatus: true, pointerPoint.Position);
        ViewerHost.Focus(FocusState.Programmatic);
    }

    private async void OnViewerPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsViewerVisible || ViewModel.CurrentPhoto is null)
        {
            return;
        }

        var pointerPoint = e.GetCurrentPoint(ViewerHost);
        if (Microsoft.UI.Xaml.Application.Current is App app
            && await TryResetViewerZoomFromPointerDoubleClickAsync(app, pointerPoint.Position))
        {
            e.Handled = true;
            ViewerHost.Focus(FocusState.Pointer);
            return;
        }

        if (_viewerZoomFactor <= ViewerPanResetZoomThreshold
            || !pointerPoint.Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (!ViewerHost.CapturePointer(e.Pointer))
        {
            return;
        }

        _isViewerPanning = true;
        _viewerPanPointerId = e.Pointer.PointerId;
        _lastViewerPanPoint = pointerPoint.Position;
        e.Handled = true;
        ViewerHost.Focus(FocusState.Pointer);
    }

    private void OnViewerPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        ShowViewerOverlay();

        if (!_isViewerPanning || _viewerPanPointerId != e.Pointer.PointerId)
        {
            return;
        }

        var pointerPoint = e.GetCurrentPoint(ViewerHost);
        if (!pointerPoint.Properties.IsLeftButtonPressed)
        {
            EndViewerPan();
            return;
        }

        _viewerPanX += pointerPoint.Position.X - _lastViewerPanPoint.X;
        _viewerPanY += pointerPoint.Position.Y - _lastViewerPanPoint.Y;
        _lastViewerPanPoint = pointerPoint.Position;
        ApplyViewerTransform();
        e.Handled = true;
    }

    private void OnViewerPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_viewerPanPointerId == e.Pointer.PointerId)
        {
            EndViewerPan();
            e.Handled = true;
        }
    }

    private void OnViewerPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        if (_viewerPanPointerId == e.Pointer.PointerId)
        {
            EndViewerPan();
            e.Handled = true;
        }
    }

    private void OnViewerPointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        if (_viewerPanPointerId == e.Pointer.PointerId)
        {
            EndViewerPan(releaseCapture: false);
        }
    }

    private async void OnViewerDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (_viewerZoomFactor <= ViewerPanResetZoomThreshold
            || Microsoft.UI.Xaml.Application.Current is not App app)
        {
            return;
        }

        await ResetViewerToFitAsync(app, showStatus: true);
        e.Handled = true;
        ViewerHost.Focus(FocusState.Programmatic);
    }

    private void RegisterViewerKeyboardAccelerators()
    {
        AddViewerKeyboardAccelerator(VirtualKey.Right);
        AddViewerKeyboardAccelerator(VirtualKey.Left);
        AddViewerKeyboardAccelerator(VirtualKey.Space);
        AddViewerKeyboardAccelerator(VirtualKey.Delete);
        AddViewerKeyboardAccelerator(VirtualKey.Z, VirtualKeyModifiers.Control);
        AddViewerKeyboardAccelerator(VirtualKey.F);
        AddViewerKeyboardAccelerator(VirtualKey.Escape);
        AddViewerKeyboardAccelerator(VirtualKey.Home);
        AddViewerKeyboardAccelerator(VirtualKey.End);
        AddViewerKeyboardAccelerator(VirtualKey.Add);
        AddViewerKeyboardAccelerator(MainKeyboardPlusKey);
        AddViewerKeyboardAccelerator(VirtualKey.Subtract);
        AddViewerKeyboardAccelerator(MainKeyboardMinusKey);
        AddViewerKeyboardAccelerator(VirtualKey.Number0);
        AddViewerKeyboardAccelerator(VirtualKey.Number1);
    }

    private void AddViewerKeyboardAccelerator(
        VirtualKey key,
        VirtualKeyModifiers modifiers = VirtualKeyModifiers.None)
    {
        var accelerator = new KeyboardAccelerator
        {
            Key = key,
            Modifiers = modifiers
        };
        accelerator.Invoked += OnViewerKeyboardAcceleratorInvoked;
        KeyboardAccelerators.Add(accelerator);
    }

    private async void OnViewerKeyboardAcceleratorInvoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args)
    {
        var isControlDown = (sender.Modifiers & VirtualKeyModifiers.Control) == VirtualKeyModifiers.Control;
        if (IsTextInputFocused() || !CanRouteViewerShortcut(sender.Key, isControlDown))
        {
            return;
        }

        args.Handled = true;
        await HandleViewerShortcutSafelyAsync(sender.Key, isControlDown);
    }

    private bool CanRouteViewerShortcut(VirtualKey key, bool isControlDown)
    {
        if (Microsoft.UI.Xaml.Application.Current is not App || _currentSession is null)
        {
            return false;
        }

        if (isControlDown)
        {
            return key == VirtualKey.Z;
        }

        return key is VirtualKey.Right
            or VirtualKey.Left
            or VirtualKey.Space
            or VirtualKey.Delete
            or VirtualKey.F
            or VirtualKey.Escape
            or VirtualKey.Home
            or VirtualKey.End
            || IsViewerZoomShortcut(key);
    }

    private async Task HandleViewerShortcutSafelyAsync(VirtualKey key, bool isControlDown)
    {
        try
        {
            await HandleViewerShortcutAsync(key, isControlDown);
        }
        catch (OperationCanceledException)
        {
            ViewModel.SetViewerStatus("Operacao cancelada");
            SyncVisualState();
        }
        catch (Exception exception)
        {
            ViewModel.SetViewerStatus($"Falha no atalho: {exception.Message}");
            SyncVisualState();
        }
        finally
        {
            ViewerHost.Focus(FocusState.Programmatic);
            ShowViewerOverlay();
        }
    }

    private async Task HandleViewerShortcutAsync(VirtualKey key, bool isControlDown)
    {
        if (Microsoft.UI.Xaml.Application.Current is not App app || _currentSession is null)
        {
            return;
        }

        if (key == VirtualKey.Z && isControlDown)
        {
            await UndoLastDeleteAsync(app);
            return;
        }

        if (isControlDown)
        {
            return;
        }

        if (await HandleViewerZoomShortcutAsync(app, key))
        {
            return;
        }

        if (key == VirtualKey.Delete)
        {
            await DeleteCurrentPhotoAsync(app);
            return;
        }

        if (key == VirtualKey.F)
        {
            await ToggleFullscreenAsync(app);
            return;
        }

        if (key == VirtualKey.Escape)
        {
            var fullscreenService = app.Services.GetRequiredService<FullscreenService>();
            if (fullscreenService.IsFullscreen(app.MainWindow))
            {
                await ExitFullscreenAsync(app);
            }

            return;
        }

        await NavigateByShortcutAsync(app, key);
    }

    private async Task NavigateByShortcutAsync(App app, VirtualKey key)
    {
        if (_currentSession is null)
        {
            return;
        }

        var previousPhotoId = ViewModel.CurrentPhoto?.Id;
        var navigationResult = key switch
        {
            VirtualKey.Right => app.Services
                .GetRequiredService<NavigateNextPhotoUseCase>()
                .Execute(_currentSession),
            VirtualKey.Space => app.Services
                .GetRequiredService<NavigateNextPhotoUseCase>()
                .Execute(_currentSession),
            VirtualKey.Left => app.Services
                .GetRequiredService<NavigatePreviousPhotoUseCase>()
                .Execute(_currentSession),
            VirtualKey.Home => app.Services
                .GetRequiredService<NavigateFirstPhotoUseCase>()
                .Execute(_currentSession),
            VirtualKey.End => app.Services
                .GetRequiredService<NavigateLastPhotoUseCase>()
                .Execute(_currentSession),
            _ => null
        };

        if (navigationResult is null)
        {
            return;
        }

        if (ViewModel.HasCurrentImage && navigationResult.CurrentPhoto?.Id == previousPhotoId)
        {
            ViewModel.ApplyNavigation(navigationResult);
            SyncVisualState();
            ViewerHost.Focus(FocusState.Programmatic);
            return;
        }

        await NavigateToAsync(app, navigationResult);
    }

    private bool IsTextInputFocused()
    {
        try
        {
            if (XamlRoot is null)
            {
                return false;
            }

            return FocusManager.GetFocusedElement(XamlRoot) is TextBox
                or PasswordBox
                or RichEditBox
                or AutoSuggestBox;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private async Task DeleteCurrentPhotoAsync(App app)
    {
        if (_currentSession is null)
        {
            return;
        }

        if (_fileCommandInProgress)
        {
            ViewModel.SetViewerStatus("Operacao em andamento");
            ShowViewerOverlay();
            return;
        }

        _fileCommandInProgress = true;
        try
        {
            var deleteTask = app.Services
                .GetRequiredService<DeleteCurrentPhotoUseCase>()
                .ExecuteAsync(_currentSession);

            ApplyOptimisticFileCommandState("Excluindo foto");
            await LoadCurrentPhotoAsync(app);

            var result = await deleteTask;
            ViewModel.ApplyDeleteResult(result);
            SyncVisualState();
            ViewerHost.Focus(FocusState.Programmatic);

            if (result.CurrentPhoto is not null && !ViewModel.HasCurrentImage)
            {
                await LoadCurrentPhotoAsync(app);
            }
        }
        catch (OperationCanceledException)
        {
            ViewModel.SetViewerStatus("Exclusao cancelada");
            SyncVisualState();
        }
        catch (Exception exception)
        {
            ViewModel.SetViewerStatus($"Falha ao excluir: {exception.Message}");
            SyncVisualState();
        }
        finally
        {
            _fileCommandInProgress = false;
            ViewerHost.Focus(FocusState.Programmatic);
        }
    }

    private async Task UndoLastDeleteAsync(App app)
    {
        if (_currentSession is null)
        {
            return;
        }

        if (_fileCommandInProgress)
        {
            ViewModel.SetViewerStatus("Operacao em andamento");
            ShowViewerOverlay();
            return;
        }

        _fileCommandInProgress = true;
        try
        {
            ViewModel.SetViewerStatus("Restaurando foto");
            SyncVisualState();
            var result = await app.Services
                .GetRequiredService<UndoLastDeleteUseCase>()
                .ExecuteAsync(_currentSession);

            ViewModel.ApplyUndoResult(result);

            if (result.Status == UndoLastDeleteStatus.NoUndoAvailable)
            {
                SyncVisualState();
                ViewerHost.Focus(FocusState.Programmatic);
                return;
            }

            CurrentPhotoImage.Source = null;
            SyncVisualState();
            ViewerHost.Focus(FocusState.Programmatic);

            if (result.CurrentPhoto is not null)
            {
                ResetViewerZoom();
                await LoadCurrentPhotoAsync(app);
            }
        }
        catch (OperationCanceledException)
        {
            ViewModel.SetViewerStatus("Restauracao cancelada");
            SyncVisualState();
        }
        catch (Exception exception)
        {
            ViewModel.SetViewerStatus($"Falha ao restaurar: {exception.Message}");
            SyncVisualState();
        }
        finally
        {
            _fileCommandInProgress = false;
            ViewerHost.Focus(FocusState.Programmatic);
        }
    }

    private async Task OpenRequestedFolderAsync(App app, FolderActivationRequestedEventArgs e)
    {
        if (_fileCommandInProgress || _folderActivationInProgress)
        {
            ViewModel.SetViewerStatus("Operacao em andamento");
            SyncVisualState();
            ShowViewerOverlay();
            return;
        }

        var launchArguments = app.Services
            .GetRequiredService<FolderLaunchArgumentsParser>()
            .ParseRaw(e.RawArguments);
        if (!launchArguments.HasFolder)
        {
            return;
        }

        _folderActivationInProgress = true;
        try
        {
            if (_currentSession is not null && !await ConfirmOpenNewSessionAsync(launchArguments))
            {
                ViewModel.SetViewerStatus("Sessao atual mantida");
                SyncVisualState();
                ShowViewerOverlay();
                ViewerHost.Focus(FocusState.Programmatic);
                return;
            }

            _imageLoadCancellation?.Cancel();
            ResetViewerZoom();
            CurrentPhotoImage.Source = null;
            var result = await ViewModel.LoadInitialSessionAsync(
                Task.Run(() => app.OpenSessionFromRawArgumentsAsync(e.RawArguments)));
            _currentSession = result?.Status == OpenFolderFromArgumentsStatus.Opened
                ? result.SessionResult?.Session
                : null;
            SyncVisualState();

            if (result?.Status == OpenFolderFromArgumentsStatus.Opened)
            {
                ViewerHost.Focus(FocusState.Programmatic);
                await LoadCurrentPhotoAsync(app);
            }
        }
        finally
        {
            _folderActivationInProgress = false;
            ViewerHost.Focus(FocusState.Programmatic);
        }
    }

    private async Task<bool> ConfirmOpenNewSessionAsync(FolderLaunchArguments launchArguments)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Abrir nova sessao?",
            Content = $"Ja existe uma sessao aberta. Abrir a pasta recebida?{Environment.NewLine}{Environment.NewLine}{launchArguments.FolderPath}",
            PrimaryButtonText = "Abrir nova sessao",
            CloseButtonText = "Manter atual",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private void ApplyOptimisticFileCommandState(string statusText)
    {
        if (_currentSession is null)
        {
            return;
        }

        var currentPhoto = ResolveCurrentActivePhoto(_currentSession);
        ViewModel.ApplyDeletePending(
            currentPhoto,
            _currentSession.CurrentIndex,
            _currentSession.ActiveCount);
        ViewModel.SetViewerStatus(statusText);
        ResetViewerZoom();
        CurrentPhotoImage.Source = null;
        SyncVisualState();
        ViewerHost.Focus(FocusState.Programmatic);
    }

    private async Task NavigateToAsync(App app, NavigationResult navigationResult)
    {
        ViewModel.ApplyNavigation(navigationResult);
        ResetViewerZoom();
        CurrentPhotoImage.Source = null;
        SyncVisualState();
        ViewerHost.Focus(FocusState.Programmatic);
        await LoadCurrentPhotoAsync(app);
    }

    private async Task ToggleFullscreenAsync(App app)
    {
        var window = app.MainWindow;
        if (window is null)
        {
            return;
        }

        var fullscreenService = app.Services.GetRequiredService<FullscreenService>();
        var isFullscreen = fullscreenService.ToggleFullscreen(window);
        await ApplyFullscreenStateAsync(app, isFullscreen);
    }

    private async Task ExitFullscreenAsync(App app)
    {
        var window = app.MainWindow;
        if (window is null)
        {
            return;
        }

        app.Services.GetRequiredService<FullscreenService>().ExitFullscreen(window);
        await ApplyFullscreenStateAsync(app, isFullscreen: false);
    }

    private async Task ApplyFullscreenStateAsync(App app, bool isFullscreen)
    {
        ViewModel.SetFullscreen(isFullscreen);
        SyncVisualState();
        ViewerHost.Focus(FocusState.Programmatic);
        await LoadCurrentPhotoAsync(app);
    }

    private static PhotoItem? ResolveCurrentActivePhoto(PhotoSession session)
    {
        var active = session.ActivePhotos();
        if (active.Count == 0)
        {
            return null;
        }

        return active[Math.Min(session.CurrentIndex, active.Count - 1)];
    }

    private static bool IsControlKeyDown()
    {
        var state = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
        return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    }

    private async Task<bool> TryResetViewerZoomFromPointerDoubleClickAsync(App app, Point pointerPosition)
    {
        var now = DateTimeOffset.UtcNow;
        var isDoubleClick = _lastViewerClickAt.HasValue
            && now - _lastViewerClickAt.Value <= ViewerPointerDoubleClickThreshold
            && GetDistance(_lastViewerClickPoint, pointerPosition) <= ViewerDoubleClickMaxDistance;

        _lastViewerClickAt = now;
        _lastViewerClickPoint = pointerPosition;

        if (!isDoubleClick || _viewerZoomFactor <= ViewerPanResetZoomThreshold)
        {
            return false;
        }

        _lastViewerClickAt = null;
        await ResetViewerToFitAsync(app, showStatus: true);
        return true;
    }

    private static double GetDistance(Point first, Point second)
    {
        var deltaX = second.X - first.X;
        var deltaY = second.Y - first.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private void EndViewerPan(bool releaseCapture = true)
    {
        _isViewerPanning = false;
        _viewerPanPointerId = null;

        if (releaseCapture)
        {
            ViewerHost.ReleasePointerCaptures();
        }
    }

    private async Task<bool> HandleViewerZoomShortcutAsync(App app, VirtualKey key)
    {
        if (!IsViewerZoomShortcut(key))
        {
            return false;
        }

        if (key == VirtualKey.Number1)
        {
            await LoadActualSizePhotoAsync(app);
            return true;
        }

        if (key == VirtualKey.Number0)
        {
            await ResetViewerToFitAsync(app, showStatus: true);
            return true;
        }

        if (IsViewerZoomInShortcut(key))
        {
            ApplyViewerZoom(_viewerZoomFactor * ViewerZoomStep, showStatus: true);
            return true;
        }

        if (IsViewerZoomOutShortcut(key))
        {
            ApplyViewerZoom(_viewerZoomFactor / ViewerZoomStep, showStatus: true);
            return true;
        }

        return false;
    }

    private static bool IsViewerZoomShortcut(VirtualKey key)
    {
        return IsViewerZoomInShortcut(key)
            || IsViewerZoomOutShortcut(key)
            || key == VirtualKey.Number0
            || key == VirtualKey.Number1;
    }

    private static bool IsViewerZoomInShortcut(VirtualKey key)
    {
        return key == VirtualKey.Add || key == MainKeyboardPlusKey;
    }

    private static bool IsViewerZoomOutShortcut(VirtualKey key)
    {
        return key == VirtualKey.Subtract || key == MainKeyboardMinusKey;
    }

    private void ApplyViewerZoom(double zoomFactor, bool showStatus, Point? anchorPoint = null)
    {
        var previousZoomFactor = _viewerZoomFactor;
        var minZoomFactor = _isActualSizeImageLoaded ? ViewerActualSizeZoomMin : ViewerZoomMin;
        var maxZoomFactor = _isActualSizeImageLoaded ? ViewerActualSizeZoomMax : ViewerZoomMax;
        _viewerZoomFactor = Math.Clamp(zoomFactor, minZoomFactor, maxZoomFactor);

        if (anchorPoint.HasValue
            && previousZoomFactor > 0
            && _viewerZoomFactor > ViewerPanResetZoomThreshold
            && ViewerHost.ActualWidth > 0
            && ViewerHost.ActualHeight > 0)
        {
            KeepViewerAnchorStable(anchorPoint.Value, previousZoomFactor, _viewerZoomFactor);
        }

        if (_viewerZoomFactor <= ViewerPanResetZoomThreshold)
        {
            _viewerPanX = 0;
            _viewerPanY = 0;
            EndViewerPan();
        }

        ApplyViewerTransform();

        if (!showStatus)
        {
            return;
        }

        ViewModel.SetViewerStatus(_viewerZoomFactor <= ViewerZoomMin + 0.01 && !_isActualSizeImageLoaded
            ? string.Empty
            : FormatViewerZoomStatus());
        SyncVisualState();
        ShowViewerOverlay();
    }

    private void KeepViewerAnchorStable(Point anchorPoint, double previousZoomFactor, double nextZoomFactor)
    {
        var viewerCenterX = ViewerHost.ActualWidth / 2;
        var viewerCenterY = ViewerHost.ActualHeight / 2;
        var pointerOffsetX = anchorPoint.X - viewerCenterX;
        var pointerOffsetY = anchorPoint.Y - viewerCenterY;
        var scaleRatio = nextZoomFactor / previousZoomFactor;

        _viewerPanX = pointerOffsetX - (scaleRatio * (pointerOffsetX - _viewerPanX));
        _viewerPanY = pointerOffsetY - (scaleRatio * (pointerOffsetY - _viewerPanY));
    }

    private void ApplyViewerTransform()
    {
        ClampViewerPan();
        CurrentPhotoTransform.ScaleX = _viewerZoomFactor;
        CurrentPhotoTransform.ScaleY = _viewerZoomFactor;
        CurrentPhotoTransform.TranslateX = _viewerPanX;
        CurrentPhotoTransform.TranslateY = _viewerPanY;
    }

    private void ClampViewerPan()
    {
        if (_viewerZoomFactor <= ViewerPanResetZoomThreshold)
        {
            _viewerPanX = 0;
            _viewerPanY = 0;
            return;
        }

        var maxPanX = Math.Max(0, (ViewerHost.ActualWidth * (_viewerZoomFactor - 1)) / 2);
        var maxPanY = Math.Max(0, (ViewerHost.ActualHeight * (_viewerZoomFactor - 1)) / 2);
        _viewerPanX = Math.Clamp(_viewerPanX, -maxPanX, maxPanX);
        _viewerPanY = Math.Clamp(_viewerPanY, -maxPanY, maxPanY);
    }

    private void ResetViewerZoom(bool showStatus = false)
    {
        ResetActualSizeState();
        _viewerZoomFactor = ViewerZoomMin;
        _viewerPanX = 0;
        _viewerPanY = 0;
        EndViewerPan();
        ApplyViewerTransform();

        if (!showStatus)
        {
            return;
        }

        ViewModel.SetViewerStatus("Ajustado a tela");
        SyncVisualState();
        ShowViewerOverlay();
    }

    private async Task ResetViewerToFitAsync(App app, bool showStatus)
    {
        var reloadPreview = _isActualSizeImageLoaded && ViewModel.CurrentPhoto is not null;
        ResetViewerZoom(showStatus);

        if (reloadPreview)
        {
            await LoadCurrentPhotoAsync(app);
            if (showStatus)
            {
                ViewModel.SetViewerStatus("Ajustado a tela");
                SyncVisualState();
                ShowViewerOverlay();
            }
        }
    }

    private async Task LoadActualSizePhotoAsync(App app)
    {
        if (ViewModel.CurrentPhoto is null)
        {
            return;
        }

        _imageLoadCancellation?.Cancel();
        _imageLoadCancellation?.Dispose();
        _imageLoadCancellation = new CancellationTokenSource();
        var cancellationToken = _imageLoadCancellation.Token;
        var requestedPhotoId = ViewModel.CurrentPhoto.Id;

        ViewModel.SetViewerStatus("Carregando 100%");
        SyncVisualState();
        ShowViewerOverlay();

        try
        {
            await Task.Yield();

            var displayContext = app.Services
                .GetRequiredService<WindowsDisplayContextService>()
                .Capture(ViewerHost, ViewModel.IsFullscreen);
            var decodeResult = await app.Services
                .GetRequiredService<JpegDecodeService>()
                .DecodeActualSizeAsync(ViewModel.CurrentPhoto.FullPath, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (ViewModel.CurrentPhoto?.Id != requestedPhotoId)
            {
                return;
            }

            if (!decodeResult.IsSuccess)
            {
                ViewModel.SetViewerStatus($"Falha ao carregar 100%: {decodeResult.ErrorCode}");
                SyncVisualState();
                ShowViewerOverlay();
                return;
            }

            var imageSource = await app.Services
                .GetRequiredService<ViewerImageSourceFactory>()
                .CreateAsync(decodeResult, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (imageSource is null)
            {
                ViewModel.SetViewerStatus("Falha ao preparar 100%");
                SyncVisualState();
                ShowViewerOverlay();
                return;
            }

            CurrentPhotoImage.Source = imageSource;
            ViewModel.CompleteImageLoad();
            ApplyActualSizeZoom(decodeResult, displayContext);
            ViewModel.SetViewerStatus("Zoom 100%");
            SyncVisualState();
            ShowViewerOverlay();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            ViewModel.SetViewerStatus($"Falha ao carregar 100%: {exception.Message}");
            SyncVisualState();
            ShowViewerOverlay();
        }
    }

    private void ApplyActualSizeZoom(ImageDecodeResult decodeResult, DisplayContextSnapshot displayContext)
    {
        _isActualSizeImageLoaded = true;
        _actualSizeBaseZoomFactor = CalculateActualSizeBaseZoomFactor(decodeResult, displayContext);
        _viewerZoomFactor = _actualSizeBaseZoomFactor;
        _viewerPanX = 0;
        _viewerPanY = 0;
        ApplyViewerTransform();
    }

    private static double CalculateActualSizeBaseZoomFactor(
        ImageDecodeResult decodeResult,
        DisplayContextSnapshot displayContext)
    {
        var fitScale = Math.Min(
            displayContext.ViewerUsableWidthPixels / (double)decodeResult.PixelWidth,
            displayContext.ViewerUsableHeightPixels / (double)decodeResult.PixelHeight);
        if (!double.IsFinite(fitScale) || fitScale <= 0)
        {
            return ViewerZoomMin;
        }

        return Math.Clamp(1 / fitScale, ViewerActualSizeZoomMin, ViewerActualSizeZoomMax);
    }

    private string FormatViewerZoomStatus()
    {
        var zoomPercent = _isActualSizeImageLoaded && _actualSizeBaseZoomFactor > 0
            ? (_viewerZoomFactor / _actualSizeBaseZoomFactor) * 100
            : _viewerZoomFactor * 100;
        return $"Zoom {Math.Round(zoomPercent)}%";
    }

    private void ResetActualSizeState()
    {
        _isActualSizeImageLoaded = false;
        _actualSizeBaseZoomFactor = ViewerZoomMin;
    }

    private async Task LoadCurrentPhotoAsync(App app)
    {
        if (ViewModel.CurrentPhoto is null)
        {
            return;
        }

        _imageLoadCancellation?.Cancel();
        _imageLoadCancellation?.Dispose();
        _imageLoadCancellation = new CancellationTokenSource();
        var cancellationToken = _imageLoadCancellation.Token;

        ViewModel.BeginImageLoad();
        CurrentPhotoImage.Source = null;
        SyncVisualState();

        try
        {
            await Task.Yield();

            var displayContext = app.Services
                .GetRequiredService<WindowsDisplayContextService>()
                .Capture(ViewerHost, ViewModel.IsFullscreen);
            var decodeResult = await app.Services
                .GetRequiredService<JpegDecodeService>()
                .DecodeForDisplayAsync(ViewModel.CurrentPhoto.FullPath, displayContext, cancellationToken);

            if (!decodeResult.IsSuccess)
            {
                ViewModel.FailImageLoad($"Falha ao carregar imagem: {decodeResult.ErrorCode}");
                SyncVisualState();
                return;
            }

            var imageSource = await app.Services
                .GetRequiredService<ViewerImageSourceFactory>()
                .CreateAsync(decodeResult, cancellationToken);

            if (imageSource is null)
            {
                ViewModel.FailImageLoad("Falha ao preparar imagem");
                SyncVisualState();
                return;
            }

            CurrentPhotoImage.Source = imageSource;
            ResetActualSizeState();
            ViewModel.CompleteImageLoad();
            SyncVisualState();
            ShowViewerOverlay();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            ViewModel.FailImageLoad(exception.Message);
            SyncVisualState();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (Microsoft.UI.Xaml.Application.Current is App app)
        {
            app.FolderActivationRequested -= OnFolderActivationRequested;
        }

        Dispose();
    }

    private void SyncVisualState()
    {
        ViewerHost.Visibility = ViewModel.IsViewerVisible ? Visibility.Visible : Visibility.Collapsed;
        HomeHost.Visibility = ViewModel.IsHomeVisible ? Visibility.Visible : Visibility.Collapsed;

        if (!ViewModel.IsViewerVisible)
        {
            HideViewerOverlay();
        }

        if (!ViewModel.HasCurrentImage)
        {
            CurrentPhotoImage.Source = null;
        }
    }

    private void OnViewerOverlayHideTimerTick(object? sender, object e)
    {
        _viewerOverlayHideTimer.Stop();

        if (ViewModel.IsViewerVisible && ViewModel.HasCurrentImage)
        {
            ViewerOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowViewerOverlay(bool autoHide = true)
    {
        if (!ViewModel.IsViewerVisible)
        {
            return;
        }

        ViewerOverlay.Visibility = Visibility.Visible;

        if (!autoHide || !ViewModel.HasCurrentImage)
        {
            return;
        }

        _viewerOverlayHideTimer.Stop();
        _viewerOverlayHideTimer.Start();
    }

    private void HideViewerOverlay()
    {
        _viewerOverlayHideTimer.Stop();
        ViewerOverlay.Visibility = Visibility.Collapsed;
    }
}
