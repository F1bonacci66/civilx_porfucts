using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitExporter.Models;
using System.Collections.Generic;

namespace RevitExporter.Services
{
    public class RevitExporter : IRevitExporter
    {
        private readonly IRevitDataReader _revitDataReader;

        public RevitExporter(IRevitDataReader revitDataReader = null)
        {
            _revitDataReader = revitDataReader ?? RevitDataReaderFactory.CreateRevitDataReader();
        }

        public async Task ExportToCsvAsync(ExportTab exportTab, IProgress<string> progress = null)
        {
            if (progress != null)
                progress.Report("Начинаем экспорт...");

            if (string.IsNullOrEmpty(exportTab.ModelsFolder) || !Directory.Exists(exportTab.ModelsFolder))
            {
                throw new DirectoryNotFoundException(string.Format("Папка с моделями не найдена: {0}", exportTab.ModelsFolder));
            }

            var revitFiles = Directory.GetFiles(exportTab.ModelsFolder, "*.rvt", SearchOption.TopDirectoryOnly);
            if (!revitFiles.Any())
            {
                throw new FileNotFoundException(string.Format("Файлы Revit не найдены в папке: {0}", exportTab.ModelsFolder));
            }

            var resultsFolder = Path.Combine(exportTab.ModelsFolder, "Результаты");
            if (!Directory.Exists(resultsFolder))
            {
                Directory.CreateDirectory(resultsFolder);
            }

            if (progress != null)
                progress.Report(string.Format("Найдено {0} файлов Revit", revitFiles.Length));

            foreach (var revitFile in revitFiles)
            {
                await ProcessRevitFileAsync(revitFile, resultsFolder, exportTab.RevitVersion, progress);
            }

            if (progress != null)
                progress.Report("Экспорт завершен");
        }

        private async Task ProcessRevitFileAsync(string revitFilePath, string resultsFolder, string revitVersion, IProgress<string> progress)
        {
            var fileName = Path.GetFileNameWithoutExtension(revitFilePath);
            if (progress != null)
                progress.Report(string.Format("Обрабатываем файл: {0}", fileName));

            try
            {
                // Проверяем, что файл существует
                if (!File.Exists(revitFilePath))
                {
                    throw new FileNotFoundException(string.Format("Файл не найден: {0}", revitFilePath));
                }

                // Получаем информацию о файле
                var fileInfo = new FileInfo(revitFilePath);
                if (progress != null)
                    progress.Report(string.Format("Размер файла: {0} MB", fileInfo.Length / 1024 / 1024));

                // Пытаемся прочитать реальные данные из Revit файла
                List<RevitElementData> revitData = null;
                string revitError = null;
                
                if (progress != null)
                    progress.Report("Проверяем возможность чтения Revit файла...");
                
                try
                {
                    if (_revitDataReader.CanReadRevitFile(revitFilePath))
                    {
                        if (progress != null)
                            progress.Report("Файл валиден. Читаем реальные данные из Revit файла...");
                        
                        revitData = await _revitDataReader.ReadRevitFileAsync(revitFilePath, progress);
                        
                        if (revitData == null)
                        {
                            revitError = "RevitDataReader вернул null вместо данных";
                        }
                        else if (revitData.Count == 0)
                        {
                            revitError = "RevitDataReader вернул пустой список данных";
                        }
                    }
                    else
                    {
                        revitError = "Файл не является валидным Revit файлом (.rvt)";
                        if (progress != null)
                            progress.Report(revitError);
                    }
                }
                catch (Exception revitEx)
                {
                    revitError = string.Format("Ошибка чтения Revit файла: {0}", revitEx.Message);
                    if (progress != null)
                        progress.Report(string.Format("❌ {0}", revitError));
                    
                    // Логируем детали ошибки
                    if (progress != null)
                        progress.Report(string.Format("Детали ошибки: {0}", revitEx.ToString()));
                }

                // Создаем CSV данные
                string csvData;
                if (revitData != null && revitData.Count > 0)
                {
                    csvData = GenerateCsvFromRealData(revitData);
                    if (progress != null)
                        progress.Report(string.Format("✅ Получено {0} записей из Revit файла", revitData.Count));
                }
                else
                {
                    // Определяем причину отсутствия данных
                    string finalError = revitError;
                    if (string.IsNullOrEmpty(finalError))
                    {
                        if (revitData == null)
                        {
                            finalError = "RevitDataReader вернул null";
                        }
                        else if (revitData.Count == 0)
                        {
                            finalError = "RevitDataReader вернул пустой список данных - Revit API недоступен или не реализован";
                        }
                    }
                    
                    // Создаем CSV с информацией об ошибке
                    csvData = GenerateErrorCsv(fileName, finalError);
                    if (progress != null)
                        progress.Report(string.Format("⚠️ Создан CSV файл с информацией об ошибке: {0}", finalError));
                    
                    if (progress != null)
                        progress.Report("💡 Рекомендация: Используйте FallbackRevitDataReader для демонстрации");
                }

                var csvFilePath = Path.Combine(resultsFolder, string.Format("{0}_export.csv", fileName));
                File.WriteAllText(csvFilePath, csvData, Encoding.UTF8);
                
                if (progress != null)
                    progress.Report(string.Format("Файл сохранен: {0}", csvFilePath));
            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report(string.Format("Ошибка обработки файла {0}: {1}", fileName, ex.Message));
                throw;
            }
        }

        private string GenerateCsvFromRealData(List<RevitElementData> revitData)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ModelName,ElementId,Category,ParameterName,ParameterValue");

            foreach (var data in revitData)
            {
                // Экранируем запятые и кавычки в значениях
                var escapedValue = EscapeCsvValue(data.ParameterValue);
                csv.AppendLine(string.Format("{0},{1},{2},{3},{4}", data.ModelName, data.ElementId, data.Category, data.ParameterName, escapedValue));
            }

            return csv.ToString();
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Если значение содержит запятую, кавычку или перенос строки, заключаем в кавычки
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                // Заменяем двойные кавычки на две двойные кавычки
                value = value.Replace("\"", "\"\"");
                return string.Format("\"{0}\"", value);
            }

            return value;
        }

        private string GenerateErrorCsv(string modelName, string errorMessage)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ModelName,ElementId,Category,ParameterName,ParameterValue");
            
            // Добавляем информацию о том, что реальные данные недоступны
            csv.AppendLine(string.Format("{0},N/A,N/A,Status,Реальные данные недоступны", modelName));
            csv.AppendLine(string.Format("{0},N/A,N/A,Reason,{1}", modelName, errorMessage ?? "Revit API недоступен"));
            csv.AppendLine(string.Format("{0},N/A,N/A,Required,Установить Autodesk Revit 2023 и реализовать чтение данных", modelName));
            csv.AppendLine(string.Format("{0},N/A,N/A,Timestamp,{1}", modelName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            
            return csv.ToString();
        }

        private string GenerateEnhancedTestData(string modelName, string revitFilePath)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ModelName,ElementId,Category,ParameterName,ParameterValue");

            // Имитируем реальные данные на основе имени файла
            var random = new Random(modelName.GetHashCode());
            var categories = new[] { "Walls", "Doors", "Windows", "Floors", "Roofs", "Columns", "Beams", "Furniture" };
            var parameterNames = new[] { "Name", "Type", "Level", "Material", "Height", "Width", "Length", "Area", "Volume", "Mark" };

            // Генерируем от 50 до 200 элементов
            var elementCount = random.Next(50, 200);
            
            for (int i = 1; i <= elementCount; i++)
            {
                var elementId = random.Next(1000, 9999);
                var category = categories[random.Next(categories.Length)];
                
                // Генерируем 3-5 параметров для каждого элемента
                var parameterCount = random.Next(3, 6);
                for (int j = 0; j < parameterCount; j++)
                {
                    var parameterName = parameterNames[random.Next(parameterNames.Length)];
                    string value;
                    
                    switch (parameterName)
                    {
                        case "Name":
                            value = string.Format("{0}_{1}", category, i);
                            break;
                        case "Type":
                            value = string.Format("Type_{0}", random.Next(1, 10));
                            break;
                        case "Level":
                            value = string.Format("Level_{0}", random.Next(1, 5));
                            break;
                        case "Material":
                            value = string.Format("Material_{0}", random.Next(1, 8));
                            break;
                        case "Height":
                        case "Width":
                        case "Length":
                            value = (random.NextDouble() * 10 + 1).ToString("F2");
                            break;
                        case "Area":
                            value = (random.NextDouble() * 100 + 10).ToString("F2");
                            break;
                        case "Volume":
                            value = (random.NextDouble() * 1000 + 50).ToString("F2");
                            break;
                        case "Mark":
                            value = string.Format("M-{0}", random.Next(100, 999));
                            break;
                        default:
                            value = string.Format("Value_{0}", random.Next(1, 100));
                            break;
                    }
                    
                    csv.AppendLine(string.Format("{0},{1},{2},{3},{4}", modelName, elementId, category, parameterName, value));
                }
            }

            return csv.ToString();
        }
    }
}
