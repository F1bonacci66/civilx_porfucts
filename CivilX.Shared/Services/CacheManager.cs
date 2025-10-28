using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace CivilX.Shared.Services
{
    /// <summary>
    /// Менеджер кэширования для оптимизации производительности
    /// </summary>
    public static class CacheManager
    {
        private static readonly string _cacheDirectory;
        private static readonly string _cacheFilePath;
        private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        static CacheManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _cacheDirectory = Path.Combine(appDataPath, "CivilX", "Cache");
            _cacheFilePath = Path.Combine(_cacheDirectory, "cache.json");
            
            // Создаем директорию кэша если не существует
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        /// <summary>
        /// Получает данные из кэша
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <returns>Данные из кэша или null если не найдены или устарели</returns>
        public static T Get<T>(string key)
        {
            try
            {
                if (!File.Exists(_cacheFilePath))
                {
                    return default(T);
                }

                var cacheData = File.ReadAllText(_cacheFilePath);
                var cache = new JavaScriptSerializer().Deserialize<Dictionary<string, CacheItem>>(cacheData);

                if (cache.ContainsKey(key))
                {
                    var item = cache[key];
                    
                    // Проверяем не устарел ли кэш
                    if (DateTime.Now - item.CachedAt < _cacheExpiration)
                    {
                        return new JavaScriptSerializer().Deserialize<T>(item.Data);
                    }
                    else
                    {
                        // Удаляем устаревший элемент
                        cache.Remove(key);
                        SaveCache(cache);
                    }
                }

                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Сохраняет данные в кэш
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="data">Данные для кэширования</param>
        public static void Set<T>(string key, T data)
        {
            try
            {
                var cache = LoadCache();
                var item = new CacheItem
                {
                    Data = new JavaScriptSerializer().Serialize(data),
                    CachedAt = DateTime.Now
                };

                cache[key] = item;
                SaveCache(cache);
            }
            catch
            {
                // Игнорируем ошибки кэширования
            }
        }

        /// <summary>
        /// Удаляет данные из кэша
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        public static void Remove(string key)
        {
            try
            {
                var cache = LoadCache();
                if (cache.ContainsKey(key))
                {
                    cache.Remove(key);
                    SaveCache(cache);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        /// <summary>
        /// Очищает весь кэш
        /// </summary>
        public static void Clear()
        {
            try
            {
                if (File.Exists(_cacheFilePath))
                {
                    File.Delete(_cacheFilePath);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        /// <summary>
        /// Проверяет существует ли ключ в кэше и не устарел ли он
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        /// <returns>True если ключ существует и не устарел</returns>
        public static bool Exists(string key)
        {
            try
            {
                if (!File.Exists(_cacheFilePath))
                {
                    return false;
                }

                var cache = LoadCache();
                if (cache.ContainsKey(key))
                {
                    var item = cache[key];
                    return DateTime.Now - item.CachedAt < _cacheExpiration;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<string, CacheItem> LoadCache()
        {
            try
            {
                if (File.Exists(_cacheFilePath))
                {
                    var cacheData = File.ReadAllText(_cacheFilePath);
                    return new JavaScriptSerializer().Deserialize<Dictionary<string, CacheItem>>(cacheData);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            return new Dictionary<string, CacheItem>();
        }

        private static void SaveCache(Dictionary<string, CacheItem> cache)
        {
            try
            {
                // Атомарное сохранение через временный файл
                var tempFilePath = _cacheFilePath + ".tmp";
                var json = new JavaScriptSerializer().Serialize(cache);
                File.WriteAllText(tempFilePath, json);
                File.Replace(tempFilePath, _cacheFilePath, null);
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        private class CacheItem
        {
            public string Data { get; set; }
            public DateTime CachedAt { get; set; }
        }
    }
}
