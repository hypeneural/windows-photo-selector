using System.ComponentModel;
using System.Runtime.CompilerServices;
using Evydencia.PhotoSelector.Application.Activation;

namespace Evydencia.PhotoSelector.App.ViewModels;

public sealed class MainPageViewModel : INotifyPropertyChanged
{
    private string _detailText = "Aguardando pasta";
    private bool _hasSession;
    private bool _isLoading;
    private string _photoCountText = "0 JPEGs";
    private string _statusText = "Nenhuma pasta carregada";

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

    public async Task LoadInitialSessionAsync(Task<OpenFolderFromArgumentsResult>? initialSessionOpenTask)
    {
        if (initialSessionOpenTask is null)
        {
            ApplyNoFolder();
            return;
        }

        IsLoading = true;
        StatusText = "Carregando sessao";
        DetailText = "Preparando fotos";
        PhotoCountText = "0 JPEGs";
        HasSession = false;

        var result = await initialSessionOpenTask;
        IsLoading = false;
        ApplyResult(result);
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
        var count = result.SessionResult?.Session.InitialCount ?? 0;

        StatusText = "Sessao carregada";
        DetailText = string.IsNullOrWhiteSpace(folderName) ? folderPath : folderName;
        PhotoCountText = FormatPhotoCount(count);
        HasSession = true;
    }

    private void ApplyFailed(OpenFolderFromArgumentsResult result)
    {
        StatusText = "Falha ao abrir pasta";
        DetailText = result.ErrorMessage ?? "Erro desconhecido";
        PhotoCountText = "0 JPEGs";
        HasSession = false;
    }

    private void ApplyNoFolder()
    {
        StatusText = "Nenhuma pasta carregada";
        DetailText = "Aguardando pasta";
        PhotoCountText = "0 JPEGs";
        HasSession = false;
        IsLoading = false;
    }

    private static string FormatPhotoCount(int count)
    {
        return count == 1 ? "1 JPEG" : $"{count} JPEGs";
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
