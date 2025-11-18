# OmniDownloader AI Coding Instructions

## Architecture Overview

**Command-Line Driven Avalonia UI App**: OmniDownloader is a cross-platform .NET 8.0 GUI application built with Avalonia UI that launches via command-line parameters. Unlike typical desktop apps, it has NO default window - the UI only appears when invoked with proper CLI arguments.

**Key Flow**: `Program.cs` → Parse CLI args → Set `App.DownloadParameters` → Create `DownloadViewModel` → Show `DownloadWindow`

## Project Structure & Patterns

```
src/                          # All source code moved to src/ folder
├── Models/                   # Data models and CLI parsing logic
├── Services/                 # Core download logic with multi-threading
├── ViewModels/              # MVVM ViewModels using CommunityToolkit.Mvvm
├── Views/                   # XAML views with enterprise styling
├── Converters/              # Value converters (bool inversion, percent formatting)
├── Properties/AssemblyInfo.cs # Manual assembly info (GenerateAssemblyInfo=false)
└── OmniDownloader.csproj    # Project file with custom assembly config
```

## Critical Development Workflow

### Build System Requirements
- **Assembly Info**: Uses manual `Properties/AssemblyInfo.cs` with `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to avoid duplicate attribute errors
- **Multi-Platform Builds**: Use `build_releases.sh` script (references `src/OmniDownloader.csproj`)
- **Testing**: Use `test_download.sh` for integration testing with real HTTP downloads

### Command Examples
```bash
# Development build from repo root
dotnet build src/OmniDownloader.csproj --configuration Release

# Multi-platform release builds
./build_releases.sh  # Creates releases/ with platform-specific packages

# Test with real download
./test_download.sh   # Tests various CLI parameter combinations
```

## Command-Line Interface Design

**Required Parameter**: `-url <URL>` - The file to download
**Optional Parameters**:
- `-path <FullPath>` - Target file/directory (smart path resolution)
- `-title <Title>` - Custom window title (defaults to filename)
- `-description <Description>` - Subtitle text
- `-post-action <ask|open|close>` - Post-download behavior

**Path Resolution Logic** (in `DownloadParameters.cs`):
- If path ends with `/` or `\` → Directory, use URL filename
- Otherwise → Exact file path
- No path → Default to `~/Downloads/` + URL filename

## Architecture Patterns

### MVVM with Source Generators
- Uses `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]` attributes
- ViewModels inherit from `ObservableObject` for automatic INotifyPropertyChanged

### Multi-threaded Download Strategy
- `DownloadService` uses HTTP Range headers for parallel chunk downloads
- Progress reporting via events: `ProgressChanged` and `DownloadCompleted`
- Cancellation support through `CancellationTokenSource`

### Enterprise UI Styling
- Custom color scheme: `#2C3E50` (dark), `#3498DB` (primary blue), `#FAFAFA` (background)
- Consistent style classes: `.header`, `.body`, `.label`, `.primary` buttons
- Fixed window dimensions for professional appearance

## Key Components

**`DownloadParameters.cs`**: CLI argument parsing with smart path resolution and validation
**`DownloadService.cs`**: Core download engine with multi-threading and progress tracking
**`DownloadViewModel.cs`**: UI state management and command handling
**`DownloadWindow.xaml`**: Enterprise-styled UI with progress visualization

## Cross-Platform Considerations

- **Assembly Configuration**: Manual AssemblyInfo.cs prevents build conflicts across platforms
- **Native Dependencies**: Avalonia handles platform-specific rendering (Skia, Win32, X11)
- **File Paths**: Uses `Path.Combine()` and platform-agnostic path handling
- **Release Packaging**: Platform-specific builds create .zip files for distribution

## Development Guidelines

1. **Always test CLI parsing** - App crashes gracefully with usage info on invalid args
2. **Respect the enterprise aesthetic** - Use existing color scheme and typography classes
3. **Handle download failures** - Service includes comprehensive error handling and user feedback
4. **Test cross-platform builds** - Use build script to validate changes across all target platforms
5. **Maintain command-line focus** - UI is secondary to CLI functionality

## Common Tasks

**Adding new CLI parameters**: Extend `DownloadParameters.ParseFromArgs()` and update usage text
**Modifying download behavior**: Update `DownloadService` methods and event handling
**UI changes**: Follow existing style patterns in `DownloadWindow.xaml`
**Build issues**: Check assembly info configuration and clean `obj/bin` folders
