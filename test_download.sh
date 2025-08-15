#!/bin/bash

# Test script for OmniDownloader application

echo "Building OmniDownloader..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo "Build successful!"
echo
echo "Testing OmniDownloader with various configurations..."

# Test 1: Basic download with default title (filename)
echo "=== Test 1: Default title (filename) ==="
TEST_URL="https://httpbin.org/json"
TEST_PATH="~/Downloads/test_basic.json"

echo "Running: dotnet run -- -url \"$TEST_URL\" -path \"$TEST_PATH\""
dotnet run -- -url "$TEST_URL" -path "$TEST_PATH"
echo

# Test 2: Custom title only
echo "=== Test 2: Custom title only ==="
TEST_PATH="~/Downloads/test_custom_title.json"
echo "Running: dotnet run -- -url \"$TEST_URL\" -path \"$TEST_PATH\" -title \"My Custom Download\""
dotnet run -- -url "$TEST_URL" -path "$TEST_PATH" -title "My Custom Download"
echo

# Test 3: Custom title and description
echo "=== Test 3: Custom title and description ==="
TEST_PATH="~/Downloads/test_full_custom.json"
echo "Running: dotnet run -- -url \"$TEST_URL\" -path \"$TEST_PATH\" -title \"Important API Data\" -description \"Downloading JSON response from httpbin.org for testing purposes\""
dotnet run -- -url "$TEST_URL" -path "$TEST_PATH" -title "Important API Data" -description "Downloading JSON response from httpbin.org for testing purposes"
echo

# Test 4: Post-action examples
echo "=== Test 4: Post-action features ==="

echo "Testing -post-action ask (shows prompt):"
TEST_PATH="~/Downloads/test_post_ask.json"
dotnet run -- -url "$TEST_URL" -path "$TEST_PATH" -title "Ask Action Test" -post-action ask
echo

echo "Testing -post-action close (auto-closes):"
TEST_PATH="~/Downloads/test_post_close.json"
dotnet run -- -url "$TEST_URL" -path "$TEST_PATH" -title "Auto-Close Test" -post-action close
echo

echo "Testing -post-action open (opens file):"
TEST_PATH="~/Downloads/test_post_open.json"
dotnet run -- -url "$TEST_URL" -path "$TEST_PATH" -title "Auto-Open Test" -post-action open
echo

echo "All tests completed. Check your Downloads folder for the downloaded files."
echo
echo "Post-action options tested:"
echo "  - ask: Shows prompt with 'Open File', 'Show in Folder', 'Just Close' options"
echo "  - close: Automatically closes the application after 2 seconds"
echo "  - open: Automatically opens the downloaded file with the default application and closes the window after 2 seconds"
