@echo off
echo Building FIXED MSI for CivilX Revit 2022 Plugin...

REM Set WiX Toolset path
set WIX_PATH="C:\Program Files (x86)\WiX Toolset v3.11\bin"

REM Check if WiX is installed
%WIX_PATH%\candle.exe -? >nul 2>nul
if %errorlevel% neq 0 (
    echo WiX Toolset not found at %WIX_PATH%
    echo Please install WiX Toolset v3.11 or later.
    echo Download from: https://wixtoolset.org/releases/
    pause
    exit /b 1
)

REM Clean previous build files
if exist "CivilX-Revit2022-v1.5-Fixed.wixobj" del "CivilX-Revit2022-v1.5-Fixed.wixobj"
if exist "CivilX-Revit2022-v1.5-Fixed.wixpdb" del "CivilX-Revit2022-v1.5-Fixed.wixpdb"
if exist "CivilX-Revit2022-v1.5-Fixed.msi" del "CivilX-Revit2022-v1.5-Fixed.msi"

echo Compiling WiX source file...
%WIX_PATH%\candle.exe -out "CivilX-Revit2022-v1.5-Fixed.wixobj" "CivilX-Revit2022-v1.5-Fixed.wxs"
if %errorlevel% neq 0 (
    echo Failed to compile WiX source file.
    pause
    exit /b 1
)

echo Linking MSI file...
%WIX_PATH%\light.exe -out "CivilX-Revit2022-v1.5-Fixed.msi" -pdbout "CivilX-Revit2022-v1.5-Fixed.wixpdb" "CivilX-Revit2022-v1.5-Fixed.wixobj"
if %errorlevel% neq 0 (
    echo Failed to create MSI file.
    pause
    exit /b 1
)

echo MSI file created successfully: CivilX-Revit2022-v1.5-Fixed.msi
echo.
echo Installation paths:
echo - DLL files: %%ProgramFiles%%\CivilX\CivilX_export\
echo - AddIn file: %%CommonAppData%%\Autodesk\Revit\Addins\2022\
echo.
pause

