using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    /// <summary>
    /// Оптимизированный экспортер данных Revit с приоритетом на качество и производительность
    /// </summary>
    public class OptimizedRevitExporter
    {
        private readonly OptimizedRevitDataReader _dataReader;
        private readonly StringBuilder _csvBuffer;
        private readonly Dictionary<string, string> _escapedValueCache;

        public OptimizedRevitExporter()
        {
            _dataReader = new OptimizedRevitDataReader();
            _csvBuffer = new StringBuilder(1024 * 1024); // Предварительное выделение 1MB
            _escapedValueCache = new Dictionary<string, string>();
        }

        /// <summary>
        /// Экспорт данных в CSV с оптимизацией производительности
        /// </summary>
        public async Task<string> ExportToCsvAsync(List<RevitElementData> data, string modelName, IProgress<string> progress = null)
        {
            if (data == null || !data.Any())
            {
                throw new ArgumentException("Данные для экспорта не могут быть пустыми", nameof(data));
            }

            if (string.IsNullOrEmpty(modelName))
            {
                throw new ArgumentException("Имя модели не может быть пустым", nameof(modelName));
            }

            try
            {
                if (progress != null)
                    progress.Report($"Начинаем экспорт {data.Count} записей для модели {modelName}");

                // Очищаем буферы
                _csvBuffer.Clear();
                _escapedValueCache.Clear();

                // Добавляем заголовок CSV
                _csvBuffer.AppendLine("ModelName,ElementId,Category,ParameterName,ParameterValue");

                // Группируем данные по элементам для оптимизации
                var groupedData = GroupDataByElement(data);
                
                if (progress != null)
                    progress.Report($"Сгруппировано {groupedData.Count} элементов");

                // Обрабатываем данные батчами
                await ProcessDataInBatches(groupedData, progress);

                if (progress != null)
                    progress.Report($"Экспорт завершен. Создано {data.Count} записей");

                return _csvBuffer.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка создания CSV: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Группировка данных по элементам для оптимизации обработки
        /// </summary>
        private Dictionary<int, List<RevitElementData>> GroupDataByElement(List<RevitElementData> data)
        {
            var grouped = new Dictionary<int, List<RevitElementData>>();
            
            foreach (var item in data)
            {
                if (!grouped.ContainsKey(item.ElementId))
                {
                    grouped[item.ElementId] = new List<RevitElementData>();
                }
                grouped[item.ElementId].Add(item);
            }
            
            return grouped;
        }

        /// <summary>
        /// Обработка данных батчами для оптимизации памяти
        /// </summary>
        private async Task ProcessDataInBatches(Dictionary<int, List<RevitElementData>> groupedData, IProgress<string> progress)
        {
            const int batchSize = 100; // Обрабатываем по 100 элементов за раз
            var elementIds = groupedData.Keys.ToList();
            
            for (int i = 0; i < elementIds.Count; i += batchSize)
            {
                var batch = elementIds.Skip(i).Take(batchSize).ToList();
                await ProcessBatch(batch, groupedData, progress);
                
                if (progress != null && i % (batchSize * 10) == 0)
                {
                    progress.Report($"Обработано {Math.Min(i + batchSize, elementIds.Count)} из {elementIds.Count} элементов");
                }
            }
        }

        /// <summary>
        /// Обработка батча элементов
        /// </summary>
        private async Task ProcessBatch(List<int> elementIds, Dictionary<int, List<RevitElementData>> groupedData, IProgress<string> progress)
        {
            await Task.Run(() =>
            {
                foreach (var elementId in elementIds)
                {
                    if (groupedData.TryGetValue(elementId, out var elementData))
                    {
                        ProcessElementData(elementData);
                    }
                }
            });
        }

        /// <summary>
        /// Обработка данных одного элемента
        /// </summary>
        private void ProcessElementData(List<RevitElementData> elementData)
        {
            if (!elementData.Any()) return;

            var firstItem = elementData.First();
            var modelName = firstItem.ModelName;
            var elementId = firstItem.ElementId;
            var category = firstItem.Category;

            // Сортируем параметры для консистентности
            var sortedData = elementData.OrderBy(x => x.ParameterName).ToList();

            foreach (var item in sortedData)
            {
                var escapedValue = GetCachedEscapedValue(item.ParameterValue);
                _csvBuffer.AppendLine($"{modelName},{elementId},{category},{item.ParameterName},{escapedValue}");
            }
        }

        /// <summary>
        /// Получение экранированного значения с кэшированием
        /// </summary>
        private string GetCachedEscapedValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (!_escapedValueCache.TryGetValue(value, out string escapedValue))
            {
                escapedValue = EscapeCsvValue(value);
                _escapedValueCache[value] = escapedValue;
            }

            return escapedValue;
        }

        /// <summary>
        /// Оптимизированное экранирование CSV значений
        /// </summary>
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Проверяем, нужно ли экранирование
            bool needsEscaping = value.Contains(",") || 
                                value.Contains("\"") || 
                                value.Contains("\n") || 
                                value.Contains("\r") ||
                                value.StartsWith(" ") ||
                                value.EndsWith(" ");

            if (!needsEscaping)
                return value;

            // Экранируем значение
            var escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        /// <summary>
        /// Сохранение в файл с оптимизацией
        /// </summary>
        public async Task SaveToFileAsync(string csvContent, string modelName, string outputPath = null, IProgress<string> progress = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                // Создаем имя файла с временной меткой
                var fileName = $"{modelName}_export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
                var fullPath = Path.Combine(outputPath, fileName);

                if (progress != null)
                    progress.Report($"Сохраняем файл: {fullPath}");

                // Асинхронное сохранение
                await File.WriteAllTextAsync(fullPath, csvContent, Encoding.UTF8);

                if (progress != null)
                    progress.Report($"Файл успешно сохранен: {fullPath}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка сохранения файла: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получение статистики экспорта
        /// </summary>
        public ExportStatistics GetExportStatistics(List<RevitElementData> data)
        {
            if (data == null || !data.Any())
                return new ExportStatistics();

            var stats = new ExportStatistics
            {
                TotalRecords = data.Count,
                UniqueElements = data.Select(x => x.ElementId).Distinct().Count(),
                UniqueCategories = data.Select(x => x.Category).Distinct().Count(),
                UniqueParameters = data.Select(x => x.ParameterName).Distinct().Count(),
                ModelName = data.FirstOrDefault()?.ModelName ?? "Unknown"
            };

            // Группируем по категориям
            stats.CategoryBreakdown = data
                .GroupBy(x => x.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            // Группируем по параметрам
            stats.ParameterBreakdown = data
                .GroupBy(x => x.ParameterName)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        public void Dispose()
        {
            _csvBuffer?.Clear();
            _escapedValueCache?.Clear();
        }
    }

    /// <summary>
    /// Статистика экспорта
    /// </summary>
    public class ExportStatistics
    {
        public int TotalRecords { get; set; }
        public int UniqueElements { get; set; }
        public int UniqueCategories { get; set; }
        public int UniqueParameters { get; set; }
        public string ModelName { get; set; }
        public Dictionary<string, int> CategoryBreakdown { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ParameterBreakdown { get; set; } = new Dictionary<string, int>();

        public override string ToString()
        {
            return $"Экспорт модели '{ModelName}': {TotalRecords} записей, {UniqueElements} элементов, {UniqueCategories} категорий, {UniqueParameters} параметров";
        }
    }
}






