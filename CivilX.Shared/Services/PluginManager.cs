using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CivilX.Shared.Auth;
using CivilX.Shared.Models;

namespace CivilX.Shared.Services
{
    /// <summary>
    /// Централизованный менеджер плагинов
    /// Предоставляет простой API для проверки доступности плагинов
    /// </summary>
    public static class PluginManager
    {
        private static readonly IAuthService _authService = new AuthService();

        /// <summary>
        /// Проверяет доступность конкретного плагина для текущей версии Revit
        /// </summary>
        /// <param name="pluginName">Название плагина (например, "DataViewer", "AuthPlugin")</param>
        /// <param name="revitVersion">Версия Revit (по умолчанию "2023")</param>
        /// <returns>True если плагин доступен, False если нет</returns>
        public static bool IsPluginAvailable(string pluginName, string revitVersion = "2023")
        {
            try
            {
                return _authService.IsPluginAvailable(pluginName, revitVersion);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получает список всех доступных плагинов для указанной версии Revit
        /// </summary>
        /// <param name="revitVersion">Версия Revit (по умолчанию "2023")</param>
        /// <returns>Список названий доступных плагинов</returns>
        public static List<string> GetAvailablePlugins(string revitVersion = "2023")
        {
            try
            {
                return _authService.GetAvailablePlugins(revitVersion);
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Получает информацию о всех продуктах пользователя
        /// </summary>
        /// <returns>Список информации о продуктах</returns>
        public static List<ProductInfo> GetUserProducts()
        {
            try
            {
                WriteToLogFile($"[PluginManager] GetUserProducts: Starting...");
                
                var token = _authService.GetStoredToken();
                WriteToLogFile($"[PluginManager] GetUserProducts: Token = {(string.IsNullOrEmpty(token) ? "EMPTY" : "FOUND")}");
                
                if (string.IsNullOrEmpty(token))
                {
                    WriteToLogFile($"[PluginManager] GetUserProducts: No token, returning empty list");
                    return new List<ProductInfo>();
                }

                WriteToLogFile($"[PluginManager] GetUserProducts: Using Task.Run to avoid deadlock...");
                
                // Используем Task.Run чтобы избежать deadlock
                var task = Task.Run(async () => await _authService.GetUserProductsAsync(token));
                var result = task.GetAwaiter().GetResult();
                
                WriteToLogFile($"[PluginManager] GetUserProducts: Got {result?.Count ?? 0} products");
                
                return result;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[PluginManager] GetUserProducts: Exception: {ex.Message}");
                WriteToLogFile($"[PluginManager] GetUserProducts: Stack trace: {ex.StackTrace}");
                return new List<ProductInfo>();
            }
        }

        private static void WriteToLogFile(string message)
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var civilXPath = Path.Combine(appDataPath, "CivilX", "AuthPlugin");
                Directory.CreateDirectory(civilXPath);
                
                var logFilePath = Path.Combine(civilXPath, "auth_plugin_log.txt");
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки записи в лог
            }
        }

        /// <summary>
        /// Получает информацию о конкретном продукте
        /// </summary>
        /// <param name="productName">Название продукта</param>
        /// <param name="revitVersion">Версия Revit</param>
        /// <returns>Информация о продукте или null если не найден</returns>
        public static ProductInfo GetProductInfo(string productName, string revitVersion = "2023")
        {
            try
            {
                var products = GetUserProducts();
                return products.FirstOrDefault(p => 
                    p.ProductName == productName && 
                    p.RevitVersion == revitVersion);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Проверяет, активирован ли конкретный продукт
        /// </summary>
        /// <param name="productName">Название продукта</param>
        /// <param name="revitVersion">Версия Revit</param>
        /// <returns>True если продукт активирован, False если нет</returns>
        public static bool IsProductActivated(string productName, string revitVersion = "2023")
        {
            try
            {
                var product = GetProductInfo(productName, revitVersion);
                return product?.IsActive == true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получает статус активации продукта в виде строки
        /// </summary>
        /// <param name="productName">Название продукта</param>
        /// <param name="revitVersion">Версия Revit</param>
        /// <returns>Статус активации ("Активирован", "Готов", "Просрочен")</returns>
        public static string GetProductStatus(string productName, string revitVersion = "2023")
        {
            try
            {
                var product = GetProductInfo(productName, revitVersion);
                if (product == null)
                {
                    return "Не найден";
                }

                if (product.IsExpired)
                {
                    return "Просрочен";
                }

                if (product.IsActive)
                {
                    return "Активирован";
                }

                return "Готов";
            }
            catch
            {
                return "Неизвестно";
            }
        }

        /// <summary>
        /// Получает версию Revit из строки версии Revit API
        /// </summary>
        /// <param name="revitApiVersion">Версия Revit API (например, "2021.12")</param>
        /// <returns>Основная версия Revit (например, "2021")</returns>
        public static string GetRevitVersionFromApi(string revitApiVersion)
        {
            try
            {
                if (string.IsNullOrEmpty(revitApiVersion))
                {
                    return "2023"; // По умолчанию
                }

                // Извлекаем год из версии (например, "2021.12" -> "2021")
                var parts = revitApiVersion.Split('.');
                if (parts.Length > 0 && int.TryParse(parts[0], out int year))
                {
                    return year.ToString();
                }

                return "2023"; // По умолчанию
            }
            catch
            {
                return "2023"; // По умолчанию
            }
        }

        /// <summary>
        /// Проверяет доступность плагина для текущей версии Revit (автоопределение)
        /// </summary>
        /// <param name="pluginName">Название плагина</param>
        /// <param name="revitApiVersion">Версия Revit API</param>
        /// <returns>True если плагин доступен</returns>
        public static bool IsPluginAvailableForCurrentRevit(string pluginName, string revitApiVersion)
        {
            try
            {
                var revitVersion = GetRevitVersionFromApi(revitApiVersion);
                return IsPluginAvailable(pluginName, revitVersion);
            }
            catch
            {
                return false;
            }
        }
    }
}
