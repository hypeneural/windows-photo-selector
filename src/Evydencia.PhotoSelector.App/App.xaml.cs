using Evydencia.PhotoSelector.App.Composition;
using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.UseCases;
using Microsoft.UI.Xaml;

namespace Evydencia.PhotoSelector.App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Microsoft.UI.Xaml.Application
{
    private Window? _window;

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

        var commandLineArguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
        InitialSessionOpenTask = Task.Run(() => OpenInitialSessionAsync(commandLineArguments));

        _window = new MainWindow();
        _window.Activate();
    }

    private async Task<OpenFolderFromArgumentsResult> OpenInitialSessionAsync(string[] commandLineArguments)
    {
        var useCase = (OpenFolderFromArgumentsUseCase?)Services.GetService(typeof(OpenFolderFromArgumentsUseCase));
        if (useCase is null)
        {
            InitialSessionOpenResult = OpenFolderFromArgumentsResult.NoFolderArgument(FolderLaunchArguments.Empty);
            return InitialSessionOpenResult;
        }

        var result = await useCase.ExecuteAsync(commandLineArguments).ConfigureAwait(false);
        LaunchArguments = result.LaunchArguments;
        InitialSessionOpenResult = result;
        return result;
    }
}
