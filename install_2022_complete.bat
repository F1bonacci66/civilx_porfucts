@echo off
echo Installing CivilX Revit 2022 Plugin...

echo Step 1: Installing DLL files...
msiexec /i "CivilX-Revit2022-v1.5-Simple.msi" /quiet

if %errorlevel% equ 0 (
    echo ✓ DLL files installed successfully!
) else (
    echo ✗ Failed to install DLL files. Error code: %errorlevel%
    pause
    exit /b 1
)

echo.
echo Step 2: Creating .addin file...
call create_addin_2022.bat

echo.
echo Installation completed!
echo.
echo Files installed:
echo - DLL files: %ProgramFiles%\CivilX\CivilX_export\
echo - .addin file: %CommonAppData%\Autodesk\Revit\Addins\2022\
echo.
echo Please restart Revit 2022 to see the plugin.
pause


