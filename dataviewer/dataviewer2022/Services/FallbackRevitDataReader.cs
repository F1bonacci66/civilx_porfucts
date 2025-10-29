using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RevitExporter.Services
{
    public class FallbackRevitDataReader : IRevitDataReader
    {
        public bool CanReadRevitFile(string filePath)
        {
            return File.Exists(filePath) && Path.GetExtension(filePath).ToLower() == ".rvt";
        }

        public async Task<List<RevitElementData>> ReadRevitFileAsync(string filePath, IProgress<string> progress = null)
        {
            return await Task.Run(() => ReadRevitFileInternal(filePath, progress));
        }

        private List<RevitElementData> ReadRevitFileInternal(string filePath, IProgress<string> progress)
        {
            var modelName = Path.GetFileNameWithoutExtension(filePath);

            if (progress != null)
                progress.Report(string.Format("🔄 FallbackRevitDataReader: Revit API недоступен"));
            
            if (progress != null)
                progress.Report(string.Format("📁 Файл: {0}", modelName));
            
            if (progress != null)
                progress.Report("❌ Невозможно прочитать реальные данные из Revit файла");
            
            if (progress != null)
                progress.Report("💡 Для чтения реальных данных необходимо:");
            if (progress != null)
                progress.Report("   1. Установить Autodesk Revit 2023");
            if (progress != null)
                progress.Report("   2. Реализовать полную функциональность в RevitDataReader");
            if (progress != null)
                progress.Report("   3. Запустить приложение в среде с доступом к Revit API");

            // Возвращаем пустой список - реальные данные недоступны
            return new List<RevitElementData>();
        }


    }
}
