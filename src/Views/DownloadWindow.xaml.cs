// ###########################################################################
//  Project:      OmniDownloader
//  Version:      0.9.1
//  Author:       Tomer Alcavi
//  GitHub:       https://github.com/alcavix
//  Project Link: https://github.com/alcavix/OmniDownloader
//  License:      MIT
//
//  If you find this project useful, drop a star or fork!
//  Questions or ideas? Open an issue on the project’s GitHub page!
//  Please keep this little credit line. It means a lot for the open-source spirit :)
//  Grateful for the open-source community and spirit that inspires projects like this.
// ###########################################################################

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
