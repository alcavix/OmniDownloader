# OmniDownloader

> A sleek, cross-platform .NET application for hassle-free file downloads with a modern, professional GUI.

## Features

- **Cross-Platform**: Built with Avalonia UI, runs on Windows, macOS, and Linux
- **Professional Enterprise UI**: Clean, compact design with modern styling and professional color scheme
- **Command-Line Interface**: Launched via command line with URL and optional path parameters
- **Multi-threaded Downloads**: Supports accelerated downloads using HTTP Range headers
- **Real-time Progress**: Shows download progress with accurate percentage display and visual progress bar
- **Comprehensive Stats**: Displays download speed (KB/s, MB/s) and estimated time remaining
- **Theme-Independent Design**: Custom button styling that works consistently across light/dark themes
- **Smart Button Behavior**: Context-aware buttons that show appropriate actions based on download state
- **Responsive Layout**: Compact, space-efficient design optimized for business environments
- **Enhanced Progress Handling**: Proper 100% completion display even for fast downloads
- **Synchronized Titles**: Consistent window and form titles for professional appearance
- **Post-Action Options**: Flexible options for handling completed downloads
- **Error Handling**: Clear, user-friendly error messages with visual indicators

## Requirements

- .NET 8.0 SDK or later

### Platform-Specific Notes

- **Windows**: Executable will be `OmniDownloader.exe`
- **macOS**: May require allowing the app to run (right-click → Open) for unsigned builds
- **Linux**: Ensure X11 or Wayland display server is available

## Installation

```bash
git clone https://github.com/alcavix/OmniDownloader.git
cd OmniDownloader
dotnet build --configuration Release
````

To publish as self-contained:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained
# macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained
# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained
# Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

## Usage

```bash
OmniDownloader.exe -url <URL> [-path <FullPath>] [-title <Title>] [-description <Description>] [-post-action <Action>]
```

- `-url <URL>` (Required): The URL of the file to download
- `-path <FullPath>` (Optional): Target path for the downloaded file
  - If path is a directory (ends with `/` or `\`), the filename from URL will be used
  - If path is a file path, the file will be saved with that exact name
  - If omitted, defaults to `~/Downloads/` with filename from URL
- `-title <Title>` (Optional): Custom window title
  - If omitted, defaults to the filename (e.g., "1GB.bin")
- `-description <Description>` (Optional): Description text shown below filename
  - If omitted, no description is displayed
- `-post-action <Action>` (Optional): What to do after download completes
  - `ask` (default): Show prompt with options (Open File, Show in Folder, Just Close)
  - `open`: Automatically open the downloaded file with default application and close window after 2 seconds
  - `close`: Automatically close the application after 2 seconds

Example:
```bash
# Test progress bar with larger file
OmniDownloader.exe -url "https://ash-speed.hetzner.com/100MB.bin" -title "Speed Test" -description "Testing download progress display"
```
## Testing

Use the provided test script to verify the application works:

```bash
./test_download.sh
```

**Parameters**

* `-url <URL>` *(Required)*: File URL
* `-path <FullPath>` *(Optional)*: Save location (file or directory)
* `-title <Title>` *(Optional)*: Custom window title
* `-description <Description>` *(Optional)*: Subtitle under filename
* `-post-action <Action>` *(Optional)*: `ask` (default), `open`, or `close`

**Examples**

```bash
OmniDownloader.exe -url https://example.com/file.zip
OmniDownloader.exe -url https://example.com/file.zip -path ~/Downloads/myfile.zip
OmniDownloader.exe -url https://example.com/file.zip -post-action open
```

## Highlights

* **Professional Look**: Enterprise color scheme and typography
* **Responsive**: Adapts to light/dark themes seamlessly
* **Smart Buttons**: Context-based actions during/after download
* **Accurate Stats**: Speed, ETA, and progress always up to date

---

## License

MIT License. Use at your own risk.
This project is provided as-is for educational and demonstration purposes.

## 🙏 Acknowledgments

- **Open Source Community** - For feedback, testing, and feature requests
- **Contributors** - Everyone who helps improve this project
- **Tomer Alcavi** - Original creator and maintainer

---

<div align="center">

**⭐ If this project helps you, please consider giving it a star! ⭐**

Made with ❤️ by the open-source community

[![GitHub stars](https://img.shields.io/github/stars/alcavix/OmniDownloader.svg?style=social&label=Star)](https://github.com/alcavix/OmniDownloader)
[![GitHub forks](https://img.shields.io/github/forks/alcavix/OmniDownloader.svg?style=social&label=Fork)](https://github.com/alcavix/OmniDownloader/fork)

</div>

---

This project was created with care and attention by **Tomer Alcavi**.

If you find it useful or inspiring, you're welcome to explore and learn from it —  
but please avoid re-publishing or presenting this work (in full or in part) under a different name or without proper credit.
Keeping attribution clear helps support open, respectful collaboration. Thank you!

If you have ideas for improvements or enhancements, I’d love to hear them!  
Open collaboration and respectful feedback are always welcome.

_Made with ❤️ by Tomer Alcavi_
