using Evydencia.PhotoSelector.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Evydencia.PhotoSelector.App;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public MainPageViewModel ViewModel { get; } = new();

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (Microsoft.UI.Xaml.Application.Current is App app)
        {
            await ViewModel.LoadInitialSessionAsync(app.InitialSessionOpenTask);
        }
    }
}
