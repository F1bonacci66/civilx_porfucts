@echo off
title CivilX Build All MSI Versions
color 0A

echo ========================================
echo    CivilX Build All MSI Versions
echo ========================================
echo.
echo This script will create MSI installers for:
echo - Revit 2021
echo - Revit 2022  
echo - Revit 2023
echo.
echo Press any key to continue...
pause >nul

echo.
echo ========================================
echo    Building MSI for Revit 2021
echo ========================================
echo.

call "Revit 2021\build_msi_2021.bat"
if %errorlevel% neq 0 (
    echo ✗ Error building MSI for Revit 2021!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    Building MSI for Revit 2022
echo ========================================
echo.

call "Revit 2022\build_msi_2022.bat"
if %errorlevel% neq 0 (
    echo ✗ Error building MSI for Revit 2022!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    Building MSI for Revit 2023
echo ========================================
echo.

call "Revit 2023\build_msi_2023.bat"
if %errorlevel% neq 0 (
    echo ✗ Error building MSI for Revit 2023!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    ALL MSI VERSIONS CREATED!
echo ========================================
echo.
echo Created MSI files:
dir "Revit 2021\v1.5\CivilX-Revit2021-v1.5.msi" /b
dir "Revit 2022\v1.5\CivilX-Revit2022-v1.5.msi" /b
dir "Revit 2023\v1.5\CivilX-Revit2023-v1.5.msi" /b
echo.
echo Installation paths:
echo - Revit 2021: C:\ProgramData\Autodesk\Revit\Addins\2021\
echo - Revit 2022: C:\ProgramData\Autodesk\Revit\Addins\2022\
echo - Revit 2023: C:\ProgramData\Autodesk\Revit\Addins\2023\
echo.
echo All DLL files will be installed to: C:\Program Files\CivilX\CivilX_export\
echo.
pause