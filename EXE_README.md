# BPM Detector - Executable Version

## Overview
This directory contains a standalone executable version of the BPM Detector application. The executable allows you to detect the Beats Per Minute (BPM) from audio files (WAV and MP3) without requiring Python to be installed on your system.

## Files Included
- `BPM_Detector.exe` - The main executable file (246.5 MB)
- `install.bat` - Optional installer script to create desktop shortcut
- `EXE_README.md` - This documentation file

## How to Use

### Simple Workflow
1. Double-click on `BPM_Detector.exe` to launch the application
2. Click "Durchsuchen..." to select an audio file (WAV or MP3)
3. **Automatic Analysis**: The BPM analysis starts automatically after file selection
4. **No Extra Steps**: Results appear directly in the interface

### Features
- **One-Click Analysis**: No need to press additional buttons
- **Smart Directory Memory**: Remembers the last used directory for file selection
- **Clean Interface**: Minimal design with only essential controls
- **Silent Operation**: No command line windows or popups during normal operation
- **Precise Results**: BPM displayed with 2 decimal places (e.g., "120.50 BPM")

## System Requirements
- Windows 10 or later (64-bit)
- No Python installation required
- No additional dependencies needed

## Supported File Formats
- **WAV files** (.wav) - Recommended for best results
- **MP3 files** (.mp3) - Also supported

## Usage Features

### Automatic Processing
- Analysis begins automatically 500ms after file selection
- No manual button pressing required
- Progress indication during analysis
- Results display immediately when complete

### Directory Memory
- Remembers the last directory where you selected a file
- Next file selection opens in the same directory
- Makes it easy to process multiple files from the same folder

### No Popups
- Success messages appear directly in the interface
- Only error messages trigger popups (when needed)
- Clean, distraction-free user experience

## Interface Layout
```
┌─────────────────────────────────────┐
│            BPM Detector             │
├─────────────────────────────────────┤
│ Audio Datei auswählen:              │
│ [File path display]  [Durchsuchen...] │
├─────────────────────────────────────┤
│ Ergebnisse:                         │
│ Erkannte BPM: [120.50] BPM         │
│ [████████████████████████]          │
│ Status: Analyse abgeschlossen       │
└─────────────────────────────────────┘
```

## Usage Tips
1. **Audio Quality**: Use high-quality audio files for better BPM detection
2. **File Selection**: Click "Durchsuchen..." and select any audio file
3. **BPM Range**: The detector works best for BPM between 60-200
4. **Clear Audio**: Audio with clear, consistent beats works better
5. **Directory**: The application remembers where you last selected files

## Troubleshooting

### Common Issues
- **"BPM could not be detected"**: Try a different audio file with clearer beats
- **Slow processing**: Large audio files may take time; be patient
- **File format errors**: Ensure you're using WAV or MP3 files

### No Command Line Windows
- The application is configured to run without showing command line windows
- Only the GUI interface appears when you run the executable
- Background processing is completely silent

### System Requirements
- Minimum 2GB RAM for large audio files
- 500MB free disk space for the application
- Modern Windows system (64-bit recommended)

## Technical Details
- Built with PyInstaller for Windows compatibility
- Includes all necessary Python libraries and dependencies
- Self-contained executable with no external dependencies
- Approximately 246.5 MB in size due to bundled dependencies
- Console window suppressed for clean operation

## Limitations
- May not detect BPM accurately for:
  - Very quiet audio files
  - Audio with irregular or inconsistent beats
  - Music with heavy effects or distortion
  - Very short audio clips (less than 3 seconds)

## Version Information
- Application: BPM Detector GUI v1.0 (Final)
- Build Date: November 2025
- Executable Size: 246.5 MB
- Python Dependencies: Included
- Features: Auto-analysis, Directory memory, No popups, 2-decimal precision

---

**Note**: This executable contains all necessary components and can run on any Windows system without additional software installation. The interface has been optimized for a clean, simple user experience.