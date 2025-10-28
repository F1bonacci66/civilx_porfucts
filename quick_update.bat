@echo off
title CivilX Quick Update
color 0F

echo ========================================
echo    CivilX Quick Update
echo ========================================
echo.
echo Быстрое обновление файлов без полной пересборки
echo (используется, если проекты уже скомпилированы)
echo.

REM Проверяем наличие скомпилированных файлов
echo Проверяем наличие скомпилированных файлов...

if not exist "CivilX.Shared\bin\x64\Release\CivilX.Shared.dll" (
    echo ✗ CivilX.Shared.dll не найден!
    echo Сначала запустите build_projects.bat
    pause
    exit /b 1
)

if not exist "dataviewer\RevitExporterAddin\bin\x64\Release\RevitExporterAddin.dll" (
    echo ✗ RevitExporterAddin.dll не найден!
    echo Сначала запустите build_projects.bat
    pause
    exit /b 1
)

if not exist "plugin_aut\bin\x64\Release\CivilXAuthPlugin.dll" (
    echo ✗ CivilXAuthPlugin.dll не найден!
    echo Сначала запустите build_projects.bat
    pause
    exit /b 1
)

echo ✓ Все файлы найдены
echo.

echo Обновляем файлы...
call update_files.bat
if %errorlevel% neq 0 (
    echo ✗ Ошибка обновления файлов!
    pause
    exit /b 1
)

echo.
echo Устанавливаем в Revit...
call install_to_revit.bat
if %errorlevel% neq 0 (
    echo ✗ Ошибка установки в Revit!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    БЫСТРОЕ ОБНОВЛЕНИЕ ЗАВЕРШЕНО!
echo ========================================
echo.
echo ✅ Файлы обновлены и установлены в Revit
echo Перезапустите Revit для применения изменений
echo.
pause




