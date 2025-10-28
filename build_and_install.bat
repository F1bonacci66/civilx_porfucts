@echo off
title CivilX Build and Install
color 0D

echo ========================================
echo    CivilX Build and Install
echo ========================================
echo.
echo Этот скрипт автоматически:
echo 1. Скомпилирует все проекты
echo 2. Обновит файлы в папке CivilX_export
echo 3. Установит плагин в Revit
echo.
echo Нажмите любую клавишу для продолжения...
pause >nul

echo.
echo ========================================
echo    ШАГ 1: КОМПИЛЯЦИЯ ПРОЕКТОВ
echo ========================================
echo.

call build_projects.bat
if %errorlevel% neq 0 (
    echo ✗ Ошибка компиляции проектов!
    echo Проверьте ошибки выше и исправьте их.
    pause
    exit /b 1
)

echo.
echo ========================================
echo    ШАГ 2: ОБНОВЛЕНИЕ ФАЙЛОВ
echo ========================================
echo.

call update_files.bat
if %errorlevel% neq 0 (
    echo ✗ Ошибка обновления файлов!
    echo Проверьте ошибки выше и исправьте их.
    pause
    exit /b 1
)

echo.
echo ========================================
echo    ШАГ 3: УСТАНОВКА В REVIT
echo ========================================
echo.

call install_to_revit.bat
if %errorlevel% neq 0 (
    echo ✗ Ошибка установки в Revit!
    echo Проверьте ошибки выше и исправьте их.
    pause
    exit /b 1
)

echo.
echo ========================================
echo    ВСЕ ГОТОВО!
echo ========================================
echo.
echo ✅ CivilX успешно скомпилирован и установлен!
echo.
echo 📋 Что было сделано:
echo   • Скомпилированы все проекты (Release x64)
echo   • Обновлены файлы в папке CivilX_export
echo   • Установлен плагин в Revit Addins
echo.
echo 🚀 Следующие шаги:
echo   1. Перезапустите Revit
echo   2. Найдите вкладку "CivilX" в ленте
echo   3. Нажмите кнопку "Data Manager"
echo.
echo 🔧 Если что-то не работает:
echo   • Проверьте, что все файлы в папке CivilX_export
echo   • Убедитесь, что пути в .addin файле правильные
echo   • Запустите Revit от имени администратора
echo.
echo 📁 Файлы установки находятся в: CivilX_export\
echo.
pause




