using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RevitExporterAddin
{
    /// <summary>
    /// Класс для динамической загрузки и инициализации других Application плагинов
    /// </summary>
    public static class PluginLoader
    {
        private static List<IExternalApplication> _loadedPlugins = new List<IExternalApplication>();

        /// <summary>
        /// Загружает и инициализирует Application плагины из указанных DLL файлов
        /// </summary>
        public static void LoadAndInitializePlugins(UIControlledApplication application)
        {
            try
            {
                // Получаем путь к папке с DLL файлами
                string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Список плагинов для загрузки: путь к DLL и имя класса
                var pluginsToLoad = new List<(string dllPath, string className)>
                {
                    // Авторизация плагин
                    (Path.Combine(assemblyDir, "CivilXAuthPlugin.dll"), "CivilXAuthPlugin.AuthApplication")
                };

                // Загружаем каждый плагин
                foreach (var (dllPath, className) in pluginsToLoad)
                {
                    if (File.Exists(dllPath))
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"PluginLoader: Загружаем плагин из {dllPath}, класс {className}");
                            
                            // Загружаем сборку
                            Assembly pluginAssembly = Assembly.LoadFrom(dllPath);
                            
                            // Получаем тип класса
                            Type pluginType = pluginAssembly.GetType(className);
                            
                            if (pluginType != null && typeof(IExternalApplication).IsAssignableFrom(pluginType))
                            {
                                // Создаем экземпляр плагина
                                IExternalApplication pluginInstance = Activator.CreateInstance(pluginType) as IExternalApplication;
                                
                                if (pluginInstance != null)
                                {
                                    // Инициализируем плагин
                                    Result result = pluginInstance.OnStartup(application);
                                    
                                    if (result == Result.Succeeded)
                                    {
                                        _loadedPlugins.Add(pluginInstance);
                                        System.Diagnostics.Debug.WriteLine($"PluginLoader: Плагин {className} успешно загружен и инициализирован");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"PluginLoader: Ошибка инициализации плагина {className}: {result}");
                                    }
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"PluginLoader: Класс {className} не найден или не реализует IExternalApplication");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"PluginLoader: Ошибка загрузки плагина {dllPath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"PluginLoader: DLL файл не найден: {dllPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PluginLoader: Критическая ошибка: {ex.Message}");
                TaskDialog.Show("Ошибка загрузки плагинов", $"Ошибка при загрузке дополнительных плагинов: {ex.Message}");
            }
        }

        /// <summary>
        /// Вызывает OnShutdown для всех загруженных плагинов
        /// </summary>
        public static void ShutdownPlugins(UIControlledApplication application)
        {
            foreach (var plugin in _loadedPlugins)
            {
                try
                {
                    plugin.OnShutdown(application);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PluginLoader: Ошибка при выключении плагина: {ex.Message}");
                }
            }
        }
    }
}

