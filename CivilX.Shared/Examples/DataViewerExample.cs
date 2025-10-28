using System;
using CivilX.Shared.Services;
using CivilX.Shared.Auth;
using CivilX.Shared.Models;

namespace CivilX.Shared.Examples
{
    /// <summary>
    /// Пример использования Shared Library в DataViewer плагине
    /// </summary>
    public static class DataViewerExample
    {
        /// <summary>
        /// Проверяет доступность DataViewer для текущей версии Revit
        /// </summary>
        /// <param name="revitApiVersion">Версия Revit API (например, "2023.12")</param>
        /// <returns>True если DataViewer доступен</returns>
        public static bool IsDataViewerAvailable(string revitApiVersion = "2023.12")
        {
            // Простая проверка через PluginManager
            return PluginManager.IsPluginAvailableForCurrentRevit("DataViewer", revitApiVersion);
        }

        /// <summary>
        /// Получает информацию о статусе DataViewer
        /// </summary>
        /// <param name="revitVersion">Версия Revit (например, "2023")</param>
        /// <returns>Статус продукта</returns>
        public static string GetDataViewerStatus(string revitVersion = "2023")
        {
            return PluginManager.GetProductStatus("DataHub", revitVersion);
        }

        /// <summary>
        /// Получает детальную информацию о продукте DataViewer
        /// </summary>
        /// <param name="revitVersion">Версия Revit</param>
        /// <returns>Информация о продукте</returns>
        public static ProductInfo GetDataViewerInfo(string revitVersion = "2023")
        {
            return PluginManager.GetProductInfo("DataHub", revitVersion);
        }

        /// <summary>
        /// Проверяет авторизацию пользователя
        /// </summary>
        /// <returns>True если пользователь авторизован</returns>
        public static bool IsUserAuthorized()
        {
            var authService = new AuthService();
            var token = authService.GetStoredToken();
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Получает информацию о пользователе
        /// </summary>
        /// <returns>Информация о пользователе или null</returns>
        public static UserInfo GetUserInfo()
        {
            try
            {
                var authService = new AuthService();
                var token = authService.GetStoredToken();
                
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                return authService.GetUserInfoAsync(token).Result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Получает список всех доступных плагинов
        /// </summary>
        /// <param name="revitVersion">Версия Revit</param>
        /// <returns>Список доступных плагинов</returns>
        public static string[] GetAvailablePlugins(string revitVersion = "2023")
        {
            var plugins = PluginManager.GetAvailablePlugins(revitVersion);
            return plugins.ToArray();
        }

        /// <summary>
        /// Пример полной проверки доступности DataViewer
        /// </summary>
        /// <param name="revitApiVersion">Версия Revit API</param>
        /// <returns>Результат проверки с деталями</returns>
        public static DataViewerCheckResult CheckDataViewerAvailability(string revitApiVersion = "2023.12")
        {
            var result = new DataViewerCheckResult();

            try
            {
                // Проверяем авторизацию
                result.IsAuthorized = IsUserAuthorized();
                if (!result.IsAuthorized)
                {
                    result.ErrorMessage = "Пользователь не авторизован";
                    return result;
                }

                // Получаем версию Revit
                result.RevitVersion = PluginManager.GetRevitVersionFromApi(revitApiVersion);

                // Проверяем доступность DataViewer
                result.IsAvailable = IsDataViewerAvailable(revitApiVersion);

                // Получаем статус
                result.Status = GetDataViewerStatus(result.RevitVersion);

                // Получаем детальную информацию
                result.ProductInfo = GetDataViewerInfo(result.RevitVersion);

                // Получаем информацию о пользователе
                result.UserInfo = GetUserInfo();

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }

    /// <summary>
    /// Результат проверки доступности DataViewer
    /// </summary>
    public class DataViewerCheckResult
    {
        public bool Success { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsAvailable { get; set; }
        public string RevitVersion { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public ProductInfo ProductInfo { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
