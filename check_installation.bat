@echo off
title CivilX Check Installation
color 0E

echo ========================================
echo    CivilX Check Installation
echo ========================================
echo.

set "REVIT_ADDINS_PATH=C:\ProgramData\Autodesk\Revit\Addins\2023"
set "ERRORS=0"

echo Проверяем установку CivilX...
echo.

REM Проверяем папку Revit Addins
echo 1. Проверка папки Revit Addins:
if exist "%REVIT_ADDINS_PATH%" (
    echo ✓ Папка Revit Addins существует: %REVIT_ADDINS_PATH%
) else (
    echo ✗ Папка Revit Addins не найдена: %REVIT_ADDINS_PATH%
    set /a ERRORS+=1
)

echo.
echo 2. Проверка файлов в Revit Addins:
if exist "%REVIT_ADDINS_PATH%\CivilX.addin" (
    echo ✓ CivilX.addin найден
) else (
    echo ✗ CivilX.addin не найден
    set /a ERRORS+=1
)

echo.
echo 3. Проверка папки CivilX_export:
if exist "CivilX_export" (
    echo ✓ Папка CivilX_export существует
) else (
    echo ✗ Папка CivilX_export не найдена
    set /a ERRORS+=1
)

echo.
echo 4. Проверка файлов в CivilX_export:
if exist "CivilX_export\CivilX.Shared.dll" (
    echo ✓ CivilX.Shared.dll найден
) else (
    echo ✗ CivilX.Shared.dll не найден
    set /a ERRORS+=1
)

if exist "CivilX_export\RevitExporterAddin.dll" (
    echo ✓ RevitExporterAddin.dll найден
) else (
    echo ✗ RevitExporterAddin.dll не найден
    set /a ERRORS+=1
)

if exist "CivilX_export\CivilXAuthPlugin.dll" (
    echo ✓ CivilXAuthPlugin.dll найден
) else (
    echo ✗ CivilXAuthPlugin.dll не найден
    set /a ERRORS+=1
)

echo.
echo 5. Проверка конфигурации .addin файла:
if exist "%REVIT_ADDINS_PATH%\CivilX.addin" (
    findstr /C:"CivilX_export" "%REVIT_ADDINS_PATH%\CivilX.addin" >nul
    if %errorlevel% equ 0 (
        echo ✓ .addin файл настроен на использование папки CivilX_export
    ) else (
        echo ⚠️ .addin файл может быть настроен неправильно
        echo Проверьте пути в файле CivilX.addin
    )
)

echo.
echo 6. Проверка прав доступа:
echo test > "%REVIT_ADDINS_PATH%\test.tmp" 2>nul
if %errorlevel% equ 0 (
    del "%REVIT_ADDINS_PATH%\test.tmp" >nul 2>&1
    echo ✓ Права доступа к папке Revit Addins подтверждены
) else (
    echo ✗ Нет прав доступа к папке Revit Addins
    echo Запустите скрипт от имени администратора
    set /a ERRORS+=1
)

echo.
echo ========================================
echo    РЕЗУЛЬТАТ ПРОВЕРКИ
echo ========================================
echo.

if %ERRORS% equ 0 (
    echo ✅ ВСЕ ПРОВЕРКИ ПРОЙДЕНЫ УСПЕШНО!
    echo.
    echo CivilX готов к использованию:
    echo 1. Перезапустите Revit
    echo 2. Найдите вкладку "CivilX" в ленте
    echo 3. Нажмите кнопку "Data Manager"
) else (
    echo ❌ НАЙДЕНО %ERRORS% ОШИБОК!
    echo.
    echo Для исправления ошибок:
    echo 1. Запустите build_and_install.bat
    echo 2. Или запустите отдельные скрипты:
    echo    - build_projects.bat (компиляция)
    echo    - update_files.bat (обновление файлов)
    echo    - install_to_revit.bat (установка в Revit)
)

echo.
pause




