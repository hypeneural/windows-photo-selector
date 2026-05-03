using System.ComponentModel;
using System.Runtime.CompilerServices;
using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Core.Photos;

namespace Evydencia.PhotoSelector.App.ViewModels;

public sealed class MainPageViewModel : INotifyPropertyChanged
{
    private string _currentImageAutomationName = "Foto atual";
    private string _currentFileName = string.Empty;
    private PhotoItem? _currentPhoto;
    private string _detailText = "Aguardando pasta";
    private bool _hasCurrentImage;
    private bool _hasSession;
    private bool _isHomeVisible = true;
    private bool _isImageLoading;
    private bool _isLoading;
    private bool _isViewerVisible;
    private string _photoCountText = "0 JPEGs";
    private string _statusText = "Nenhuma pasta carregada";
    private string _viewerCounterText = string.Empty;
    private string _viewerStatusText = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string DetailText
    {
        get => _detailText;
        private set => SetProperty(ref _detailText, value);
    }

    public string PhotoCountText
    {
        get => _photoCountText;
        private set => SetProperty(ref _photoCountText, value);
    }

    public bool HasSession
    {
        get => _hasSession;
        private set => SetProperty(ref _hasSession, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public PhotoItem? CurrentPhoto
    {
        get => _currentPhoto;
        private set => SetProperty(ref _currentPhoto, value);
    }

    public string CurrentFileName
    {
        get => _currentFileName;
        private set => SetProperty(ref _currentFileName, value);
    }

    public string ViewerCounterText
    {
        get => _viewerCounterText;
        private set => SetProperty(ref _viewerCounterText, value);
    }

    public string ViewerStatusText
    {
        get => _viewerStatusText;
        private set => SetProperty(ref _viewerStatusText, value);
    }

    public string CurrentImageAutomationName
    {
        get => _currentImageAutomationName;
        private set => SetProperty(ref _currentImageAutomationName, value);
    }

    public bool IsImageLoading
    {
        get => _isImageLoading;
        private set => SetProperty(ref _isImageLoading, value);
    }

    public bool HasCurrentImage
    {
        get => _hasCurrentImage;
        private set => SetProperty(ref _hasCurrentImage, value);
    }

    public bool IsViewerVisible
    {
        get => _isViewerVisible;
        private set => SetProperty(ref _isViewerVisible, value);
    }

    public bool IsHomeVisible
    {
        get => _isHomeVisible;
        private set => SetProperty(ref _isHomeVisible, value);
    }

    public async Task<OpenFolderFromArgumentsResult?> LoadInitialSessionAsync(Task<OpenFolderFromArgumentsResult>? initialSessionOpenTask)
    {
        if (initialSessionOpenTask is null)
        {
            ApplyNoFolder();
            return null;
        }

        IsLoading = true;
        StatusText = "Carregando sessao";
        DetailText = "Preparando fotos";
        PhotoCountText = "0 JPEGs";
        HasSession = false;
        IsViewerVisible = false;
        IsHomeVisible = true;

        var result = await initialSessionOpenTask;
        IsLoading = false;
        ApplyResult(result);
        return result;
    }

    public void BeginImageLoad()
    {
        if (CurrentPhoto is null)
        {
            return;
        }

        HasCurrentImage = false;
        IsImageLoading = true;
        ViewerStatusText = "Carregando imagem";
    }

    public void CompleteImageLoad()
    {
        HasCurrentImage = true;
        IsImageLoading = false;
        ViewerStatusText = string.Empty;
    }

    public void FailImageLoad(string message)
    {
        HasCurrentImage = false;
        IsImageLoading = false;
        ViewerStatusText = string.IsNullOrWhiteSpace(message)
            ? "Falha ao carregar imagem"
            : message;
    }

    private void ApplyResult(OpenFolderFromArgumentsResult result)
    {
        switch (result.Status)
        {
            case OpenFolderFromArgumentsStatus.Opened:
                ApplyOpened(result);
                break;
            case OpenFolderFromArgumentsStatus.Failed:
                ApplyFailed(result);
                break;
            default:
                ApplyNoFolder();
                break;
        }
    }

    private void ApplyOpened(OpenFolderFromArgumentsResult result)
    {
        var folderPath = result.LaunchArguments.FolderPath ?? string.Empty;
        var folderName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var session = result.SessionResult?.Session;
        var currentPhoto = result.SessionResult?.CurrentPhoto;
        var count = session?.InitialCount ?? 0;

        StatusText = "Sessao carregada";
        DetailText = string.IsNullOrWhiteSpace(folderName) ? folderPath : folderName;
        PhotoCountText = FormatPhotoCount(count);
        HasSession = true;
        CurrentPhoto = currentPhoto;
        HasCurrentImage = false;
        CurrentFileName = currentPhoto?.FileName ?? string.Empty;
        CurrentImageAutomationName = string.IsNullOrWhiteSpace(CurrentFileName)
            ? "Foto atual"
            : $"Foto atual {CurrentFileName}";
        ViewerCounterText = currentPhoto is null || session is null
            ? string.Empty
            : $"1 / {session.ActiveCount}";
        ViewerStatusText = currentPhoto is null ? "Nenhum JPEG encontrado" : "Aguardando imagem";
        IsViewerVisible = true;
        IsHomeVisible = false;
    }

    private void ApplyFailed(OpenFolderFromArgumentsResult result)
    {
        StatusText = "Falha ao abrir pasta";
        DetailText = result.ErrorMessage ?? "Erro desconhecido";
        PhotoCountText = "0 JPEGs";
        HasSession = false;
        ResetViewerState();
    }

    private void ApplyNoFolder()
    {
        StatusText = "Nenhuma pasta carregada";
        DetailText = "Aguardando pasta";
        PhotoCountText = "0 JPEGs";
        HasSession = false;
        IsLoading = false;
        ResetViewerState();
    }

    private static string FormatPhotoCount(int count)
    {
        return count == 1 ? "1 JPEG" : $"{count} JPEGs";
    }

    private void ResetViewerState()
    {
        CurrentPhoto = null;
        HasCurrentImage = false;
        CurrentFileName = string.Empty;
        ViewerCounterText = string.Empty;
        ViewerStatusText = string.Empty;
        CurrentImageAutomationName = "Foto atual";
        IsImageLoading = false;
        IsViewerVisible = false;
        IsHomeVisible = true;
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
