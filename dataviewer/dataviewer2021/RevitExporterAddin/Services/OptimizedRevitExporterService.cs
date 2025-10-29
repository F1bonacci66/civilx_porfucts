using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    /// <summary>
    /// Оптимизированный сервис экспорта данных Revit с приоритетом на качество и производительность
    /// </summary>
    public class OptimizedRevitExporterService
    {
        private readonly OptimizedRevitDataReader _dataReader;
        private readonly OptimizedRevitExporter _exporter;
        private readonly Dictionary<string, Document> _documentCache;

        public OptimizedRevitExporterService()
        {
            _dataReader = new OptimizedRevitDataReader();
            _exporter = new OptimizedRevitExporter();
            _documentCache = new Dictionary<string, Document>();
        }

        /// <summary>
        /// Экспорт данных из Revit документа в CSV файл
        /// </summary>
        public async Task<ExportResult> ExportDocumentToCsvAsync(
            Document document, 
            string modelName, 
            string outputPath = null, 
            IProgress<string> progress = null)
        {
            try
            {
                if (document == null)
                    throw new ArgumentNullException(nameof(document), "Документ Revit не может быть null");

                if (string.IsNullOrEmpty(modelName))
                    throw new ArgumentException("Имя модели не может быть пустым", nameof(modelName));

                if (progress != null)
                    progress.Report($"Начинаем экспорт документа: {modelName}");

                // Извлекаем данные из документа
                var elementData = _dataReader.ExtractElementsFromDocument(document, modelName, progress);
                
                if (!elementData.Any())
                {
                    throw new InvalidOperationException("Не удалось извлечь данные из документа");
                }

                // Экспортируем в CSV
                var csvContent = await _exporter.ExportToCsvAsync(elementData, modelName, progress);

                // Сохраняем файл
                await _exporter.SaveToFileAsync(csvContent, modelName, outputPath, progress);

                // Получаем статистику
                var statistics = _exporter.GetExportStatistics(elementData);

                if (progress != null)
                    progress.Report($"Экспорт завершен успешно: {statistics}");

                return new ExportResult
                {
                    Success = true,
                    ModelName = modelName,
                    Statistics = statistics,
                    CsvContent = csvContent,
                    OutputPath = outputPath
                };
            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report($"❌ Ошибка экспорта: {ex.Message}");

                return new ExportResult
                {
                    Success = false,
                    ModelName = modelName,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// Экспорт данных из файла Revit в CSV
        /// </summary>
        public async Task<ExportResult> ExportFileToCsvAsync(
            string revitFilePath, 
            string outputPath = null, 
            IProgress<string> progress = null)
        {
            Document document = null;
            try
            {
                if (string.IsNullOrEmpty(revitFilePath))
                    throw new ArgumentException("Путь к файлу Revit не может быть пустым", nameof(revitFilePath));

                if (!File.Exists(revitFilePath))
                    throw new FileNotFoundException($"Файл не найден: {revitFilePath}");

                var modelName = Path.GetFileNameWithoutExtension(revitFilePath);

                if (progress != null)
                    progress.Report($"Открываем файл Revit: {revitFilePath}");

                // Открываем документ
                document = await OpenDocumentAsync(revitFilePath, progress);

                if (document == null)
                    throw new InvalidOperationException("Не удалось открыть документ Revit");

                // Экспортируем данные
                return await ExportDocumentToCsvAsync(document, modelName, outputPath, progress);
            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report($"❌ Ошибка экспорта файла: {ex.Message}");

                return new ExportResult
                {
                    Success = false,
                    ModelName = Path.GetFileNameWithoutExtension(revitFilePath),
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
            finally
            {
                // Закрываем документ если он был открыт
                if (document != null && !document.IsValidObject)
                {
                    try
                    {
                        document.Close();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Массовый экспорт файлов Revit
        /// </summary>
        public async Task<List<ExportResult>> ExportMultipleFilesAsync(
            List<string> revitFilePaths, 
            string outputPath = null, 
            IProgress<string> progress = null)
        {
            var results = new List<ExportResult>();

            if (revitFilePaths == null || !revitFilePaths.Any())
            {
                if (progress != null)
                    progress.Report("Список файлов для экспорта пуст");
                return results;
            }

            if (progress != null)
                progress.Report($"Начинаем массовый экспорт {revitFilePaths.Count} файлов");

            for (int i = 0; i < revitFilePaths.Count; i++)
            {
                var filePath = revitFilePaths[i];
                
                if (progress != null)
                    progress.Report($"Обрабатываем файл {i + 1} из {revitFilePaths.Count}: {Path.GetFileName(filePath)}");

                try
                {
                    var result = await ExportFileToCsvAsync(filePath, outputPath, progress);
                    results.Add(result);

                    if (result.Success)
                    {
                        if (progress != null)
                            progress.Report($"✅ Файл {i + 1} экспортирован успешно");
                    }
                    else
                    {
                        if (progress != null)
                            progress.Report($"❌ Ошибка экспорта файла {i + 1}: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    if (progress != null)
                        progress.Report($"❌ Критическая ошибка при обработке файла {i + 1}: {ex.Message}");

                    results.Add(new ExportResult
                    {
                        Success = false,
                        ModelName = Path.GetFileNameWithoutExtension(filePath),
                        ErrorMessage = ex.Message,
                        Exception = ex
                    });
                }
            }

            var successCount = results.Count(r => r.Success);
            if (progress != null)
                progress.Report($"Массовый экспорт завершен: {successCount} из {revitFilePaths.Count} файлов экспортированы успешно");

            return results;
        }

        /// <summary>
        /// Асинхронное открытие документа Revit
        /// </summary>
        private async Task<Document> OpenDocumentAsync(string filePath, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Проверяем кэш
                    if (_documentCache.TryGetValue(filePath, out Document cachedDocument))
                    {
                        if (cachedDocument.IsValidObject)
                        {
                            if (progress != null)
                                progress.Report("Используем кэшированный документ");
                            return cachedDocument;
                        }
                        else
                        {
                            _documentCache.Remove(filePath);
                        }
                    }

                    if (progress != null)
                        progress.Report("Открываем документ Revit...");

                    // Открываем документ
                    var document = Application.OpenDocumentFile(filePath);
                    
                    if (document != null)
                    {
                        _documentCache[filePath] = document;
                        if (progress != null)
                            progress.Report("Документ успешно открыт");
                    }

                    return document;
                }
                catch (Exception ex)
                {
                    if (progress != null)
                        progress.Report($"Ошибка открытия документа: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Получение информации о файле Revit
        /// </summary>
        public RevitFileInfo GetRevitFileInfo(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return new RevitFileInfo { IsValid = false, ErrorMessage = "Файл не найден" };

                var fileInfo = new FileInfo(filePath);
                
                return new RevitFileInfo
                {
                    IsValid = true,
                    FilePath = filePath,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    Extension = fileInfo.Extension.ToLower()
                };
            }
            catch (Exception ex)
            {
                return new RevitFileInfo 
                { 
                    IsValid = false, 
                    ErrorMessage = ex.Message 
                };
            }
        }

        /// <summary>
        /// Очистка кэша документов
        /// </summary>
        public void ClearDocumentCache()
        {
            foreach (var document in _documentCache.Values)
            {
                try
                {
                    if (document.IsValidObject)
                    {
                        document.Close();
                    }
                }
                catch { }
            }
            _documentCache.Clear();
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            ClearDocumentCache();
            _exporter?.Dispose();
        }
    }

    /// <summary>
    /// Результат экспорта
    /// </summary>
    public class ExportResult
    {
        public bool Success { get; set; }
        public string ModelName { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public ExportStatistics Statistics { get; set; }
        public string CsvContent { get; set; }
        public string OutputPath { get; set; }
        public DateTime ExportTime { get; set; } = DateTime.Now;

        public override string ToString()
        {
            if (Success)
            {
                return $"✅ Экспорт '{ModelName}' завершен успешно: {Statistics}";
            }
            else
            {
                return $"❌ Ошибка экспорта '{ModelName}': {ErrorMessage}";
            }
        }
    }

    /// <summary>
    /// Информация о файле Revit
    /// </summary>
    public class RevitFileInfo
    {
        public bool IsValid { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public string Extension { get; set; }
        public string ErrorMessage { get; set; }

        public string FileSizeFormatted => FormatFileSize(FileSize);

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public override string ToString()
        {
            if (IsValid)
            {
                return $"Файл: {FileName}, Размер: {FileSizeFormatted}, Изменен: {LastModified:yyyy-MM-dd HH:mm}";
            }
            else
            {
                return $"Неверный файл: {ErrorMessage}";
            }
        }
    }
}






