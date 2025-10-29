@echo off
echo Testing MSI installation...

echo Attempting to install CivilX-Revit2022-v1.5-Fixed.msi...
echo This will show the installation dialog.

msiexec /i "CivilX-Revit2022-v1.5-Fixed.msi"

if %errorlevel% equ 0 (
    echo Installation completed successfully!
    echo.
    echo Checking installation paths:
    echo.
    echo DLL files should be in: %ProgramFiles%\CivilX\CivilX_export\
    if exist "%ProgramFiles%\CivilX\CivilX_export\CivilX.Shared.dll" (
        echo ✓ CivilX.Shared.dll found
    ) else (
        echo ✗ CivilX.Shared.dll NOT found
    )
    
    if exist "%ProgramFiles%\CivilX\CivilX_export\RevitExporterAddin.dll" (
        echo ✓ RevitExporterAddin.dll found
    ) else (
        echo ✗ RevitExporterAddin.dll NOT found
    )
    
    echo.
    echo AddIn file should be in: %CommonAppData%\Autodesk\Revit\Addins\2022\
    if exist "%CommonAppData%\Autodesk\Revit\Addins\2022\RevitExporterAddin.addin" (
        echo ✓ RevitExporterAddin.addin found
    ) else (
        echo ✗ RevitExporterAddin.addin NOT found
    )
) else (
    echo Installation failed with error code: %errorlevel%
)

echo.
pause

