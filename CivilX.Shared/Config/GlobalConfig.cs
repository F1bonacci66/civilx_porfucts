using System;

namespace CivilX.Shared.Config
{
    /// <summary>
    /// Глобальная конфигурация для всех продуктов CivilX
    /// </summary>
    public static class GlobalConfig
    {
        #region API Configuration

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

        #endregion

        #region Website URLs

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

        #endregion

        #region Database Configuration

        /// <summary>
        /// Хост базы данных
        /// </summary>
        public static string DatabaseHost => "localhost";

        /// <summary>
        /// Имя базы данных
        /// </summary>
        public static string DatabaseName => "u3279080_CivilX_users";

        /// <summary>
        /// Имя пользователя БД
        /// </summary>
        public static string DatabaseUsername => "u3279080_civilx_user";

        /// <summary>
        /// Пароль БД
        /// </summary>
        public static string DatabasePassword => "!Grosheva78";

        /// <summary>
        /// Строка подключения к MySQL
        /// </summary>
        public static string DatabaseConnectionString => 
            $"Server={DatabaseHost};Database={DatabaseName};Uid={DatabaseUsername};Pwd={DatabasePassword};Charset=utf8mb4;";

        #endregion

        #region Application Settings

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

        /// <summary>
        /// Время жизни кэша (в минутах)
        /// </summary>
        public static int CacheLifetimeMinutes => 5;

        /// <summary>
        /// Название компании
        /// </summary>
        public static string CompanyName => "CivilX";

        /// <summary>
        /// Email поддержки
        /// </summary>
        public static string SupportEmail => "info@civilx.ru";

        /// <summary>
        /// Версия API
        /// </summary>
        public static string ApiVersion => "1.0";

        #endregion

        #region Registry and File Paths

        /// <summary>
        /// Ключ реестра для хранения токенов
        /// </summary>
        public static string TokenRegistryKey => @"SOFTWARE\CivilX\AuthPlugin";

        /// <summary>
        /// Имя значения в реестре для токена
        /// </summary>
        public static string TokenValueName => "AuthToken";

        /// <summary>
        /// Папка AppData для CivilX
        /// </summary>
        public static string AppDataFolder => "CivilX";

        /// <summary>
        /// Папка плагина авторизации
        /// </summary>
        public static string PluginFolder => "AuthPlugin";

        #endregion

        #region Validation

        /// <summary>
        /// Проверяет корректность конфигурации
        /// </summary>
        public static void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(BaseUrl))
                throw new InvalidOperationException("BaseUrl не настроен");

            if (string.IsNullOrEmpty(AuthApiUrl))
                throw new InvalidOperationException("AuthApiUrl не настроен");

            if (string.IsNullOrEmpty(DatabaseHost))
                throw new InvalidOperationException("DatabaseHost не настроен");

            if (string.IsNullOrEmpty(DatabaseName))
                throw new InvalidOperationException("DatabaseName не настроен");

            if (RequestTimeoutSeconds <= 0)
                throw new InvalidOperationException("RequestTimeoutSeconds должен быть больше 0");

            if (MaxRetryAttempts < 0)
                throw new InvalidOperationException("MaxRetryAttempts не может быть отрицательным");
        }

        /// <summary>
        /// Получает информацию о конфигурации для логирования
        /// </summary>
        public static string GetConfigurationInfo()
        {
            return $"CivilX Configuration:\n" +
                   $"- Base URL: {BaseUrl}\n" +
                   $"- Auth API: {AuthApiUrl}\n" +
                   $"- Database: {DatabaseName}@{DatabaseHost}\n" +
                   $"- Request Timeout: {RequestTimeoutSeconds}s\n" +
                   $"- Max Retries: {MaxRetryAttempts}\n" +
                   $"- Cache Lifetime: {CacheLifetimeMinutes}min\n" +
                   $"- Company: {CompanyName}\n" +
                   $"- Support: {SupportEmail}";
        }

        #endregion
    }
}






