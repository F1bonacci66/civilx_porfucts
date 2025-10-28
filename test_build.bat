@echo off
title CivilX Test Build
color 0A

echo ========================================
echo    CivilX Test Build
echo ========================================
echo.

echo Тестируем компиляцию проектов...
echo.

REM Проверяем наличие проектов
if not exist "CivilX.Shared\CivilX.Shared.csproj" (
    echo ✗ Проект CivilX.Shared не найден!
    pause
    exit /b 1
)

if not exist "dataviewer\RevitExporterAddin\RevitExporterAddin.csproj" (
    echo ✗ Проект RevitExporterAddin не найден!
    pause
    exit /b 1
)

if not exist "plugin_aut\CivilXAuthPlugin.csproj" (
    echo ✗ Проект CivilXAuthPlugin не найден!
    pause
    exit /b 1
)

echo ✓ Все проекты найдены
echo.

REM Проверяем наличие Visual Studio
echo Проверяем Visual Studio...
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" (
    echo ✓ Visual Studio 2022 Community найдена
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\Tools\VsDevCmd.bat" (
    echo ✓ Visual Studio 2022 Professional найдена
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\Tools\VsDevCmd.bat"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" (
    echo ✓ Visual Studio 2022 Enterprise найдена
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat"
) else (
    echo ✗ Visual Studio 2022 не найдена!
    echo Установите Visual Studio 2022 или измените пути в скрипте
    pause
    exit /b 1
)

echo.
echo Инициализируем среду Visual Studio...
call "%VS_PATH%"

echo.
echo Тестируем компиляцию CivilX.Shared...
cd "CivilX.Shared"
msbuild "CivilX.Shared.csproj" /p:Configuration=Release /p:Platform=x64 /verbosity:minimal
if %errorlevel% neq 0 (
    echo ✗ Ошибка компиляции CivilX.Shared!
    cd ..
    pause
    exit /b 1
)
echo ✓ CivilX.Shared скомпилирован успешно
cd ..

echo.
echo ========================================
echo    ТЕСТ ЗАВЕРШЕН УСПЕШНО!
echo ========================================
echo.
echo ✅ Все проверки пройдены
echo ✅ Visual Studio найдена
echo ✅ Компиляция работает
echo.
echo Теперь можно запустить полный скрипт:
echo build_and_install.bat
echo.
pause




