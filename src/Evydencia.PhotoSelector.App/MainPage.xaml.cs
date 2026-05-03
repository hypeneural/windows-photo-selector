using Evydencia.PhotoSelector.App.Display;
using Evydencia.PhotoSelector.App.Imaging;
using Evydencia.PhotoSelector.App.ViewModels;
using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Imaging.Decode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Evydencia.PhotoSelector.App;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page, IDisposable
{
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
                await LoadCurrentPhotoAsync(app);
            }
        }
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
                .Capture(ViewerHost, isFullscreen: false);
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
