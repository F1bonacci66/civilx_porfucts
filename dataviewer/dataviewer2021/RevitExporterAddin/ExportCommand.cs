using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitExporterAddin.Services;
using RevitExporterAddin.Views;
using System.Windows;
using CivilX.Shared.Auth;
using CivilX.Shared.Models;
using CivilX.Shared.Services;

namespace RevitExporterAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportCommand : IExternalCommand
    {
        private static Views.MainWindow _currentWindow = null;

        static ExportCommand()
        {
            WriteToLogFileStatic("=== ExportCommand DLL Loaded - v1.2 with Category Filters ===");
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                WriteToLogFile("=== DataViewer Execute Started ===");
                
                // Получаем доступ к приложению Revit
                var app = commandData.Application.Application;
                WriteToLogFile("Revit application obtained");
                
                // Проверяем доступность плагина ДО создания окна
                WriteToLogFile("Checking plugin availability...");
                bool isAvailable = IsPluginAvailable();
                WriteToLogFile($"Plugin availability check result: {isAvailable}");
                
                if (!isAvailable)
                {
                    WriteToLogFile("Plugin is not available - showing dialog");
                    TaskDialog.Show("DataViewer - Недоступен", 
                        "Плагин DataHub недоступен. Необходима активная подписка на группу DataHub.");
                    return Result.Succeeded; // Возвращаем Success, чтобы не показывать ошибку Revit
                }
                
                WriteToLogFile("Plugin is available - checking for existing window");
                
                // Проверяем, есть ли уже открытое окно
                if (_currentWindow != null && _currentWindow.IsLoaded)
                {
                    WriteToLogFile("Existing window found - bringing to front");
                    _currentWindow.Activate();
                    _currentWindow.WindowState = System.Windows.WindowState.Normal;
                    _currentWindow.Topmost = true;
                    _currentWindow.Topmost = false;
                }
                else
                {
                    WriteToLogFile("No existing window - creating new window");
                    // Создаем новое окно только если нет открытого
                    _currentWindow = new Views.MainWindow(app);
                    _currentWindow.Closed += (s, e) => _currentWindow = null; // Очищаем ссылку при закрытии
                    _currentWindow.Show();
                    WriteToLogFile("New window shown successfully");
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Exception in Execute: {ex.Message}");
                WriteToLogFile($"Stack trace: {ex.StackTrace}");
                
                message = string.Format("Ошибка запуска приложения: {0}", ex.Message);
                
                // Показываем детальную ошибку пользователю
                TaskDialog.Show("Revit Exporter - Ошибка", 
                    string.Format("Произошла ошибка при запуске приложения:\n\n{0}\n\n{1}", 
                        ex.Message, ex.StackTrace));
                
                return Result.Failed;
            }
        }

        private bool IsPluginAvailable()
        {
            try
            {
                WriteToLogFile("IsPluginAvailable: Starting check");
                
                // Используем Shared Library для проверки доступности плагина
                bool result = PluginManager.IsPluginAvailable("DataViewer", "2023");
                WriteToLogFile($"IsPluginAvailable: Plugin availability check result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"IsPluginAvailable: Exception: {ex.Message}");
                return false;
            }
        }


        private void WriteToLogFile(string message)
        {
            try
            {
                string logPath = @"C:\Users\dimag\AppData\Roaming\CivilX\DataViewer\dataviewer_log.txt";
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                
                // Создаем директорию если не существует
                string logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        private static void WriteToLogFileStatic(string message)
        {
            try
            {
                string logPath = @"C:\Users\dimag\AppData\Roaming\CivilX\DataViewer\dataviewer_log.txt";
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                
                // Создаем директорию если не существует
                string logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }
    }
}
