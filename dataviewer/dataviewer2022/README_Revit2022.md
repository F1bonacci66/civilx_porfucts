# Revit Exporter Plugin для Revit 2022

Это версия плагина Revit Exporter, адаптированная для работы с Revit 2022 API.

## Особенности

- Совместимость с Revit 2022
- Использует API Revit 2022 из папки `C:\Projects\1.RevitFiles\2022 API`
- Полный функционал экспорта данных из Revit в CSV
- Управление проектами и экспортом данных

## Установка

1. Скомпилируйте проект `RevitExporterAddin2022`
2. Скопируйте файл `RevitExporterAddin.addin` в папку AddIns Revit 2022
3. Скопируйте скомпилированную DLL в соответствующую папку

## Структура проекта

- `RevitExporterAddin2022.dll` - основная сборка плагина
- `RevitExporterAddin.addin` - файл конфигурации для Revit
- Все исходные файлы проекта скопированы из основной версии

## API

Проект использует следующие API:
- RevitAPI.dll (Revit 2022)
- RevitAPIUI.dll (Revit 2022)
- CivilX.Shared.dll (общая библиотека)

## Компиляция

Проект настроен для компиляции в режиме x64 и использует .NET Framework 4.8.

