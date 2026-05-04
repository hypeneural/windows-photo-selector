using Evydencia.PhotoSelector.App.Activation;
using Evydencia.PhotoSelector.App.Composition;
using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.AppLifecycle;
using Microsoft.UI.Xaml;

namespace Evydencia.PhotoSelector.App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Microsoft.UI.Xaml.Application
{
    private Window? _window;

    internal event EventHandler<FolderActivationRequestedEventArgs>? FolderActivationRequested;

    public IServiceProvider Services { get; }

    public FolderLaunchArguments LaunchArguments { get; private set; } = FolderLaunchArguments.Empty;

    public OpenFolderFromArgumentsResult? InitialSessionOpenResult { get; private set; }

    public Task<OpenFolderFromArgumentsResult>? InitialSessionOpenTask { get; private set; }

    public Window? MainWindow => _window;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();
        Services = AppCompositionRoot.CreateServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _ = args;

        AppInstanceCoordinator.MainInstance.Activated += OnAppInstanceActivated;
        InitialSessionOpenTask = Task.Run(OpenInitialSessionAsync);

        _window = new MainWindow();
        _window.Activate();
    }

    public async Task<OpenFolderFromArgumentsResult> OpenSessionFromRawArgumentsAsync(string rawArguments)
    {
        var useCase = Services.GetRequiredService<OpenFolderFromArgumentsUseCase>();
        var result = await useCase.ExecuteRawAsync(rawArguments).ConfigureAwait(false);
        StoreOpenResult(result);
        return result;
    }

    public async Task<OpenFolderFromArgumentsResult> OpenSessionFromArgumentsAsync(IReadOnlyList<string> commandLineArguments)
    {
        var useCase = Services.GetRequiredService<OpenFolderFromArgumentsUseCase>();
        var result = await useCase.ExecuteAsync(commandLineArguments).ConfigureAwait(false);
        StoreOpenResult(result);
        return result;
    }

    private Task<OpenFolderFromArgumentsResult> OpenInitialSessionAsync()
    {
        var activationArguments = AppInstance.GetCurrent().GetActivatedEventArgs();
        var rawArguments = LaunchActivationArgumentsReader.ReadRawArguments(activationArguments);
        return string.IsNullOrWhiteSpace(rawArguments)
            ? OpenSessionFromArgumentsAsync(Environment.GetCommandLineArgs().Skip(1).ToArray())
            : OpenSessionFromRawArgumentsAsync(rawArguments);
    }

    private void OnAppInstanceActivated(object? sender, AppActivationArguments args)
    {
        var rawArguments = LaunchActivationArgumentsReader.ReadRawArguments(args);
        if (string.IsNullOrWhiteSpace(rawArguments) || _window is null)
        {
            return;
        }

        _window.DispatcherQueue.TryEnqueue(() =>
        {
            _window.Activate();
            FolderActivationRequested?.Invoke(this, new FolderActivationRequestedEventArgs(rawArguments));
        });
    }

    private void StoreOpenResult(OpenFolderFromArgumentsResult result)
    {
        LaunchArguments = result.LaunchArguments;
        InitialSessionOpenResult = result;
    }
}
