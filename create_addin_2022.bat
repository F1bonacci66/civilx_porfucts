@echo off
echo Creating .addin file for Revit 2022...

REM Create the Revit AddIns directory if it doesn't exist
if not exist "%CommonAppData%\Autodesk\Revit\Addins\2022" (
    echo Creating directory: %CommonAppData%\Autodesk\Revit\Addins\2022
    mkdir "%CommonAppData%\Autodesk\Revit\Addins\2022"
)

REM Create the .addin file
echo Creating CivilX.addin file...
(
echo ^<?xml version="1.0" encoding="utf-8"?^>
echo ^<RevitAddIns^>
echo   ^<!-- CivilX Unified Application - объединяет все плагины в один --^>
echo   ^<AddIn Type="Application"^>
echo     ^<AddInId^>B2C3D4E5-F6A7-8901-BCDE-F23456789012^</AddInId^>
echo     ^<Name^>CivilX^</Name^>
echo     ^<Assembly^>CivilX_export\RevitExporterAddin.dll^</Assembly^>
echo     ^<FullClassName^>RevitExporterAddin.RevitApplication^</FullClassName^>
echo     ^<VendorId^>CivilX^</VendorId^>
echo     ^<VendorDescription^>CivilX Plugin Suite - Unified Application^</VendorDescription^>
echo   ^</AddIn^>
echo ^</RevitAddIns^>
) > "%CommonAppData%\Autodesk\Revit\Addins\2022\CivilX.addin"

if exist "%CommonAppData%\Autodesk\Revit\Addins\2022\CivilX.addin" (
    echo ✓ .addin file created successfully!
    echo Location: %CommonAppData%\Autodesk\Revit\Addins\2022\CivilX.addin
) else (
    echo ✗ Failed to create .addin file
)

echo.
echo Installation complete! 
echo DLL files are in: %ProgramFiles%\CivilX\CivilX_export\
echo .addin file is in: %CommonAppData%\Autodesk\Revit\Addins\2022\
echo.
echo You can now restart Revit 2022 to see the plugin.
pause


