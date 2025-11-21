#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Build script for BPM Detector executable
Erstellt eine ausführbare Datei der BPM Detector GUI Anwendung
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path


def check_pyinstaller():
    """Check if PyInstaller is installed"""
    try:
        import PyInstaller
        print("[OK] PyInstaller is installed")
        return True
    except ImportError:
        print("[ERROR] PyInstaller is not installed")
        print("Installing PyInstaller...")
        try:
            subprocess.check_call([sys.executable, "-m", "pip", "install", "pyinstaller"])
            print("[OK] PyInstaller installed successfully")
            return True
        except subprocess.CalledProcessError:
            print("[ERROR] Failed to install PyInstaller")
            return False


def create_spec_file():
    """Create PyInstaller spec file for the GUI application"""
    spec_content = '''# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['bpm_gui.py'],
    pathex=[],
    binaries=[],
    datas=[],
    hiddenimports=[
        'tkinter',
        'tkinter.ttk',
        'bpm_detection.bpm_detection',
        'numpy',
        'scipy',
        'PyWavelets',
        'matplotlib',
        'pydub'
    ],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='BPM_Detector',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    version='version_info.txt',
    icon='icon.ico' if os.path.exists('icon.ico') else None
)
'''
    
    with open('bpm_detector.spec', 'w', encoding='utf-8') as f:
        f.write(spec_content)
    print("[OK] Created bpm_detector.spec")


def create_version_info():
    """Create version information file for Windows executable"""
    version_content = '''# UTF-8
#
# For more details about fixed file info 'ffi' see:
# http://msdn.microsoft.com/en-us/library/ms646997.aspx
VSVersionInfo(
  ffi=FixedFileInfo(
    # filevers and prodvers should be always a tuple with four items: (1, 2, 3, 4)
    # Set not needed items to zero 0.
    filevers=(1, 0, 0, 0),
    prodvers=(1, 0, 0, 0),
    # Contains a bitmask that specifies the valid bits 'flags'r
    mask=0x3f,
    # Contains a bitmask that specifies the Boolean attributes of the file.
    flags=0x0,
    # The operating system for which this file was designed.
    # 0x4 - NT and there is no need to change it.
    OS=0x4,
    # The general type of file.
    # 0x1 - the file is an application.
    fileType=0x1,
    # The function of the file.
    # 0x0 - the function is not defined for this fileType
    subtype=0x0,
    # Creation date and time stamp.
    date=(0, 0)
  ),
  kids=[
    StringFileInfo(
      [
      StringTable(
        u'040904B0',
        [StringStruct(u'CompanyName', u'BPM Detector'),
        StringStruct(u'FileDescription', u'BPM Detektor GUI Application'),
        StringStruct(u'FileVersion', u'1.0.0.0'),
        StringStruct(u'InternalName', u'BPM_Detector'),
        StringStruct(u'LegalCopyright', u'Copyright © 2025'),
        StringStruct(u'OriginalFilename', u'BPM_Detector.exe'),
        StringStruct(u'ProductName', u'BPM Detector'),
        StringStruct(u'ProductVersion', u'1.0.0.0')])
      ]),
    VarFileInfo([VarStruct(u'Translation', [1033, 1200])])
  ]
)
'''
    
    with open('version_info.txt', 'w', encoding='utf-8') as f:
        f.write(version_content)
    print("[OK] Created version_info.txt")


def create_icon():
    """Create a simple icon file if none exists"""
    # This is a minimal ICO file creation
    # In a real scenario, you would use PIL or similar to create a proper icon
    try:
        # Check if PIL is available for icon creation
        from PIL import Image, ImageDraw, ImageFont
        
        # Create a simple icon
        size = (256, 256)
        img = Image.new('RGBA', size, (0, 128, 255, 255))  # Blue background
        draw = ImageDraw.Draw(img)
        
        # Draw a simple waveform icon
        # Music note
        draw.ellipse([80, 60, 120, 100], fill=(255, 255, 255, 255))
        draw.rectangle([110, 60, 130, 120], fill=(255, 255, 255, 255))
        draw.ellipse([110, 40, 140, 70], fill=(255, 255, 255, 255))
        
        # Beat indicators
        draw.ellipse([160, 80, 180, 100], fill=(255, 255, 255, 255))
        draw.ellipse([190, 80, 210, 100], fill=(255, 255, 255, 255))
        
        img.save('icon.ico', format='ICO', sizes=[(256, 256)])
        print("[OK] Created icon.ico")
        
    except ImportError:
        print("[WARNING] PIL not available, skipping icon creation")
        # Create an empty file so the spec doesn't fail
        with open('icon.ico', 'wb') as f:
            f.write(b'')
        print("[OK] Created placeholder icon.ico")


def build_executable():
    """Build the executable using PyInstaller"""
    print("Building executable...")
    
    try:
        # Run PyInstaller
        cmd = [
            sys.executable, "-m", "PyInstaller",
            "--clean",
            "--noconfirm",
            "bpm_detector.spec"
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode == 0:
            print("[OK] Executable built successfully")
            
            # Check if executable exists
            exe_path = Path("dist/BPM_Detector.exe")
            if exe_path.exists():
                file_size = exe_path.stat().st_size / (1024 * 1024)  # MB
                print(f"[OK] Executable created: {exe_path}")
                print(f"[OK] File size: {file_size:.1f} MB")
                return True
            else:
                print("[ERROR] Executable file not found")
                return False
        else:
            print("[ERROR] Build failed")
            print("STDOUT:", result.stdout)
            print("STDERR:", result.stderr)
            return False
            
    except Exception as e:
        print(f"[ERROR] Build error: {e}")
        return False


def clean_build_files():
    """Clean up temporary build files"""
    print("Cleaning up temporary files...")
    
    dirs_to_clean = ['build', '__pycache__']
    files_to_clean = ['bpm_detector.spec', 'version_info.txt', 'icon.ico']
    
    for dir_name in dirs_to_clean:
        if os.path.exists(dir_name):
            shutil.rmtree(dir_name)
            print(f"[OK] Removed directory: {dir_name}")
    
    for file_name in files_to_clean:
        if os.path.exists(file_name):
            os.remove(file_name)
            print(f"[OK] Removed file: {file_name}")


def create_installer_script():
    """Create a simple installer batch script"""
    installer_content = '''@echo off
echo BPM Detector Installer
echo =======================

if not exist "BPM_Detector.exe" (
    echo Error: BPM_Detector.exe not found!
    echo Please run build_exe.py first to create the executable.
    pause
    exit /b 1
)

echo Installing BPM Detector...
echo Creating desktop shortcut...

REM Create desktop shortcut
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\\Desktop\\BPM Detector.lnk'); $Shortcut.TargetPath = '%CD%\\BPM_Detector.exe'; $Shortcut.WorkingDirectory = '%CD%'; $Shortcut.Description = 'BPM Detektor GUI Application'; $Shortcut.Save()"

echo.
echo Installation completed!
echo You can now run BPM Detector from your desktop.
echo.
pause
'''
    
    with open('install.bat', 'w', encoding='utf-8') as f:
        f.write(installer_content)
    print("[OK] Created install.bat")


def main():
    """Main function to orchestrate the build process"""
    print("BPM Detector Executable Builder")
    print("=" * 40)
    print()
    
    # Check current directory
    if not os.path.exists('bpm_gui.py'):
        print("[ERROR] bpm_gui.py not found in current directory")
        print("Please run this script from the directory containing bpm_gui.py")
        return False
    
    # Check if PyInstaller is available
    if not check_pyinstaller():
        return False
    
    # Create necessary files
    print("\nCreating build files...")
    create_spec_file()
    create_version_info()
    create_icon()
    create_installer_script()
    
    # Build the executable
    print("\nBuilding executable...")
    success = build_executable()
    
    if success:
        print("\n[OK] Build completed successfully!")
        print("\nExecutable location: dist/BPM_Detector.exe")
        print("You can run this file directly on Windows without Python installed.")
        print("\nOptional: Run 'install.bat' to create a desktop shortcut.")
        
        # Ask if user wants to clean up
        try:
            response = input("\nClean up temporary build files? (y/n): ").lower().strip()
            if response in ['y', 'yes', 'ja']:
                clean_build_files()
        except KeyboardInterrupt:
            print("\nCleaning up...")
            clean_build_files()
    else:
        print("\n[ERROR] Build failed!")
        print("Check the error messages above for details.")
        return False
    
    return True


if __name__ == "__main__":
    main()