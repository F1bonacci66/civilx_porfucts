using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    public class RevitExporterService : IRevitExporter
    {
        public async Task ExportToCsvAsync(ExportTab exportTab, IProgress<string> progress = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (progress != null)
                        progress.Report("Начинаем экспорт...");

                    // Здесь должна быть логика экспорта из Revit
                    // Пока что создаем заглушку
                    var csvContent = GenerateCsvContent(exportTab);
                    
                    if (progress != null)
                        progress.Report("Создаем CSV файл...");

                    SaveToFile(csvContent, exportTab.Name);
                    
                    if (progress != null)
                        progress.Report("Экспорт завершен успешно!");
                }
                catch (Exception ex)
                {
                    if (progress != null)
                        progress.Report($"Ошибка экспорта: {ex.Message}");
                    throw;
                }
            });
        }

        private string GenerateCsvContent(ExportTab exportTab)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ModelName,ElementId,Category,ParameterName,ParameterValue");

            // Заглушка - в реальной реализации здесь будет экспорт из Revit
            csv.AppendLine($"{exportTab.Name},1,Wall,Length,5.0");
            csv.AppendLine($"{exportTab.Name},2,Wall,Height,3.0");
            csv.AppendLine($"{exportTab.Name},3,Door,Width,0.9");

            return csv.ToString();
        }

        private void SaveToFile(string csvContent, string modelName)
        {
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileName = string.Format("{0}_export_{1:yyyy-MM-dd_HH-mm-ss}.csv", 
                modelName, DateTime.Now);
            
            var fullPath = Path.Combine(outputPath, fileName);
            File.WriteAllText(fullPath, csvContent, Encoding.UTF8);
        }
    }
}


