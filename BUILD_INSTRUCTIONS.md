# BPM Detector - Visual Studio Build Instructions

## Overview
This repository contains three implementations of the BPM Detector:
1. **Original Python Version** (`bpm_gui.py`, `bpm_detection/bpm_detection.py`)
2. **C# Version** (`bpm-detector-csharp/`) - Recommended for Visual Studio
3. **C++ Version** (`bpm-detector-cpp/`, updated existing `bpm-detector/`)

## Project Structures

### C# Implementation (Recommended)
```
bpm-detector-csharp/
├── BPMDetector.sln                    # Visual Studio Solution
└── BPMDetector/
    ├── BPMDetector.csproj             # Project file with dependencies
    ├── Program.cs                     # Main entry point
    ├── MainForm.cs                    # Windows Forms GUI
    ├── BPMDetector.cs                 # Core BPM detection algorithm
    └── AudioFileReader.cs             # Audio file handling (WAV/MP3)
```

### C++ Implementation
```
bpm-detector/                           # Your existing Visual Studio project
├── bpm-detector.sln                   # Solution file
└── bpm-detector/
    ├── bpm-detector.vcxproj           # Updated project file
    ├── main.cpp                       # Console application main
    └── (needs BPMDetector.h/cpp)

bpm-detector-cpp/                       # Core C++ implementation
├── BPMDetector.h                      # Header file
└── BPMDetector.cpp                    # Implementation file
```

## C# Implementation - Build Instructions

### Prerequisites
- Visual Studio 2022 or later
- .NET 6.0 or later
- NuGet Package Manager (included with Visual Studio)

### Steps to Build
1. **Open the solution:**
   ```
   File → Open → Project/Solution
   Select: bpm-detector-csharp/BPMDetector.sln
   ```

2. **Restore NuGet packages:**
   - Right-click on solution → "Restore NuGet Packages"
   - Or run: `dotnet restore` in Package Manager Console

3. **Build the project:**
   - Build → Build Solution (Ctrl+Shift+B)
   - Or right-click project → "Build"

4. **Run the application:**
   - Press F5 to debug
   - Or right-click project → "Debug" → "Start New Instance"

### Dependencies (Auto-installed via NuGet)
- **MathNet.Numerics** v5.0.0 - Signal processing and mathematics
- **NAudio** v2.2.1 - Audio file reading (WAV/MP3 support)
- **System.IO.Packaging** v7.0.0 - File handling utilities

### Output
- Executable: `bin/Debug/net6.0-windows/BPMDetector.exe`
- For redistribution: Use "Publish" feature to create standalone executable

## C++ Implementation - Build Instructions

### Prerequisites
- Visual Studio 2022 with C++ workload
- Windows SDK 10.0 or later

### Steps to Build
1. **Open the existing solution:**
   ```
   File → Open → Project/Solution
   Select: bpm-detector/bpm-detector.sln
   ```

2. **Ensure files are properly linked:**
   - `main.cpp` is already included in the project
   - Header file path is configured in project properties

3. **Build the project:**
   - Build → Build Solution (Ctrl+Shift+B)
   - Choose platform: x64 (recommended) or Win32

4. **Run the console application:**
   - Press F5 to debug
   - Or use command line: `bpm-detector.exe <audio_file>`

### Usage Example
```cmd
cd bpm-detector\x64\Release
bpm-detector.exe "C:\Music\song.wav"
bpm-detector.exe "C:\Music\song.mp3" -v
```

## C++ Project Configuration
The updated `.vcxproj` file includes:
- Proper include directories for the C++ source files
- Console subsystem configuration
- Unicode character set support
- Optimizations for Release builds

## Testing Both Implementations

### Test Files
Both implementations support:
- **WAV files** (uncompressed PCM)
- **MP3 files** (via NAudio in C#, basic support in C++)

### Test Commands

#### C# Version
```cmd
# After building, run the executable directly
BPMDetector.exe
# Use the GUI to select files
```

#### C++ Version
```cmd
# Command line usage
bpm-detector.exe "path/to/audio.wav"
bpm-detector.exe "path/to/audio.mp3" -v
```

## Performance Comparison

| Feature | Python | C# | C++ |
|---------|--------|----|-----|
| Development Speed | Fast | Fast | Medium |
| Runtime Performance | Medium | Good | Excellent |
| Executable Size | Large | Medium | Small |
| Dependencies | Many | Few | Minimal |
| GUI Framework | tkinter | Windows Forms | Console/Win32 |
| Audio Support | Full | Full | WAV + Limited MP3 |

## Deployment Options

### C# Deployment
1. **Visual Studio Publish:**
   - Right-click project → Publish
   - Choose "Folder" → Create self-contained executable

2. **Command Line:**
   ```cmd
   dotnet publish -c Release -r win-x64 --self-contained
   ```

### C++ Deployment
1. **Release Build:**
   - Configuration: Release, Platform: x64
   - Build solution
   - Copy executable and required DLLs

## Troubleshooting

### C# Issues
- **NuGet restore fails:** Check internet connection and NuGet package sources
- **Runtime errors:** Ensure .NET 6.0 runtime is installed
- **Audio file errors:** Check file format compatibility

### C++ Issues
- **Linker errors:** Verify header file paths in project properties
- **Audio file errors:** WAV format works best, MP3 support is limited
- **Console window:** This is a console application by design

## Recommendations

### For Production Use: C# Version
- Better audio library support (NAudio)
- Professional Windows GUI
- Easier maintenance and debugging
- Cross-platform potential (.NET Core)

### For Maximum Performance: C++ Version
- Fastest execution
- Smallest executable size
- No runtime dependencies
- Best for command-line automation

### For Quick Development: Python Version
- Fastest to modify and test
- Rich ecosystem of audio processing libraries
- Easy to add new features

## Next Steps
1. Choose your preferred implementation
2. Follow the build instructions above
3. Test with your audio files
4. Modify and extend as needed