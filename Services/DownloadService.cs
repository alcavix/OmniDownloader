using System.Diagnostics;
using OmniDownloader.Models;

namespace OmniDownloader.Services;

public class DownloadService
{
    private readonly HttpClient _httpClient;
    private readonly int _numberOfThreads;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public event Action<DownloadProgress>? ProgressChanged;
    public event Action<bool, string?>? DownloadCompleted;
    
    public DownloadService(int numberOfThreads = 4)
    {
        _numberOfThreads = numberOfThreads;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DownloaderGUI/1.0");
    }
    
    public async Task StartDownloadAsync(string url, string filePath)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            var fileName = Path.GetFileName(filePath);
            var directory = Path.GetDirectoryName(filePath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Get file size first
            var fileSize = await GetFileSizeAsync(url);
            
            if (fileSize <= 0)
            {
                // Fallback to single-threaded download
                await DownloadSingleThreaded(url, filePath, fileName);
                return;
            }
            
            // Check if server supports range requests
            var supportsRanges = await SupportsRangeRequestsAsync(url);
            
            if (!supportsRanges || fileSize < 1024 * 1024) // Less than 1MB, use single thread
            {
                await DownloadSingleThreaded(url, filePath, fileName);
                return;
            }
            
            // Multi-threaded download
            await DownloadMultiThreaded(url, filePath, fileName, fileSize);
        }
        catch (Exception ex)
        {
            DownloadCompleted?.Invoke(false, ex.Message);
        }
    }
    
    private async Task<long> GetFileSizeAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request, _cancellationTokenSource!.Token);
            
            if (response.Content.Headers.ContentLength.HasValue)
            {
                return response.Content.Headers.ContentLength.Value;
            }
        }
        catch
        {
            // Ignore errors, will fallback to GET request
        }
        
        return 0;
    }
    
    private async Task<bool> SupportsRangeRequestsAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request, _cancellationTokenSource!.Token);
            
            return response.Headers.AcceptRanges?.Contains("bytes") == true;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task DownloadSingleThreaded(string url, string filePath, string fileName)
    {
        var stopwatch = Stopwatch.StartNew();
        long totalBytesReceived = 0;
        
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource!.Token);
        response.EnsureSuccessStatusCode();
        
        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        
        using var contentStream = await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token);
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        
        var buffer = new byte[8192];
        int bytesRead;
        
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
            totalBytesReceived += bytesRead;
            
            var progress = CreateProgressReport(fileName, totalBytesReceived, totalBytes, stopwatch.Elapsed);
            ProgressChanged?.Invoke(progress);
        }
        
        DownloadCompleted?.Invoke(true, null);
    }
    
    private async Task DownloadMultiThreaded(string url, string filePath, string fileName, long fileSize)
    {
        var chunkSize = fileSize / _numberOfThreads;
        var tasks = new List<Task>();
        var tempFiles = new List<string>();
        var stopwatch = Stopwatch.StartNew();
        var progressLock = new object();
        long totalBytesReceived = 0;
        
        for (int i = 0; i < _numberOfThreads; i++)
        {
            var start = i * chunkSize;
            var end = i == _numberOfThreads - 1 ? fileSize - 1 : start + chunkSize - 1;
            var tempFile = $"{filePath}.part{i}";
            tempFiles.Add(tempFile);
            
            tasks.Add(DownloadChunk(url, tempFile, start, end, fileName, fileSize, progressLock, 
                () => totalBytesReceived, bytes => Interlocked.Add(ref totalBytesReceived, bytes), stopwatch));
        }
        
        await Task.WhenAll(tasks);
        
        // Combine temp files
        await CombineTempFiles(tempFiles, filePath);
        
        // Clean up temp files
        foreach (var tempFile in tempFiles)
        {
            try { File.Delete(tempFile); } catch { }
        }
        
        DownloadCompleted?.Invoke(true, null);
    }
    
    private async Task DownloadChunk(string url, string tempFile, long start, long end, string fileName, 
        long totalFileSize, object progressLock, Func<long> getTotalBytes, Action<long> addBytes, Stopwatch stopwatch)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
        
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource!.Token);
        response.EnsureSuccessStatusCode();
        
        using var contentStream = await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token);
        using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        
        var buffer = new byte[8192];
        int bytesRead;
        
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
            addBytes(bytesRead);
            
            lock (progressLock)
            {
                var progress = CreateProgressReport(fileName, getTotalBytes(), totalFileSize, stopwatch.Elapsed);
                ProgressChanged?.Invoke(progress);
            }
        }
    }
    
    private async Task CombineTempFiles(List<string> tempFiles, string outputFile)
    {
        using var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
        
        foreach (var tempFile in tempFiles)
        {
            using var inputStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
            await inputStream.CopyToAsync(outputStream);
        }
    }
    
    private DownloadProgress CreateProgressReport(string fileName, long bytesReceived, long totalBytes, TimeSpan elapsed)
    {
        var progress = new DownloadProgress
        {
            FileName = fileName,
            BytesReceived = bytesReceived,
            TotalBytes = totalBytes
        };
        
        if (totalBytes > 0)
        {
            progress.PercentComplete = Math.Round((double)bytesReceived / totalBytes * 100, 1);
        }
        
        if (elapsed.TotalSeconds > 0)
        {
            progress.DownloadSpeedBytesPerSecond = bytesReceived / elapsed.TotalSeconds;
            progress.FormattedSpeed = FormatSpeed(progress.DownloadSpeedBytesPerSecond);
            
            if (progress.DownloadSpeedBytesPerSecond > 0 && totalBytes > 0)
            {
                var remainingBytes = totalBytes - bytesReceived;
                var remainingSeconds = remainingBytes / progress.DownloadSpeedBytesPerSecond;
                progress.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingSeconds);
                progress.FormattedETA = FormatTimeSpan(progress.EstimatedTimeRemaining);
            }
        }
        
        return progress;
    }
    
    private string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024)
            return $"{bytesPerSecond:F0} B/s";
        else if (bytesPerSecond < 1024 * 1024)
            return $"{bytesPerSecond / 1024:F1} KB/s";
        else if (bytesPerSecond < 1024 * 1024 * 1024)
            return $"{bytesPerSecond / (1024 * 1024):F1} MB/s";
        else
            return $"{bytesPerSecond / (1024 * 1024 * 1024):F1} GB/s";
    }
    
    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 60)
            return $"{timeSpan.Seconds}s";
        else if (timeSpan.TotalMinutes < 60)
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        else
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
    }
    
    public void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
    }
    
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _httpClient?.Dispose();
    }
}
