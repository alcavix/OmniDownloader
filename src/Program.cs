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
using OmniDownloader.Models;

namespace OmniDownloader;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Parse command line arguments
            var downloadParameters = DownloadParameters.ParseFromArgs(args);
            
            // Set the download parameters for the app
            App.DownloadParameters = downloadParameters;
            
            // Create and run the Avalonia application
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (ArgumentException ex)
        {
            // Show usage information
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Usage: OmniDownloader -url <URL> [-path <FullPath>] [-title <Title>] [-description <Description>] [-post-action <Action>]");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  -url <URL>               Required. The file to download.");
            Console.WriteLine("  -path <FullPath>         Optional. Target path (if it's a folder → use filename from URL,");
            Console.WriteLine("                           if it's a file → save with that name).");
            Console.WriteLine("  -title <Title>           Optional. Custom window title (defaults to filename).");
            Console.WriteLine("  -description <Description> Optional. Description text shown below filename.");
            Console.WriteLine("  -post-action <Action>    Optional. What to do after download (ask|open|close). Default: ask");
            Console.WriteLine("                           - ask: Show prompt with options");
            Console.WriteLine("                           - open: Automatically open the downloaded file and close window");
            Console.WriteLine("                           - close: Automatically close the application");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  OmniDownloader -url https://example.com/file.zip");
            Console.WriteLine("  OmniDownloader -url https://example.com/file.zip -path ~/Downloads/");
            Console.WriteLine("  OmniDownloader -url https://example.com/file.zip -title \"My Download\"");
            Console.WriteLine("  OmniDownloader -url https://example.com/file.zip -post-action open");
            Console.WriteLine("  OmniDownloader -url https://example.com/file.zip -post-action close");
            Console.WriteLine("  OmniDownloader -url https://example.com/file.zip -title \"Important File\" -description \"This is a critical system update\" -post-action ask");
            
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Environment.Exit(1);
        }
    }
    
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
