# 📦 ИНСТРУКЦИЯ ПО СБОРКЕ MSI ФАЙЛОВ

## 🎯 Обзор

Эта инструкция описывает процесс создания MSI установочных файлов для CivilX плагина с использованием WiX Toolset.

## 📋 Требования

### Обязательные компоненты:
- **WiX Toolset v3.11** или новее
- **Visual Studio 2022** (для компиляции проектов)
- **.NET Framework 4.8**
- **Права администратора**

### Установка WiX Toolset:
1. Скачайте WiX Toolset с официального сайта: https://wixtoolset.org/releases/
2. Установите в стандартную папку: `C:\Program Files (x86)\WiX Toolset v3.11\`

## 🏗️ Процесс сборки MSI

### Шаг 1: Подготовка файлов
```batch
# Сначала скомпилируйте все проекты
build_projects.bat

# Обновите файлы в папке экспорта
update_files.bat
```

### Шаг 2: Создание WiX файла
Создайте файл `CivilX-Revit2021-v1.5.wxs` на основе существующего файла для Revit 2022.

### Шаг 3: Компиляция MSI
```batch
# Используйте готовый скрипт или выполните команды вручную
build_msi_2021.bat
```

## 📁 Структура WiX файла

### Основные секции:
1. **Product** - информация о продукте
2. **Package** - настройки установщика
3. **Directory** - структура папок
4. **ComponentGroup** - компоненты для установки
5. **File** - файлы для копирования

### Пример структуры:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" 
           Name="CivilX Revit 2021 Plugin" 
           Language="1033" 
           Version="1.5.0.0" 
           Manufacturer="CivilX" 
           UpgradeCode="[UNIQUE-GUID]">
    
    <Package InstallerVersion="200" 
             Compressed="yes" 
             InstallScope="perMachine" 
             Description="CivilX Revit 2021 Plugin Installer" />

    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="CivilX">
          <Directory Id="CivilXExportFolder" Name="CivilX_export" />
        </Directory>
      </Directory>
      <Directory Id="CommonAppDataFolder">
        <Directory Id="AutodeskFolder" Name="Autodesk">
          <Directory Id="RevitFolder" Name="Revit">
            <Directory Id="AddinsFolder" Name="Addins">
              <Directory Id="Revit2021Folder" Name="2021" />
            </Directory>
          </Directory>
        </Directory>
      </Directory>
    </Directory>
  </Product>
</Wix>
```

## 🔧 Создание скрипта для Revit 2021

### Создайте файл `build_msi_2021.bat`:

```batch
@echo off
echo Building MSI for CivilX Revit 2021 Plugin...

REM Set WiX Toolset path
set WIX_PATH="C:\Program Files (x86)\WiX Toolset v3.11\bin"

REM Check if WiX is installed
%WIX_PATH%\candle.exe -? >nul 2>nul
if %errorlevel% neq 0 (
    echo WiX Toolset not found at %WIX_PATH%
    echo Please install WiX Toolset v3.11 or later.
    echo Download from: https://wixtoolset.org/releases/
    pause
    exit /b 1
)

REM Clean previous build files
if exist "CivilX-Revit2021-v1.5.wixobj" del "CivilX-Revit2021-v1.5.wixobj"
if exist "CivilX-Revit2021-v1.5.wixpdb" del "CivilX-Revit2021-v1.5.wixpdb"
if exist "CivilX-Revit2021-v1.5.msi" del "CivilX-Revit2021-v1.5.msi"

echo Compiling WiX source file...
%WIX_PATH%\candle.exe -out "CivilX-Revit2021-v1.5.wixobj" "CivilX-Revit2021-v1.5.wxs"
if %errorlevel% neq 0 (
    echo Failed to compile WiX source file.
    pause
    exit /b 1
)

echo Linking MSI file...
%WIX_PATH%\light.exe -out "CivilX-Revit2021-v1.5.msi" -pdbout "CivilX-Revit2021-v1.5.wixpdb" "CivilX-Revit2021-v1.5.wixobj"
if %errorlevel% neq 0 (
    echo Failed to create MSI file.
    pause
    exit /b 1
)

echo MSI file created successfully: CivilX-Revit2021-v1.5.msi
echo.
echo Installation paths:
echo - DLL files: %%ProgramFiles%%\CivilX\CivilX_export\
echo - AddIn file: %%CommonAppData%%\Autodesk\Revit\Addins\2021\
echo.
pause
```

## 📂 Пути установки

### Для Revit 2021:
- **DLL файлы:** `C:\Program Files\CivilX\CivilX_export\`
- **AddIn файл:** `C:\ProgramData\Autodesk\Revit\Addins\2021\`

### Файлы для включения в MSI:
1. `RevitExporterAddin.dll` (из dataviewer2021)
2. `CivilX.Shared.dll`
3. `CivilXAuthPlugin.dll`
4. `RevitExporterAddin.addin` (конфигурация)
5. Иконки и ресурсы

## 🚀 Автоматизация

### Полный процесс сборки:
```batch
# 1. Компиляция проектов
build_projects.bat

# 2. Обновление файлов
update_files.bat

# 3. Создание MSI
build_msi_2021.bat
```

### Создайте универсальный скрипт `build_all_msi.bat`:
```batch
@echo off
echo Building MSI files for all Revit versions...

echo Building Revit 2021 MSI...
call build_msi_2021.bat

echo Building Revit 2022 MSI...
call build_msi_2022.bat

echo Building Revit 2023 MSI...
call build_msi.bat

echo All MSI files created successfully!
pause
```

## 🔍 Проверка MSI файла

### После создания MSI:
1. **Проверьте размер файла** (должен быть > 1MB)
2. **Протестируйте установку** на чистой системе
3. **Проверьте пути установки**
4. **Убедитесь в корректности .addin файла**

### Команды для проверки:
```batch
# Проверка размера
dir *.msi

# Тестовая установка (с параметром /quiet)
msiexec /i "CivilX-Revit2021-v1.5.msi" /quiet

# Удаление для тестирования
msiexec /x "CivilX-Revit2021-v1.5.msi" /quiet
```

## ⚠️ Важные моменты

### GUID и версии:
- **UpgradeCode** должен быть уникальным для каждой версии Revit
- **Product Id** используйте "*" для автогенерации
- **Version** увеличивайте при обновлениях

### Пути к файлам:
- Используйте абсолютные пути в Source атрибутах
- Проверьте существование всех файлов перед сборкой
- Убедитесь в корректности путей в .addin файле

### Права доступа:
- MSI требует права администратора для установки
- Файлы устанавливаются в Program Files и ProgramData
- Revit должен быть закрыт во время установки

## 🎉 Результат

После успешной сборки вы получите:
- `CivilX-Revit2021-v1.5.msi` - установочный файл
- `CivilX-Revit2021-v1.5.wixobj` - промежуточный файл
- `CivilX-Revit2021-v1.5.wixpdb` - файл отладки

Установщик автоматически:
- Скопирует все необходимые файлы
- Установит .addin файл в правильную папку
- Создаст записи в реестре для удаления
- Предоставит интерфейс для деинсталляции

