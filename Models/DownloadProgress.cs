namespace OmniDownloader.Models;

public class DownloadProgress
{
    public long BytesReceived { get; set; }
    public long TotalBytes { get; set; }
    public double PercentComplete { get; set; }
    public double DownloadSpeedBytesPerSecond { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FormattedSpeed { get; set; } = string.Empty;
    public string FormattedETA { get; set; } = string.Empty;
}
