# Компиляция проекта DataViewer
Write-Host "Компиляция проекта DataViewer..." -ForegroundColor Green

# Переходим в папку проекта
$projectPath = "C:\Проекты\CivilX Сайт\Продукты\CivilX\dataviewer\RevitExporterAddin"
Set-Location $projectPath

Write-Host "Компилируем RevitExporterAddin..." -ForegroundColor Yellow
$result = MSBuild "RevitExporterAddin.csproj" /p:Configuration=Release /p:Platform=x64

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Компиляция успешна" -ForegroundColor Green
    Write-Host "Копируем файлы в CivilX_export..." -ForegroundColor Yellow
    
    $sourceDll = "bin\x64\Release\RevitExporterAddin.dll"
    $sourcePdb = "bin\x64\Release\RevitExporterAddin.pdb"
    $targetPath = "..\..\CivilX_export\"
    
    Copy-Item $sourceDll "$targetPath\RevitExporterAddin.dll" -Force
    Copy-Item $sourcePdb "$targetPath\RevitExporterAddin.pdb" -Force
    
    Write-Host "✓ Файлы обновлены" -ForegroundColor Green
    Write-Host ""
    Write-Host "Исправление CSV комментариев применено!" -ForegroundColor Cyan
    Write-Host "Теперь в CSV файлах не будет строк с символом #" -ForegroundColor Cyan
} else {
    Write-Host "✗ Ошибка компиляции" -ForegroundColor Red
}

Read-Host "Нажмите Enter для продолжения"
