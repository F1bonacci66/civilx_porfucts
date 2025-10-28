@echo off
title CivilX Update Files
color 0B

echo ========================================
echo    CivilX Update Files
echo ========================================
echo.

REM Проверяем существование папки CivilX_export
if not exist "CivilX_export" (
    echo Создаем папку CivilX_export...
    mkdir "CivilX_export"
    echo ✓ Папка CivilX_export создана
)

REM Проверяем существование скомпилированных файлов
echo Проверяем наличие скомпилированных файлов...

if not exist "CivilX.Shared\bin\x64\Release\CivilX.Shared.dll" (
    echo ✗ ОШИБКА: CivilX.Shared.dll не найден!
    echo Сначала запустите build_projects.bat
    pause
    exit /b 1
)

if not exist "dataviewer\RevitExporterAddin\bin\x64\Release\RevitExporterAddin.dll" (
    echo ✗ ОШИБКА: RevitExporterAddin.dll не найден!
    echo Сначала запустите build_projects.bat
    pause
    exit /b 1
)

if not exist "plugin_aut\bin\x64\Release\CivilXAuthPlugin.dll" (
    echo ✗ ОШИБКА: CivilXAuthPlugin.dll не найден!
    echo Сначала запустите build_projects.bat
    pause
    exit /b 1
)

echo ✓ Все файлы найдены
echo.

echo Копируем файлы в CivilX_export...

REM Копируем основные DLL файлы
echo Копируем CivilX.Shared.dll...
copy "CivilX.Shared\bin\x64\Release\CivilX.Shared.dll" "CivilX_export\" >nul
if %errorlevel% equ 0 (
    echo ✓ CivilX.Shared.dll скопирован
) else (
    echo ✗ Ошибка копирования CivilX.Shared.dll
    pause
    exit /b 1
)

echo Копируем RevitExporterAddin.dll...
copy "dataviewer\RevitExporterAddin\bin\x64\Release\RevitExporterAddin.dll" "CivilX_export\" >nul
if %errorlevel% equ 0 (
    echo ✓ RevitExporterAddin.dll скопирован
) else (
    echo ✗ Ошибка копирования RevitExporterAddin.dll
    pause
    exit /b 1
)

echo Копируем CivilXAuthPlugin.dll...
copy "plugin_aut\bin\x64\Release\CivilXAuthPlugin.dll" "CivilX_export\" >nul
if %errorlevel% equ 0 (
    echo ✓ CivilXAuthPlugin.dll скопирован
) else (
    echo ✗ Ошибка копирования CivilXAuthPlugin.dll
    pause
    exit /b 1
)

REM Копируем PDB файлы (для отладки)
echo Копируем отладочные файлы...
copy "CivilX.Shared\bin\x64\Release\CivilX.Shared.pdb" "CivilX_export\" >nul 2>&1
copy "dataviewer\RevitExporterAddin\bin\x64\Release\RevitExporterAddin.pdb" "CivilX_export\" >nul 2>&1
copy "plugin_aut\bin\x64\Release\CivilXAuthPlugin.pdb" "CivilX_export\" >nul 2>&1
echo ✓ PDB файлы скопированы

REM Копируем иконку, если она существует
if exist "plugin_aut\main_icon.jpg" (
    copy "plugin_aut\main_icon.jpg" "CivilX_export\" >nul 2>&1
    echo ✓ Иконка скопирована
)

REM Проверяем наличие .addin файла
if not exist "CivilX_export\CivilX.addin" (
    echo ⚠️ ВНИМАНИЕ: Файл CivilX.addin не найден в CivilX_export!
    echo Убедитесь, что файл существует и содержит правильные пути.
)

echo.
echo ========================================
echo    Обновление файлов завершено!
echo ========================================
echo.
echo Файлы в папке CivilX_export:
dir "CivilX_export" /b
echo.
echo Следующий шаг: Запустите install_to_revit.bat
echo.
pause




