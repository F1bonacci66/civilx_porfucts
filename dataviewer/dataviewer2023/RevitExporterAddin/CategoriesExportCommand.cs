using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace RevitExporterAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CategoriesExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                WriteToLogFile("=== Categories Export Started ===");
                
                // Получаем доступ к приложению Revit
                var app = commandData.Application.Application;
                var doc = commandData.Application.ActiveUIDocument?.Document;
                
                if (doc == null)
                {
                    TaskDialog.Show("Ошибка", "Нет открытого документа Revit");
                    return Result.Failed;
                }
                
                WriteToLogFile($"Processing document: {doc.Title}");
                
                // Собираем все категории
                var categories = CollectCategories(doc);
                
                // Экспортируем в CSV
                ExportCategoriesToCsv(categories, doc.Title);
                
                WriteToLogFile($"Categories export completed. Found {categories.Count} categories");
                
                TaskDialog.Show("Экспорт категорий", 
                    $"Экспорт завершен!\n\nНайдено категорий: {categories.Count}\n\nФайл сохранен в папке 'Документы'");
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Exception in CategoriesExportCommand: {ex.Message}");
                WriteToLogFile($"Stack trace: {ex.StackTrace}");
                
                message = $"Ошибка экспорта категорий: {ex.Message}";
                TaskDialog.Show("Ошибка", message);
                
                return Result.Failed;
            }
        }

        private List<CategoryInfo> CollectCategories(Document document)
        {
            var categories = new List<CategoryInfo>();
            
            try
            {
                // Получаем все категории из документа
                var categorySet = document.Settings.Categories;
                
                foreach (Category category in categorySet)
                {
                    if (category == null) continue;
                    
                    try
                    {
                        // Считаем количество элементов в категории
                        var collector = new FilteredElementCollector(document);
                        var elements = collector.OfCategoryId(category.Id).ToElements();
                        
                        var categoryInfo = new CategoryInfo
                        {
                            Name = category.Name,
                            Id = category.Id.IntegerValue,
                            ElementCount = elements.Count,
                            IsSubcategory = category.Parent != null,
                            ParentCategory = category.Parent?.Name ?? ""
                        };
                        
                        categories.Add(categoryInfo);
                    }
                    catch (Exception ex)
                    {
                        WriteToLogFile($"Error processing category {category.Name}: {ex.Message}");
                    }
                }
                
                // Сортируем по количеству элементов (по убыванию)
                categories = categories.OrderByDescending(c => c.ElementCount).ToList();
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Error collecting categories: {ex.Message}");
            }
            
            return categories;
        }

        private void ExportCategoriesToCsv(List<CategoryInfo> categories, string documentTitle)
        {
            try
            {
                // Создаем папку для экспорта
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var exportFolder = Path.Combine(documentsPath, "RevitCategoriesExport");
                Directory.CreateDirectory(exportFolder);
                
                // Создаем имя файла
                var fileName = $"Categories_{documentTitle}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
                var filePath = Path.Combine(exportFolder, fileName);
                
                // Создаем CSV содержимое
                var csvContent = new List<string>();
                csvContent.Add("CategoryName,CategoryId,ElementCount,IsSubcategory,ParentCategory");
                
                foreach (var category in categories)
                {
                    var line = $"\"{category.Name}\",{category.Id},{category.ElementCount},{category.IsSubcategory},\"{category.ParentCategory}\"";
                    csvContent.Add(line);
                }
                
                // Сохраняем файл
                File.WriteAllLines(filePath, csvContent);
                
                WriteToLogFile($"Categories exported to: {filePath}");
                
                // Открываем папку с результатами
                System.Diagnostics.Process.Start("explorer.exe", exportFolder);
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Error exporting categories to CSV: {ex.Message}");
                throw;
            }
        }

        private void WriteToLogFile(string message)
        {
            try
            {
                string logPath = @"C:\Users\dimag\AppData\Roaming\CivilX\DataViewer\categories_export_log.txt";
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                
                // Создаем директорию если не существует
                string logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }
    }

    public class CategoryInfo
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int ElementCount { get; set; }
        public bool IsSubcategory { get; set; }
        public string ParentCategory { get; set; }
    }
}


