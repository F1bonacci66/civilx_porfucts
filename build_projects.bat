@echo off
title CivilX Build Projects
color 0A

echo ========================================
echo    CivilX Build Projects
echo ========================================
echo.

REM Проверяем наличие Visual Studio
set "VS_PATH="
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\Tools\VsDevCmd.bat" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\Tools\VsDevCmd.bat"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat"
) else (
    echo ОШИБКА: Visual Studio 2022 не найдена!
    echo Установите Visual Studio 2022 или обновите пути в скрипте.
    pause
    exit /b 1
)

echo Найдена Visual Studio: %VS_PATH%
echo.

REM Инициализируем среду Visual Studio
call "%VS_PATH%"

echo Шаг 1: Компиляция CivilX.Shared...
cd "CivilX.Shared"
msbuild "CivilX.Shared.csproj" /p:Configuration=Release /p:Platform=x64 /verbosity:minimal
if %errorlevel% neq 0 (
    echo ✗ Ошибка компиляции CivilX.Shared!
    pause
    exit /b 1
)
echo ✓ CivilX.Shared скомпилирован успешно
cd ..

echo.
echo Шаг 2: Компиляция RevitExporterAddin...
cd "dataviewer\RevitExporterAddin"
msbuild "RevitExporterAddin.csproj" /p:Configuration=Release /p:Platform=x64 /verbosity:minimal
if %errorlevel% neq 0 (
    echo ✗ Ошибка компиляции RevitExporterAddin!
    pause
    exit /b 1
)
echo ✓ RevitExporterAddin скомпилирован успешно
cd ..\..

echo.
echo Шаг 3: Компиляция CivilXAuthPlugin...
cd "plugin_aut"
msbuild "CivilXAuthPlugin.csproj" /p:Configuration=Release /p:Platform=x64 /verbosity:minimal
if %errorlevel% neq 0 (
    echo ✗ Ошибка компиляции CivilXAuthPlugin!
    pause
    exit /b 1
)
echo ✓ CivilXAuthPlugin скомпилирован успешно
cd ..

echo.
echo ========================================
echo    Компиляция завершена успешно!
echo ========================================
echo.
echo Все проекты скомпилированы в конфигурации Release x64
echo Готовые файлы находятся в папках bin\x64\Release\
echo.
pause




