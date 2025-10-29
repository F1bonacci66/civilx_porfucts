using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    public class RevitExporter
    {
        public string ExportToCsv(List<RevitElementData> data, string modelName)
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("ModelName,ElementId,Category,Parameter,Value");

                foreach (var item in data)
                {
                    // Экранируем запятые и кавычки в значениях
                    var escapedValue = EscapeCsvValue(item.ParameterValue);
                    csv.AppendLine(string.Format("{0},{1},{2},{3},{4}", 
                        item.ModelName, item.ElementId, item.Category, item.ParameterName, escapedValue));
                }

                return csv.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Ошибка создания CSV: {0}", ex.Message), ex);
            }
        }

        public void SaveToFile(string csvContent, string modelName, string outputPath = null)
        {
            try
            {
                // Если путь не указан, используем папку документов
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                // Создаем имя файла с временной меткой
                var fileName = string.Format("{0}_export_{1:yyyy-MM-dd_HH-mm-ss}.csv", 
                    modelName, DateTime.Now);
                
                var fullPath = Path.Combine(outputPath, fileName);
                
                // Сохраняем файл
                File.WriteAllText(fullPath, csvContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Ошибка сохранения файла: {0}", ex.Message), ex);
            }
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
    }
}
