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
