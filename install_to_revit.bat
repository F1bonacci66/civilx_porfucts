@echo off
title CivilX Install to Revit
color 0C

echo ========================================
echo    CivilX Install to Revit
echo ========================================
echo.

REM Определяем путь к папке Revit Addins
set "REVIT_ADDINS_PATH=C:\ProgramData\Autodesk\Revit\Addins\2023"

echo Проверяем папку Revit Addins: %REVIT_ADDINS_PATH%

REM Проверяем существование папки Revit Addins
if not exist "%REVIT_ADDINS_PATH%" (
    echo Создаем папку Revit Addins...
    mkdir "%REVIT_ADDINS_PATH%"
    echo ✓ Папка Revit Addins создана
) else (
    echo ✓ Папка Revit Addins существует
)

REM Проверяем наличие .addin файла
if not exist "CivilX_export\CivilX.addin" (
    echo ✗ ОШИБКА: Файл CivilX.addin не найден в CivilX_export!
    echo Сначала запустите update_files.bat
    pause
    exit /b 1
)

echo ✓ Файл CivilX.addin найден

REM Проверяем права доступа
echo Проверяем права доступа к папке Revit Addins...
echo test > "%REVIT_ADDINS_PATH%\test.tmp" 2>nul
if %errorlevel% neq 0 (
    echo ✗ ОШИБКА: Нет прав доступа к папке Revit Addins!
    echo Запустите скрипт от имени администратора.
    pause
    exit /b 1
) else (
    del "%REVIT_ADDINS_PATH%\test.tmp" >nul 2>&1
    echo ✓ Права доступа подтверждены
)

echo.
echo Устанавливаем CivilX в Revit...

REM Удаляем старые файлы CivilX, если они существуют
if exist "%REVIT_ADDINS_PATH%\CivilX.addin" (
    echo Удаляем старую версию CivilX...
    del "%REVIT_ADDINS_PATH%\CivilX.addin" >nul 2>&1
    echo ✓ Старая версия удалена
)

if exist "%REVIT_ADDINS_PATH%\CivilX.Shared.dll" (
    del "%REVIT_ADDINS_PATH%\CivilX.Shared.dll" >nul 2>&1
)

if exist "%REVIT_ADDINS_PATH%\RevitExporterAddin.dll" (
    del "%REVIT_ADDINS_PATH%\RevitExporterAddin.dll" >nul 2>&1
)

if exist "%REVIT_ADDINS_PATH%\CivilXAuthPlugin.dll" (
    del "%REVIT_ADDINS_PATH%\CivilXAuthPlugin.dll" >nul 2>&1
)

REM Копируем .addin файл
echo Копируем CivilX.addin...
copy "CivilX_export\CivilX.addin" "%REVIT_ADDINS_PATH%\" >nul
if %errorlevel% equ 0 (
    echo ✓ CivilX.addin установлен
) else (
    echo ✗ Ошибка установки CivilX.addin
    pause
    exit /b 1
)

REM Проверяем содержимое .addin файла
echo Проверяем конфигурацию .addin файла...
findstr /C:"CivilX_export" "%REVIT_ADDINS_PATH%\CivilX.addin" >nul
if %errorlevel% equ 0 (
    echo ✓ .addin файл настроен на использование папки CivilX_export
    echo ✓ Все DLL файлы должны оставаться в папке CivilX_export
) else (
    echo ⚠️ ВНИМАНИЕ: .addin файл может быть настроен неправильно
    echo Проверьте пути в файле CivilX.addin
)

echo.
echo ========================================
echo    Установка завершена!
echo ========================================
echo.
echo CivilX успешно установлен в Revit!
echo.
echo Следующие шаги:
echo 1. Перезапустите Revit
echo 2. Найдите вкладку "CivilX" в ленте
echo 3. Нажмите кнопку "Data Manager"
echo.
echo Если плагин не появился:
echo - Проверьте, что все файлы в папке CivilX_export
echo - Убедитесь, что пути в .addin файле правильные
echo - Запустите Revit от имени администратора
echo.
pause




