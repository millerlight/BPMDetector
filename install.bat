@echo off
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
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\BPM Detector.lnk'); $Shortcut.TargetPath = '%CD%\BPM_Detector.exe'; $Shortcut.WorkingDirectory = '%CD%'; $Shortcut.Description = 'BPM Detektor GUI Application'; $Shortcut.Save()"

echo.
echo Installation completed!
echo You can now run BPM Detector from your desktop.
echo.
pause
