using System;

namespace RevitExporterAddin.Config
{
    /// <summary>
    /// Конфигурация API CivilX
    /// </summary>
    public static class ApiConfig
    {
        /// <summary>
        /// Базовый URL сайта CivilX
        /// </summary>
        public static string BaseUrl => "http://civilx.ru";

        /// <summary>
        /// URL API для авторизации
        /// </summary>
        public static string AuthApiUrl => $"{BaseUrl}/auth-api.php";

        /// <summary>
        /// URL для входа в систему
        /// </summary>
        public static string LoginUrl => $"{AuthApiUrl}/api/login";

        /// <summary>
        /// URL для регистрации
        /// </summary>
        public static string RegisterUrl => $"{AuthApiUrl}/api/register";

        /// <summary>
        /// URL для получения информации о пользователе
        /// </summary>
        public static string MeUrl => $"{AuthApiUrl}/api/me";

        /// <summary>
        /// URL для получения продуктов пользователя
        /// </summary>
        public static string UserProductsUrl => $"{AuthApiUrl}/api/user-products";

        /// <summary>
        /// URL для активации продукта
        /// </summary>
        public static string ActivateProductUrl => $"{AuthApiUrl}/api/activate-product";

        /// <summary>
        /// URL для деактивации продукта
        /// </summary>
        public static string DeactivateProductUrl => $"{AuthApiUrl}/api/deactivate-product";

        /// <summary>
        /// URL для проверки группы плагинов
        /// </summary>
        public static string CheckPluginGroupUrl => $"{AuthApiUrl}/api/check-plugin-group";

        /// <summary>
        /// URL для получения доступных версий
        /// </summary>
        public static string AvailableVersionsUrl => $"{AuthApiUrl}/api/available-versions";

        /// <summary>
        /// URL для получения версий продуктов
        /// </summary>
        public static string ProductVersionsUrl => $"{AuthApiUrl}/api/product-versions";

        /// <summary>
        /// URL для получения продуктов подписки
        /// </summary>
        public static string SubscriptionProductsUrl => $"{AuthApiUrl}/api/subscription-products";

        /// <summary>
        /// URL главной страницы сайта
        /// </summary>
        public static string WebsiteUrl => BaseUrl;

        /// <summary>
        /// URL личного кабинета
        /// </summary>
        public static string DashboardUrl => $"{BaseUrl}/dashboard";

        /// <summary>
        /// URL страницы авторизации
        /// </summary>
        public static string AuthPageUrl => $"{BaseUrl}/auth";

        /// <summary>
        /// URL страницы контактов
        /// </summary>
        public static string ContactsUrl => $"{BaseUrl}/contacts";

        /// <summary>
        /// URL страницы тарифов
        /// </summary>
        public static string PricingUrl => $"{BaseUrl}/pricing";

        /// <summary>
        /// Таймаут для HTTP запросов (в секундах)
        /// </summary>
        public static int RequestTimeoutSeconds => 30;

        /// <summary>
        /// Максимальное количество попыток повторного запроса
        /// </summary>
        public static int MaxRetryAttempts => 3;

        /// <summary>
        /// Задержка между попытками (в миллисекундах)
        /// </summary>
        public static int RetryDelayMs => 1000;
    }
}






