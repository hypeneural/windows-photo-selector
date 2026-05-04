using Evydencia.PhotoSelector.App.Display;
using Evydencia.PhotoSelector.App.Imaging;
using Evydencia.PhotoSelector.App.Windowing;
using Evydencia.PhotoSelector.App.ViewModels;
using Evydencia.PhotoSelector.Application.Activation;
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
using Windows.System;
using Windows.UI.Core;

namespace Evydencia.PhotoSelector.App;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page, IDisposable
{
    private PhotoSession? _currentSession;
    private bool _fileCommandInProgress;
    private CancellationTokenSource? _imageLoadCancellation;

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public MainPageViewModel ViewModel { get; } = new();

    public void Dispose()
    {
        _imageLoadCancellation?.Cancel();
        _imageLoadCancellation?.Dispose();
        _imageLoadCancellation = null;
        GC.SuppressFinalize(this);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (Microsoft.UI.Xaml.Application.Current is App app)
        {
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

    private async void OnViewerKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (Microsoft.UI.Xaml.Application.Current is not App app || _currentSession is null)
        {
            return;
        }

        if (e.Key == VirtualKey.Z && IsControlKeyDown())
        {
            e.Handled = true;
            await UndoLastDeleteAsync(app);
            return;
        }

        if (e.Key == VirtualKey.Delete)
        {
            e.Handled = true;
            await DeleteCurrentPhotoAsync(app);
            return;
        }

        if (e.Key == VirtualKey.F)
        {
            e.Handled = true;
            await ToggleFullscreenAsync(app);
            return;
        }

        if (e.Key == VirtualKey.Escape)
        {
            var fullscreenService = app.Services.GetRequiredService<FullscreenService>();
            if (fullscreenService.IsFullscreen(app.MainWindow))
            {
                e.Handled = true;
                await ExitFullscreenAsync(app);
            }

            return;
        }

        var previousPhotoId = ViewModel.CurrentPhoto?.Id;
        var navigationResult = e.Key switch
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
            _ => null
        };

        if (navigationResult is null)
        {
            return;
        }

        e.Handled = true;
        if (ViewModel.HasCurrentImage && navigationResult.CurrentPhoto?.Id == previousPhotoId)
        {
            ViewModel.ApplyNavigation(navigationResult);
            SyncVisualState();
            ViewerHost.Focus(FocusState.Programmatic);
            return;
        }

        await NavigateToAsync(app, navigationResult);
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
        CurrentPhotoImage.Source = null;
        SyncVisualState();
        ViewerHost.Focus(FocusState.Programmatic);
    }

    private async Task NavigateToAsync(App app, NavigationResult navigationResult)
    {
        ViewModel.ApplyNavigation(navigationResult);
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
            ViewModel.CompleteImageLoad();
            SyncVisualState();
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
        Dispose();
    }

    private void SyncVisualState()
    {
        ViewerHost.Visibility = ViewModel.IsViewerVisible ? Visibility.Visible : Visibility.Collapsed;
        HomeHost.Visibility = ViewModel.IsHomeVisible ? Visibility.Visible : Visibility.Collapsed;

        if (!ViewModel.HasCurrentImage)
        {
            CurrentPhotoImage.Source = null;
        }
    }
}
