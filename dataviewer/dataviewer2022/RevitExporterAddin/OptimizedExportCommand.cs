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
    /// <summary>
    /// Оптимизированная команда экспорта с приоритетом на качество и производительность
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OptimizedExportCommand : IExternalCommand
    {
        private OptimizedRevitExporterService _exporterService;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                WriteToLogFile("=== Optimized DataViewer Execute Started ===");
                
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
                    return Result.Succeeded;
                }
                
                WriteToLogFile("Plugin is available - creating optimized exporter service");
                
                // Создаем оптимизированный сервис экспорта
                _exporterService = new OptimizedRevitExporterService();
                
                WriteToLogFile("Creating and showing optimized main window");
                
                // Открываем оптимизированный интерфейс управления проектами
                var mainWindow = new OptimizedMainWindow(app, _exporterService);
                mainWindow.Show();
                
                WriteToLogFile("Optimized window shown successfully");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Exception in OptimizedExecute: {ex.Message}");
                WriteToLogFile($"Stack trace: {ex.StackTrace}");
                
                message = string.Format("Ошибка запуска оптимизированного приложения: {0}", ex.Message);
                
                // Показываем детальную ошибку пользователю
                TaskDialog.Show("Revit Exporter - Ошибка", 
                    string.Format("Произошла ошибка при запуске оптимизированного приложения:\n\n{0}\n\n{1}", 
                        ex.Message, ex.StackTrace));
                
                return Result.Failed;
            }
        }

        /// <summary>
        /// Быстрый экспорт текущего документа
        /// </summary>
        public static async Task<ExportResult> QuickExportCurrentDocument(ExternalCommandData commandData, IProgress<string> progress = null)
        {
            try
            {
                var document = commandData.Application.ActiveUIDocument.Document;
                var modelName = document.Title ?? "CurrentDocument";
                
                if (progress != null)
                    progress.Report($"Быстрый экспорт документа: {modelName}");

                var exporterService = new OptimizedRevitExporterService();
                var result = await exporterService.ExportDocumentToCsvAsync(document, modelName, null, progress);
                
                exporterService.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report($"❌ Ошибка быстрого экспорта: {ex.Message}");
                
                return new ExportResult
                {
                    Success = false,
                    ModelName = "CurrentDocument",
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// Экспорт выбранных файлов
        /// </summary>
        public static async Task<List<ExportResult>> ExportSelectedFiles(IProgress<string> progress = null)
        {
            try
            {
                // Диалог выбора файлов
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Выберите файлы Revit для экспорта",
                    Filter = "Revit Files (*.rvt)|*.rvt|All Files (*.*)|*.*",
                    Multiselect = true,
                    CheckFileExists = true
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    if (progress != null)
                        progress.Report("Экспорт отменен пользователем");
                    return new List<ExportResult>();
                }

                var selectedFiles = openFileDialog.FileNames.ToList();
                
                if (progress != null)
                    progress.Report($"Выбрано {selectedFiles.Count} файлов для экспорта");

                // Диалог выбора папки для сохранения
                var folderDialog = new FolderBrowserDialog
                {
                    Description = "Выберите папку для сохранения CSV файлов",
                    ShowNewFolderButton = true
                };

                string outputPath = null;
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    outputPath = folderDialog.SelectedPath;
                }

                // Экспортируем файлы
                var exporterService = new OptimizedRevitExporterService();
                var results = await exporterService.ExportMultipleFilesAsync(selectedFiles, outputPath, progress);
                
                exporterService.Dispose();
                return results;
            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report($"❌ Ошибка экспорта выбранных файлов: {ex.Message}");
                
                return new List<ExportResult>
                {
                    new ExportResult
                    {
                        Success = false,
                        ModelName = "MultipleFiles",
                        ErrorMessage = ex.Message,
                        Exception = ex
                    }
                };
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
                string logPath = @"C:\Users\dimag\AppData\Roaming\CivilX\DataViewer\optimized_dataviewer_log.txt";
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

    /// <summary>
    /// Оптимизированное главное окно с улучшенной производительностью
    /// </summary>
    public class OptimizedMainWindow : System.Windows.Window
    {
        private readonly OptimizedRevitExporterService _exporterService;
        private readonly Autodesk.Revit.ApplicationServices.Application _revitApp;

        public OptimizedMainWindow(Autodesk.Revit.ApplicationServices.Application revitApp, OptimizedRevitExporterService exporterService)
        {
            _revitApp = revitApp;
            _exporterService = exporterService;
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "Optimized Revit DataViewer - Высокая производительность";
            Width = 800;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var grid = new System.Windows.Controls.Grid();
            
            // Создаем интерфейс с кнопками для быстрого экспорта
            var stackPanel = new System.Windows.Controls.StackPanel
            {
                Margin = new Thickness(20),
                Orientation = System.Windows.Controls.Orientation.Vertical
            };

            var titleLabel = new System.Windows.Controls.Label
            {
                Content = "🚀 Optimized Revit DataViewer",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var descriptionLabel = new System.Windows.Controls.Label
            {
                Content = "Оптимизированная версия с приоритетом на качество и производительность",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30)
            };

            // Кнопка быстрого экспорта текущего документа
            var quickExportButton = new System.Windows.Controls.Button
            {
                Content = "⚡ Быстрый экспорт текущего документа",
                Height = 40,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14
            };
            quickExportButton.Click += QuickExportButton_Click;

            // Кнопка экспорта выбранных файлов
            var selectFilesButton = new System.Windows.Controls.Button
            {
                Content = "📁 Экспорт выбранных файлов",
                Height = 40,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14
            };
            selectFilesButton.Click += SelectFilesButton_Click;

            // Кнопка открытия полного интерфейса
            var fullInterfaceButton = new System.Windows.Controls.Button
            {
                Content = "🖥️ Открыть полный интерфейс",
                Height = 40,
                Margin = new Thickness(0, 0, 0, 20),
                FontSize = 14
            };
            fullInterfaceButton.Click += FullInterfaceButton_Click;

            // Область для отображения прогресса
            var progressTextBlock = new System.Windows.Controls.TextBlock
            {
                Name = "ProgressTextBlock",
                Text = "Готов к работе",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 0),
                FontSize = 12,
                Height = 200,
                VerticalAlignment = VerticalAlignment.Top
            };

            var scrollViewer = new System.Windows.Controls.ScrollViewer
            {
                Content = progressTextBlock,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
            };

            stackPanel.Children.Add(titleLabel);
            stackPanel.Children.Add(descriptionLabel);
            stackPanel.Children.Add(quickExportButton);
            stackPanel.Children.Add(selectFilesButton);
            stackPanel.Children.Add(fullInterfaceButton);
            stackPanel.Children.Add(scrollViewer);

            grid.Children.Add(stackPanel);
            Content = grid;
        }

        private async void QuickExportButton_Click(object sender, RoutedEventArgs e)
        {
            var progressTextBlock = FindName("ProgressTextBlock") as System.Windows.Controls.TextBlock;
            
            try
            {
                progressTextBlock.Text = "Начинаем быстрый экспорт текущего документа...\n";
                
                var progress = new Progress<string>(message => 
                {
                    progressTextBlock.Text += message + "\n";
                });

                var result = await OptimizedExportCommand.QuickExportCurrentDocument(
                    new ExternalCommandData(null, null), progress);

                if (result.Success)
                {
                    progressTextBlock.Text += $"\n✅ Экспорт завершен успешно!\n{result.Statistics}\n";
                }
                else
                {
                    progressTextBlock.Text += $"\n❌ Ошибка экспорта: {result.ErrorMessage}\n";
                }
            }
            catch (Exception ex)
            {
                progressTextBlock.Text += $"\n❌ Критическая ошибка: {ex.Message}\n";
            }
        }

        private async void SelectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var progressTextBlock = FindName("ProgressTextBlock") as System.Windows.Controls.TextBlock;
            
            try
            {
                progressTextBlock.Text = "Выбираем файлы для экспорта...\n";
                
                var progress = new Progress<string>(message => 
                {
                    progressTextBlock.Text += message + "\n";
                });

                var results = await OptimizedExportCommand.ExportSelectedFiles(progress);

                var successCount = results.Count(r => r.Success);
                progressTextBlock.Text += $"\n📊 Результат: {successCount} из {results.Count} файлов экспортированы успешно\n";
                
                foreach (var result in results)
                {
                    progressTextBlock.Text += $"{result}\n";
                }
            }
            catch (Exception ex)
            {
                progressTextBlock.Text += $"\n❌ Критическая ошибка: {ex.Message}\n";
            }
        }

        private void FullInterfaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем полный интерфейс
                var fullWindow = new Views.MainWindow(_revitApp);
                fullWindow.Show();
                
                // Закрываем текущее окно
                Close();
            }
            catch (Exception ex)
            {
                var progressTextBlock = FindName("ProgressTextBlock") as System.Windows.Controls.TextBlock;
                progressTextBlock.Text += $"\n❌ Ошибка открытия полного интерфейса: {ex.Message}\n";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _exporterService?.Dispose();
            base.OnClosed(e);
        }
    }
}






