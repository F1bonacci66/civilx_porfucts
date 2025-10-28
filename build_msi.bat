@echo off
echo ========================================
echo    CivilX MSI Compilation
echo ========================================
echo.

echo Step 1: Compiling WiX file...
"C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" "C:\Проекты\CivilX Сайт\Продукты\CivilX\CivilX-Revit2023-v1.5.wxs" -out "C:\Проекты\CivilX Сайт\Продукты\CivilX\CivilX-Revit2023-v1.5.wixobj"

if %errorlevel% neq 0 (
    echo ✗ Error compiling WiX file!
    pause
    exit /b 1
)

echo ✓ WiX file compiled successfully
echo.

echo Step 2: Creating MSI file...
"C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" "C:\Проекты\CivilX Сайт\Продукты\CivilX\CivilX-Revit2023-v1.5.wixobj" -ext WixUIExtension -out "C:\Проекты\CivilX Сайт\Продукты\CivilX\CivilX-Revit2023-v1.5.msi"

if %errorlevel% neq 0 (
    echo ✗ Error creating MSI file!
    pause
    exit /b 1
)

echo ✓ MSI file created successfully!
echo.

echo Checking MSI file size...
for %%A in ("C:\Проекты\CivilX Сайт\Продукты\CivilX\CivilX-Revit2023-v1.5.msi") do echo File size: %%~zA bytes

echo.
echo ========================================
echo    MSI CREATION COMPLETED!
echo ========================================
echo.
echo MSI file location:
echo C:\Проекты\CivilX Сайт\Продукты\CivilX\CivilX-Revit2023-v1.5.msi
echo.
pause
