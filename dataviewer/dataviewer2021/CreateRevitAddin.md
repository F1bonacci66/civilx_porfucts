# 🔧 Создание Revit Add-in для чтения данных

## 🎯 Проблема
Revit API работает ТОЛЬКО внутри Revit как плагин. Наше приложение не может подключиться к Revit снаружи.

## ✅ Решение - Revit Add-in

### 1. Создать новый проект плагина
```
File → New → Project → Class Library (.NET Framework 4.8)
Name: RevitExporterAddin
```

### 2. Добавить ссылки на Revit API
```
References:
- RevitAPI.dll
- RevitAPIUI.dll
```

### 3. Создать файл манифеста (.addin)
```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Command">
    <Name>Revit Exporter</Name>
    <Assembly>RevitExporterAddin.dll</Assembly>
    <FullClassName>RevitExporterAddin.ExportCommand</FullClassName>
    <Text>Export Data</Text>
    <Description>Экспорт данных из Revit в CSV</Description>
    <VisibilityMode>AlwaysVisible</VisibilityMode>
  </AddIn>
</RevitAddIns>
```

### 4. Создать команду плагина
```csharp
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

[Transaction(TransactionMode.ReadOnly)]
public class ExportCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            // Получаем доступ к документу
            Document doc = commandData.Application.ActiveUIDocument.Document;
            
            // Используем наш уже готовый код!
            var reader = new RevitDataReader();
            var data = reader.ExtractElementsFromDocument(doc, doc.Title, null);
            
            // Сохраняем в CSV
            SaveToCsv(data, doc.Title);
            
            TaskDialog.Show("Успех", $"Экспортировано {data.Count} записей");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}
```

## 🚀 Преимущества этого подхода:

1. **Работает внутри Revit** - полный доступ к API
2. **Наш код готов** - можем переиспользовать RevitDataReader
3. **Простая установка** - копировать .dll и .addin в папку Revit
4. **Удобно для пользователя** - кнопка прямо в Revit

## 📂 Структура файлов:
```
RevitExporterAddin/
├── ExportCommand.cs           // Команда плагина
├── RevitDataReader.cs         // Наш готовый код
├── RevitElementData.cs        // Модель данных
├── RevitExporterAddin.addin   // Манифест
└── bin/Debug/
    └── RevitExporterAddin.dll // Скомпилированный плагин
```

## 📍 Установка:
1. Скопировать `.dll` и `.addin` в:
   `C:\ProgramData\Autodesk\Revit\Addins\2023\`
2. Перезапустить Revit
3. Найти кнопку "Export Data" в интерфейсе

## 💡 Хотите, чтобы я создал этот плагин?



