namespace OmniDownloader.Models;

public enum PostDownloadAction
{
    Ask,
    Open,
    Close
}

public class DownloadParameters
{
    public string Url { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PostDownloadAction PostAction { get; set; } = PostDownloadAction.Ask;
    
    public static DownloadParameters ParseFromArgs(string[] args)
    {
        var parameters = new DownloadParameters();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-url":
                    if (i + 1 < args.Length)
                    {
                        parameters.Url = args[i + 1];
                        i++; // Skip next argument as it's the value
                    }
                    break;
                    
                case "-path":
                    if (i + 1 < args.Length)
                    {
                        var path = args[i + 1];
                        
                        // Expand ~ to user home directory
                        if (path.StartsWith("~/"))
                        {
                            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                                               path.Substring(2));
                        }
                        
                        parameters.FilePath = path;
                        i++; // Skip next argument as it's the value
                    }
                    break;
                    
                case "-title":
                    if (i + 1 < args.Length)
                    {
                        parameters.Title = args[i + 1];
                        i++; // Skip next argument as it's the value
                    }
                    break;
                    
                case "-description":
                    if (i + 1 < args.Length)
                    {
                        parameters.Description = args[i + 1];
                        i++; // Skip next argument as it's the value
                    }
                    break;
                    
                case "-post-action":
                    if (i + 1 < args.Length)
                    {
                        var actionValue = args[i + 1].ToLower();
                        parameters.PostAction = actionValue switch
                        {
                            "open" => PostDownloadAction.Open,
                            "close" => PostDownloadAction.Close,
                            "ask" => PostDownloadAction.Ask,
                            _ => PostDownloadAction.Ask // Default to ask for invalid values
                        };
                        i++; // Skip next argument as it's the value
                    }
                    break;
            }
        }
        
        // Validate required parameters
        if (string.IsNullOrEmpty(parameters.Url))
        {
            throw new ArgumentException("URL parameter (-url) is required");
        }
        
        // Extract filename from URL if not provided
        if (string.IsNullOrEmpty(parameters.FileName))
        {
            try
            {
                var uri = new Uri(parameters.Url);
                parameters.FileName = Path.GetFileName(uri.LocalPath);
                
                // If no filename from URL, use a default
                if (string.IsNullOrEmpty(parameters.FileName))
                {
                    parameters.FileName = "download.bin";
                }
            }
            catch
            {
                parameters.FileName = "download.bin";
            }
        }
        
        // Determine final file path
        if (string.IsNullOrEmpty(parameters.FilePath))
        {
            // Default to Downloads folder
            parameters.FilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                "Downloads", 
                parameters.FileName);
        }
        else if (Directory.Exists(parameters.FilePath) || parameters.FilePath.EndsWith("/") || parameters.FilePath.EndsWith("\\"))
        {
            // It's a directory, append filename
            parameters.FilePath = Path.Combine(parameters.FilePath, parameters.FileName);
        }
        // Otherwise, it's already a full file path
        
        // Set default title to filename if not provided
        if (string.IsNullOrEmpty(parameters.Title))
        {
            parameters.Title = parameters.FileName;
        }
        
        return parameters;
    }
}
