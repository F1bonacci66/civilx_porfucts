using System;
using System.IO;

namespace RevitExporter.Services
{
    public static class RevitDataReaderFactory
    {
        public static IRevitDataReader CreateRevitDataReader()
        {
            // Проверяем, что Revit API доступен
            if (IsRevitApiAvailable())
            {
                try
                {
                    // Пытаемся создать реальный RevitDataReader
                    var reader = new RevitDataReader();
                    Console.WriteLine("✅ RevitDataReader создан успешно");
                    Console.WriteLine("🚀 Будет выполнена попытка чтения реальных данных из Revit файла");
                    return reader;
                }
                catch (Exception ex)
                {
                    // Revit API недоступен
                    Console.WriteLine(string.Format("❌ Ошибка создания RevitDataReader: {0}", ex.Message));
                }
            }
            else
            {
                Console.WriteLine("⚠️ Revit API файлы не найдены");
            }

            // Возвращаем fallback версию
            Console.WriteLine("🔄 Используется FallbackRevitDataReader");
            return new FallbackRevitDataReader();
        }

        private static bool IsRevitApiAvailable()
        {
            try
            {
                // Проверяем наличие файлов Revit API
                var revitApiPath = @"C:\Program Files\Autodesk\Revit 2023\RevitAPI.dll";
                var revitApiUiPath = @"C:\Program Files\Autodesk\Revit 2023\RevitAPIUI.dll";
                
                return File.Exists(revitApiPath) && File.Exists(revitApiUiPath);
            }
            catch
            {
                return false;
            }
        }

        private static string GetRevitVersion()
        {
            try
            {
                // Пытаемся получить версию Revit через реестр или другие способы
                // Это простая проверка доступности Revit API
                return "2023"; // Заглушка
            }
            catch
            {
                return null;
            }
        }
    }
}
