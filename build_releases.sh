#!/bin/bash

# OmniDownloader Release Build Script
# Creates organized release packages for multiple platforms
# Compatible with bash 3.2+ (macOS default)

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project configuration
PROJECT_NAME="OmniDownloader"
PROJECT_FILE="src/OmniDownloader.csproj"
RELEASE_DIR="releases"
#VERSION=$(date +"%Y.%m.%d")
VERSION="0.9.1"

# Platform configurations (using arrays instead of associative arrays for compatibility)
PLATFORM_NAMES=("Win_x64" "Win_x86" "Win_ARM64" "OSX_x64" "OSX_ARM64" "Linux_x64" "Linux_ARM64")
RUNTIME_IDS=("win-x64" "win-x86" "win-arm64" "osx-x64" "osx-arm64" "linux-x64" "linux-arm64")

# Function to get runtime ID for a platform name
get_runtime_id() {
    local platform_name=$1
    local index=0
    
    for name in "${PLATFORM_NAMES[@]}"; do
        if [ "$name" = "$platform_name" ]; then
            echo "${RUNTIME_IDS[$index]}"
            return 0
        fi
        ((index++))
    done
    
    echo "unknown"
    return 1
}

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to clean previous builds
clean_builds() {
    print_status "Cleaning previous builds..."
    if [ -d "$RELEASE_DIR" ]; then
        rm -rf "$RELEASE_DIR"
    fi
    
    # Clean the dotnet project
    dotnet clean "$PROJECT_FILE" --configuration Release --verbosity quiet
    
    # Also manually remove obj and bin folders to ensure clean state
    local project_dir=$(dirname "$PROJECT_FILE")
    if [ -d "$project_dir/obj" ]; then
        rm -rf "$project_dir/obj"
    fi
    if [ -d "$project_dir/bin" ]; then
        rm -rf "$project_dir/bin"
    fi
    
    print_success "Cleaned previous builds"
}

# Function to build for a specific platform
build_platform() {
    local platform_name=$1
    local runtime_id=$2
    local output_dir="$RELEASE_DIR/$platform_name"
    
    print_status "Building $PROJECT_NAME for $platform_name ($runtime_id)..."
    
    # Build the project
    dotnet publish "$PROJECT_FILE" \
        --configuration Release \
        --runtime "$runtime_id" \
        --self-contained true \
        --output "$output_dir" \
        --verbosity quiet \
        --nologo \
        -p:PublishSingleFile=true \
        -p:PublishReadyToRun=true \
        -p:PublishTrimmed=true \
        -p:TrimmerSingleWarn=false \
        -p:SuppressTrimAnalysisWarnings=true
    
    if [ $? -eq 0 ]; then
        print_success "Built $platform_name successfully"
        
        # Count files in output directory
        file_count=$(find "$output_dir" -type f | wc -l)
        
        # If more than 1 file or platform is Windows/Linux, create zip
        if [ "$file_count" -gt 1 ] || [[ "$platform_name" == Win_* ]] || [[ "$platform_name" == Linux_* ]]; then
            create_zip "$platform_name" "$output_dir"
        else
            print_status "Single file output for $platform_name - keeping as folder"
        fi
        
        return 0
    else
        print_error "Failed to build $platform_name"
        return 1
    fi
}

# Function to create zip archive
create_zip() {
    local platform_name=$1
    local source_dir=$2
    local zip_name="$RELEASE_DIR/${PROJECT_NAME}-${platform_name}-v${VERSION}.zip"
    
    print_status "Creating zip archive for $platform_name..."
    
    cd "$source_dir"
    zip -r "../$(basename "$zip_name")" . > /dev/null 2>&1
    cd - > /dev/null
    
    # Remove the source directory after zipping
    rm -rf "$source_dir"
    
    print_success "Created $zip_name"
}

# Function to create README for releases
create_release_readme() {
    local readme_file="$RELEASE_DIR/README.txt"
    
    cat > "$readme_file" << EOF
OmniDownloader - Release v$VERSION
==================================

Professional cross-platform download utility with enterprise-grade UI.

Package Contents:
================

Windows Packages:
- OmniDownloader-Win_x64-v$VERSION.zip    (Windows 64-bit Intel/AMD)
- OmniDownloader-Win_x86-v$VERSION.zip    (Windows 32-bit)
- OmniDownloader-Win_ARM64-v$VERSION.zip  (Windows ARM64)

macOS Packages:
- OmniDownloader-OSX_x64/                  (macOS Intel 64-bit)
- OmniDownloader-OSX_ARM64/                (macOS Apple Silicon)

Linux Packages:
- OmniDownloader-Linux_x64-v$VERSION.zip  (Linux 64-bit)
- OmniDownloader-Linux_ARM64-v$VERSION.zip (Linux ARM64)

Installation:
============

Windows:
1. Extract the zip file
2. Run OmniDownloader.exe from command line with parameters

macOS:
1. Copy the app to /Applications or desired location
2. Run from Terminal: ./OmniDownloader <parameters>
3. Note: You may need to allow the app in System Preferences > Security

Linux:
1. Extract the zip file
2. Make executable: chmod +x OmniDownloader
3. Run: ./OmniDownloader <parameters>

Usage Example:
=============

OmniDownloader -url "https://example.com/file.zip" -path "~/Downloads/" -title "My Download"

For full documentation, see: https://github.com/alcavix/OmniDownloader

Build Date: $(date)
Version: $VERSION
EOF

    print_success "Created release README"
}

# Function to display build summary
show_summary() {
    print_status "Build Summary:"
    echo "=============="
    
    if [ -d "$RELEASE_DIR" ]; then
        for item in "$RELEASE_DIR"/*; do
            if [ -f "$item" ]; then
                size=$(du -h "$item" | cut -f1)
                echo "  📦 $(basename "$item") ($size)"
            elif [ -d "$item" ]; then
                size=$(du -sh "$item" | cut -f1)
                echo "  📁 $(basename "$item")/ ($size)"
            fi
        done
        
        total_size=$(du -sh "$RELEASE_DIR" | cut -f1)
        echo ""
        print_success "Total release size: $total_size"
        print_success "All releases ready in: $RELEASE_DIR/"
    else
        print_error "No releases were created"
    fi
}

# Main execution
main() {
    echo "========================================"
    echo "  OmniDownloader Release Builder v1.0"
    echo "========================================"
    echo ""
    
    # Check if we're in the right directory
    if [ ! -f "$PROJECT_FILE" ]; then
        print_error "Could not find $PROJECT_FILE in current directory"
        print_error "Please run this script from the project root directory"
        exit 1
    fi
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK not found. Please install .NET 8.0 SDK"
        exit 1
    fi
    
    # Create release directory
    mkdir -p "$RELEASE_DIR"
    
    # Clean previous builds
    clean_builds
    
    # Build for all platforms
    successful_builds=0
    total_builds=${#PLATFORM_NAMES[@]}
    
    for platform_name in "${PLATFORM_NAMES[@]}"; do
        runtime_id=$(get_runtime_id "$platform_name")
        
        if [ "$runtime_id" != "unknown" ]; then
            if build_platform "$platform_name" "$runtime_id"; then
                ((successful_builds++))
            fi
        else
            print_error "Unknown platform: $platform_name"
        fi
        echo ""
    done
    
    # Create release documentation
    create_release_readme
    
    # Show summary
    echo ""
    show_summary
    
    echo ""
    if [ $successful_builds -eq $total_builds ]; then
        print_success "All $total_builds platforms built successfully! 🎉"
        print_status "Release packages are ready for distribution"
    else
        print_warning "Built $successful_builds out of $total_builds platforms"
    fi
    
    echo ""
    print_status "To upload releases:"
    echo "  1. Navigate to: $RELEASE_DIR/"
    echo "  2. Upload zip files and folders to your distribution platform"
    echo "  3. Include README.txt for user instructions"
}

# Run main function
main "$@"
