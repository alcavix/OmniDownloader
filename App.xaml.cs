using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OmniDownloader.Models;
using OmniDownloader.ViewModels;
using OmniDownloader.Views;

namespace OmniDownloader;

public partial class App : Application
{
    public static DownloadParameters? DownloadParameters { get; set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (DownloadParameters != null)
            {
                var viewModel = new DownloadViewModel(DownloadParameters);
                var window = new DownloadWindow
                {
                    DataContext = viewModel
                };
                
                desktop.MainWindow = window;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
