using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitExporterAddin
{
    [Transaction(TransactionMode.Manual)]
    public class RevitApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Сначала загружаем и инициализируем другие Application плагины динамически
                // Это гарантирует, что плагины будут инициализированы в правильном порядке
                // AuthApplication создаст панель "Настройки" первой (слева)
                PluginLoader.LoadAndInitializePlugins(application);
                
                // Затем создаем остальные компоненты (панель DataHub будет справа)
                RibbonManager.CreateRibbonTab(application);
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка запуска", $"Ошибка при создании вкладки CivilX: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                // Выключаем все загруженные плагины
                PluginLoader.ShutdownPlugins(application);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при выключении плагинов: {ex.Message}");
            }
            
            return Result.Succeeded;
        }

    }
}