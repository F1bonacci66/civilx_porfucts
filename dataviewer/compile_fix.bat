@echo off
chcp 65001
echo Компиляция проекта DataViewer...

cd /d "C:\Проекты\CivilX Сайт\Продукты\CivilX\dataviewer\RevitExporterAddin"

echo Компилируем RevitExporterAddin...
MSBuild RevitExporterAddin.csproj /p:Configuration=Release /p:Platform=x64

if %ERRORLEVEL% EQU 0 (
    echo ✓ Компиляция успешна
    echo Копируем файлы в CivilX_export...
    
    copy "bin\x64\Release\RevitExporterAddin.dll" "..\..\CivilX_export\RevitExporterAddin.dll"
    copy "bin\x64\Release\RevitExporterAddin.pdb" "..\..\CivilX_export\RevitExporterAddin.pdb"
    
    echo ✓ Файлы обновлены
    echo.
    echo Исправление CSV комментариев применено!
    echo Теперь в CSV файлах не будет строк с символом #
) else (
    echo ✗ Ошибка компиляции
)

pause
