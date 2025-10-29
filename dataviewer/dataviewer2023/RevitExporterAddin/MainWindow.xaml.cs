using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using RevitExporterAddin.Services;
using Autodesk.Revit.DB.Structure;
using System.Web.Script.Serialization;
using CivilX.Shared.Auth;
using CivilX.Shared.Models;

namespace RevitExporterAddin
{
    public partial class MainWindow : Window
    {
        // Константа версии Revit для этого плагина
        private const string REVIT_VERSION = "2023";
        
        private readonly Autodesk.Revit.ApplicationServices.Application _revitApp;
        private Document _revitDocument;
        private string _outputFolder;
        private readonly RevitDataReader _dataReader;
        private readonly RevitExporter _exporter;
        private readonly AuthService _authService;

        public MainWindow(Autodesk.Revit.ApplicationServices.Application revitApp)
        {
            InitializeComponent();
            
            _revitApp = revitApp;
            _dataReader = new RevitDataReader();
            _exporter = new RevitExporter();
            _authService = new AuthService();
            
            // Устанавливаем значения по умолчанию
            FilePathTextBox.Text = "Выберите .rvt файл для экспорта";
            _outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            // Логируем информацию о приложении
            LogMessage("✅ Revit приложение загружено");
            LogMessage(string.Format("📊 Версия Revit: {0}", _revitApp.VersionName));
            
            // Проверяем активацию продукта
            CheckProductActivation();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Выберите .rvt файл для экспорта";
                    dialog.Filter = "Revit файлы (*.rvt)|*.rvt|Все файлы (*.*)|*.*";
                    dialog.FilterIndex = 1;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var filePath = dialog.FileName;
                        FilePathTextBox.Text = filePath;
                        
                        LogMessage(string.Format("📁 Выбран файл: {0}", filePath));
                        
                        // Пытаемся открыть файл
                        OpenRevitFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(string.Format("❌ Ошибка выбора файла: {0}", ex.Message));
                System.Windows.MessageBox.Show(string.Format("Ошибка выбора файла:\n{0}", ex.Message), 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRevitFile(string filePath)
        {
            try
            {
                LogMessage("🔓 Открываем файл Revit с оптимизацией...");
                
                // Определяем тип модели и выбираем оптимальную стратегию
                _revitDocument = OpenModelWithOptimization(filePath);
                
                if (_revitDocument != null)
                {
                    LogMessage("✅ Файл успешно открыт");
                    LogMessage(string.Format("📁 Модель: {0}", _revitDocument.Title));
                    LogMessage(string.Format("📂 Путь: {0}", _revitDocument.PathName));
                    
                    // Показываем информацию о типе модели
                    if (_revitDocument.IsWorkshared)
                    {
                        LogMessage("🏢 Тип: Хранилище (отсоединено для оптимизации)");
                    }
                    else
                    {
                        LogMessage("🏠 Тип: Локальная модель");
                    }
                    
                    // Показываем информацию о единицах измерения
                    try
                    {
                        var units = _revitDocument.GetUnits();
                        var lengthUnit = units.GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
                        var areaUnit = units.GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
                        
                        LogMessage(string.Format("📏 Единицы длины: {0}", GetUnitDisplayName(lengthUnit)));
                        LogMessage(string.Format("📐 Единицы площади: {0}", GetUnitDisplayName(areaUnit)));
                    }
                    catch
                    {
                        LogMessage("⚠️ Не удалось получить информацию о единицах измерения");
                    }
                    
                    LogMessage("🚀 Готов к экспорту!");
                }
                else
                {
                    LogMessage("❌ Не удалось открыть файл");
                }
            }
            catch (Exception ex)
            {
                LogMessage(string.Format("❌ Ошибка открытия файла: {0}", ex.Message));
                System.Windows.MessageBox.Show(string.Format("Ошибка открытия файла:\n{0}", ex.Message), 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Autodesk.Revit.DB.Document OpenModelWithOptimization(string filePath)
        {
            try
            {
                // Сначала пробуем быстрое открытие для определения типа модели
                Autodesk.Revit.DB.Document tempDoc = null;
                bool isWorkshared = false;
                
                try
                {
                    // Быстрое открытие для проверки типа
                    tempDoc = _revitApp.OpenDocumentFile(filePath);
                    if (tempDoc != null)
                    {
                        isWorkshared = tempDoc.IsWorkshared;
                        
                        // Если это хранилище, закрываем и открываем с оптимизацией
                        if (isWorkshared)
                        {
                            tempDoc.Close(false);
                            return OpenWorksharedModel(filePath);
                        }
                        else
                        {
                            // Локальная модель - возвращаем как есть
                            return tempDoc;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Если быстрое открытие не сработало, пробуем с OpenOptions
                    if (tempDoc != null)
                    {
                        tempDoc.Close(false);
                    }
                    return OpenWithFallbackOptions(filePath);
                }
                
                return tempDoc;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка оптимизированного открытия модели: {ex.Message}", ex);
            }
        }

        private Autodesk.Revit.DB.Document OpenWorksharedModel(string filePath)
        {
            try
            {
                // Для хранилища - отсоединяем с сохранением рабочих наборов
                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DetachAndPreserveWorksets;
                openOptions.Audit = false;
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                // Настраиваем рабочие наборы для оптимизации
                var worksetConfig = new Autodesk.Revit.DB.WorksetConfiguration(Autodesk.Revit.DB.WorksetConfigurationOption.CloseAllWorksets);
                openOptions.SetOpenWorksetsConfiguration(worksetConfig);
                
                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                var document = _revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (document != null)
                {
                    return document;
                }
                
                // Fallback: пробуем без отсоединения
                return OpenWithFallbackOptions(filePath);
            }
            catch (Exception ex)
            {
                // Fallback к стандартному открытию
                return OpenWithFallbackOptions(filePath);
            }
        }

        private Autodesk.Revit.DB.Document OpenWithFallbackOptions(string filePath)
        {
            try
            {
                // Способ 1: С OpenOptions для локальной модели
                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DoNotDetach;
                openOptions.Audit = false;
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                var document = _revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (document != null)
                {
                    return document;
                }
            }
            catch (Exception optionsEx)
            {
                try
                {
                    // Способ 2: Простое открытие
                    var document = _revitApp.OpenDocumentFile(filePath);
                    if (document != null)
                    {
                        return document;
                    }
                }
                catch (Exception simpleEx)
                {
                    throw new Exception($"Все способы открытия файла не сработали. OpenOptions: {optionsEx.Message}, Простой: {simpleEx.Message}");
                }
            }
            
            return null;
        }

        private void SelectOutputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Выберите папку для сохранения CSV файла";
                    dialog.ShowNewFolderButton = true;
                    dialog.SelectedPath = _outputFolder;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _outputFolder = dialog.SelectedPath;
                        LogMessage(string.Format("📁 Папка для сохранения: {0}", _outputFolder));
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(string.Format("❌ Ошибка выбора папки: {0}", ex.Message));
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportButton.IsEnabled = false;
                LogMessage("🚀 Начинаем экспорт...");

                // Проверяем настройки
                if (_revitDocument == null)
                {
                    LogMessage("❌ Не выбран файл Revit для экспорта");
                    System.Windows.MessageBox.Show("Сначала выберите .rvt файл для экспорта!", 
                                  "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_outputFolder))
                {
                    LogMessage("❌ Не выбрана папка для сохранения");
                    return;
                }

                if (string.IsNullOrEmpty(FileNameTextBox.Text))
                {
                    LogMessage("❌ Не указано имя файла");
                    return;
                }

                // Выполняем экспорт асинхронно
                await ExportDataAsync();
            }
            catch (Exception ex)
            {
                LogMessage(string.Format("❌ Критическая ошибка: {0}", ex.Message));
                LogMessage(string.Format("📋 Детали: {0}", ex.StackTrace));
            }
            finally
            {
                ExportButton.IsEnabled = true;
            }
        }

        private async Task ExportDataAsync()
        {
            try
            {
                LogMessage("📖 Читаем данные из Revit документа...");

                // Создаем прогресс для логирования
                var progress = new Progress<string>(message => LogMessage(message));

                // Извлекаем данные
                var data = await Task.Run(() => 
                    _dataReader.ExtractElementsFromDocument(_revitDocument, _revitDocument.Title, progress));

                if (data == null || data.Count == 0)
                {
                    LogMessage("❌ Не удалось извлечь данные из документа");
                    return;
                }

                LogMessage(string.Format("✅ Извлечено {0} параметров из {1} элементов", data.Count, data.Select(d => d.ElementId).Distinct().Count()));

                // Создаем CSV
                LogMessage("📝 Создаем CSV файл...");
                var csvContent = _exporter.ExportToCsv(data, _revitDocument.Title);

                // Сохраняем файл
                var fileName = string.Format("{0}_{1:yyyy-MM-dd_HH-mm-ss}.csv", FileNameTextBox.Text, DateTime.Now);
                var fullPath = Path.Combine(_outputFolder, fileName);
                
                await Task.Run(() => File.WriteAllText(fullPath, csvContent, System.Text.Encoding.UTF8));

                LogMessage(string.Format("💾 Файл сохранен: {0}", fullPath));
                LogMessage("📊 Статистика экспорта:");
                LogMessage(string.Format("   • Модель: {0}", _revitDocument.Title));
                LogMessage(string.Format("   • Элементов: {0}", data.Select(d => d.ElementId).Distinct().Count()));
                LogMessage(string.Format("   • Параметров: {0}", data.Count));
                LogMessage(string.Format("   • Размер файла: {0:F1} KB", new FileInfo(fullPath).Length / 1024.0));

                System.Windows.MessageBox.Show(string.Format("Экспорт завершен успешно!\n\nФайл сохранен в:\n{0}", fullPath), 
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Открываем папку с файлом
                System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", fullPath));
            }
            catch (Exception ex)
            {
                LogMessage(string.Format("❌ Ошибка экспорта: {0}", ex.Message));
                LogMessage(string.Format("📋 Детали: {0}", ex.StackTrace));
                System.Windows.MessageBox.Show(string.Format("Ошибка экспорта:\n{0}", ex.Message), 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
            LogMessage("🧹 Лог очищен");
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = string.Format("[{0}] {1}", timestamp, message);
            
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(logEntry + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Закрываем документ, если он был открыт
                if (_revitDocument != null && !_revitDocument.IsReadOnly)
                {
                    _revitDocument.Close(false);
                    LogMessage("🔒 Документ Revit закрыт");
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки при закрытии
            }
            
            base.OnClosed(e);
        }

        private string GetUnitDisplayName(ForgeTypeId unitTypeId)
        {
            try
            {
                if (unitTypeId == UnitTypeId.Millimeters) return "мм";
                if (unitTypeId == UnitTypeId.Centimeters) return "см";
                if (unitTypeId == UnitTypeId.Meters) return "м";
                if (unitTypeId == UnitTypeId.Feet) return "фут";
                if (unitTypeId == UnitTypeId.Inches) return "дюйм";
                if (unitTypeId == UnitTypeId.SquareMillimeters) return "мм²";
                if (unitTypeId == UnitTypeId.SquareCentimeters) return "см²";
                if (unitTypeId == UnitTypeId.SquareMeters) return "м²";
                if (unitTypeId == UnitTypeId.SquareFeet) return "фут²";
                if (unitTypeId == UnitTypeId.SquareInches) return "дюйм²";
                if (unitTypeId == UnitTypeId.CubicMillimeters) return "мм³";
                if (unitTypeId == UnitTypeId.CubicCentimeters) return "см³";
                if (unitTypeId == UnitTypeId.CubicMeters) return "м³";
                if (unitTypeId == UnitTypeId.CubicFeet) return "фут³";
                if (unitTypeId == UnitTypeId.CubicInches) return "дюйм³";
                if (unitTypeId == UnitTypeId.Degrees) return "градусы";
                if (unitTypeId == UnitTypeId.Radians) return "радианы";
                
                return unitTypeId.ToString();
            }
            catch
            {
                return "неизвестно";
            }
        }

        private async void CheckProductActivation()
        {
            try
            {
                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    ShowActivationRequired("Требуется авторизация для использования DataViewer");
                    return;
                }

                // Проверяем активацию продукта DataHub
                var isActivated = await CheckDataHubActivation(token);
                if (!isActivated)
                {
                    ShowActivationRequired("DataHub не активирован. Активируйте продукт в плагине авторизации.");
                    return;
                }

                LogMessage("💡 Выберите .rvt файл для экспорта");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка проверки активации: {ex.Message}");
                ShowActivationRequired("Ошибка проверки активации. Обратитесь к администратору.");
            }
        }

        private async Task<bool> CheckDataHubActivation(string token)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    var response = await client.GetAsync("http://civilx.ru/auth-api.php/api/user-products");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = new JavaScriptSerializer().Deserialize<UserProductsResponse>(responseContent);
                        var dataHubProduct = result.Products?.FirstOrDefault(p => 
                            p.ProductName?.ToLower() == "datahub" || 
                            p.ProductName?.ToLower() == "dataviewer");
                        
                        if (dataHubProduct != null)
                        {
                            var status = dataHubProduct.ActivationStatus?.ToLower();
                            return status == "active" || status == "activated";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка проверки активации DataHub: {ex.Message}");
            }
            
            return false;
        }

        private void ShowActivationRequired(string message)
        {
            LogMessage($"🔒 {message}");
            
            // Блокируем интерфейс
            SelectFileButton.IsEnabled = false;
            SelectOutputFolderButton.IsEnabled = false;
            ExportButton.IsEnabled = false;
            
            // Показываем сообщение пользователю
            System.Windows.MessageBox.Show(message + "\n\nОткройте плагин авторизации (Настройки) для активации продукта.", 
                          "Требуется активация", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
