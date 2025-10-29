@echo off
echo ========================================
echo    CivilX MSI Compilation for Revit 2021
echo ========================================
echo.

echo Step 1: Compiling WiX file...
"C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" "v1.5\CivilX-Revit2021-v1.5.wxs" -out "v1.5\CivilX-Revit2021-v1.5.wixobj"

if %errorlevel% neq 0 (
    echo ✗ Error compiling WiX file!
    pause
    exit /b 1
)

echo ✓ WiX file compiled successfully
echo.

echo Step 2: Creating MSI file...
"C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" "v1.5\CivilX-Revit2021-v1.5.wixobj" -ext WixUIExtension -out "v1.5\CivilX-Revit2021-v1.5.msi"

if %errorlevel% neq 0 (
    echo ✗ Error creating MSI file!
    pause
    exit /b 1
)

echo ✓ MSI file created successfully!
echo.

echo Checking MSI file size...
for %%A in ("v1.5\CivilX-Revit2021-v1.5.msi") do echo File size: %%~zA bytes

echo.
echo ========================================
echo    MSI CREATION COMPLETED!
echo ========================================
echo.
echo MSI file location:
echo %~dp0v1.5\CivilX-Revit2021-v1.5.msi
echo.
echo This MSI will install:
echo - CivilX.addin to: C:\ProgramData\Autodesk\Revit\Addins\2021\
echo - All DLL files to: C:\Program Files\CivilX\CivilX_export\
echo.
pause