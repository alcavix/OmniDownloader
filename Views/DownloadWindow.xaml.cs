using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OmniDownloader.ViewModels;

namespace OmniDownloader.Views;

public partial class DownloadWindow : Window
{
    public DownloadWindow()
    {
        InitializeComponent();
        
        // Subscribe to DataContext changes
        this.PropertyChanged += OnPropertyChanged;
        this.Opened += OnWindowOpened;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(DataContext) && DataContext is DownloadViewModel viewModel)
        {
            viewModel.RequestClose += OnRequestClose;
        }
    }
    
    private async void OnWindowOpened(object? sender, EventArgs e)
    {
        if (DataContext is DownloadViewModel viewModel)
        {
            await viewModel.StartDownloadAsync();
        }
    }
    
    private void OnRequestClose()
    {
        Close();
    }
}
