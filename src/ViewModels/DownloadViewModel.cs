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

using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniDownloader.Models;
using OmniDownloader.Services;

namespace OmniDownloader.ViewModels;

public partial class DownloadViewModel : ObservableObject
{
    private readonly DownloadService _downloadService;
    private readonly DownloadParameters _parameters;
    
    [ObservableProperty]
    private string fileName = string.Empty;
    
    [ObservableProperty]
    private string title = string.Empty;
    
    [ObservableProperty]
    private string description = string.Empty;
    
    [ObservableProperty]
    private bool hasDescription = false;
    
    [ObservableProperty]
    private bool showPostActionPrompt = false;
    
    [ObservableProperty]
    private string downloadedFilePath = string.Empty;
    
    [ObservableProperty]
    private double progressPercentage = 0;
    
    [ObservableProperty]
    private string downloadSpeed = "0 B/s";
    
    [ObservableProperty]
    private string estimatedTimeRemaining = "--";
    
    [ObservableProperty]
    private string statusMessage = "Preparing download...";
    
    [ObservableProperty]
    private bool isDownloadComplete = false;
    
    [ObservableProperty]
    private bool hasError = false;
    
    [ObservableProperty]
    private string errorMessage = string.Empty;
    
    public event Action? RequestClose;
    
    public DownloadViewModel(DownloadParameters parameters)
    {
        _parameters = parameters;
        _downloadService = new DownloadService();
        
        FileName = parameters.FileName;
        Title = parameters.Title;
        Description = parameters.Description;
        HasDescription = !string.IsNullOrEmpty(parameters.Description);
        DownloadedFilePath = parameters.FilePath;
        
        _downloadService.ProgressChanged += OnProgressChanged;
        _downloadService.DownloadCompleted += OnDownloadCompleted;
    }
    
    public async Task StartDownloadAsync()
    {
        try
        {
            StatusMessage = "Starting download...";
            await _downloadService.StartDownloadAsync(_parameters.Url, _parameters.FilePath);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "Download failed";
        }
    }
    
    private void OnProgressChanged(DownloadProgress progress)
    {
        // In Avalonia, update UI from background thread
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressPercentage = progress.PercentComplete / 100.0; // Convert to 0-1 range for ProgressBar
            DownloadSpeed = progress.FormattedSpeed;
            EstimatedTimeRemaining = progress.FormattedETA;
            StatusMessage = $"Downloading... {progress.PercentComplete:F1}%";
        });
    }
    
    private void OnDownloadCompleted(bool success, string? errorMessage)
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (success)
            {
                // Ensure progress shows 100% first
                ProgressPercentage = 1.0;
                StatusMessage = "Download completed successfully!";
                
                // Small delay to ensure UI updates the progress bar before showing completion
                System.Threading.Tasks.Task.Delay(50).ContinueWith(_ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        IsDownloadComplete = true;
                        // Handle post-download action based on parameter
                        HandlePostDownloadAction();
                    });
                });
            }
            else
            {
                HasError = true;
                ErrorMessage = errorMessage ?? "An unknown error occurred";
                StatusMessage = "Download failed";
            }
        });
    }
    
    private void HandlePostDownloadAction()
    {
        switch (_parameters.PostAction)
        {
            case Models.PostDownloadAction.Open:
                OpenDownloadedFile();
                // Auto-close after 2 seconds to give time for file to open
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        RequestClose?.Invoke();
                    });
                });
                break;
                
            case Models.PostDownloadAction.Close:
                // Auto-close after 2 seconds
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        RequestClose?.Invoke();
                    });
                });
                break;
                
            case Models.PostDownloadAction.Ask:
                ShowPostActionPrompt = true;
                break;
        }
    }
    
    [RelayCommand]
    private void CancelDownload()
    {
        _downloadService.CancelDownload();
        RequestClose?.Invoke();
    }
    
    [RelayCommand]
    private void CloseWindow()
    {
        RequestClose?.Invoke();
    }
    
    [RelayCommand]
    private void OpenFile()
    {
        ShowFileInFolder();
    }
    
    [RelayCommand]
    private void OpenFileAndClose()
    {
        OpenDownloadedFile();
        RequestClose?.Invoke();
    }
    
    private void OpenDownloadedFile()
    {
        try
        {
            if (File.Exists(DownloadedFilePath))
            {
                // Cross-platform file opening
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = DownloadedFilePath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(startInfo);
            }
        }
        catch (Exception ex)
        {
            // Handle file opening errors gracefully
            StatusMessage = $"Could not open file: {ex.Message}";
        }
    }
    
    private void ShowFileInFolder()
    {
        try
        {
            if (File.Exists(DownloadedFilePath))
            {
                // Cross-platform way to show file in folder
                if (OperatingSystem.IsWindows())
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{DownloadedFilePath}\"");
                }
                else if (OperatingSystem.IsMacOS())
                {
                    System.Diagnostics.Process.Start("open", $"-R \"{DownloadedFilePath}\"");
                }
                else
                {
                    // For Linux, open the containing directory
                    var directory = Path.GetDirectoryName(DownloadedFilePath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        System.Diagnostics.Process.Start("xdg-open", directory);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle file opening errors gracefully
            StatusMessage = $"Could not show file in folder: {ex.Message}";
        }
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
    }
}
