using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using RevitExporterAddin.Models;
using RevitExporterAddin.Services;
using Autodesk.Revit.DB.Structure;

namespace RevitExporterAddin
{
    public partial class MainWindow : Window
    {
        private readonly Autodesk.Revit.ApplicationServices.Application _revitApp;
        private Document _revitDocument;
        private string _outputFolder;
        private readonly RevitDataReader _dataReader;
        private readonly RevitExporter _exporter;

        public MainWindow(Autodesk.Revit.ApplicationServices.Application revitApp)
        {
            InitializeComponent();
            
            _revitApp = revitApp;
            _dataReader = new RevitDataReader();
            _exporter = new RevitExporter();
            
            // Устанавливаем значения по умолчанию
            FilePathTextBox.Text = "Выберите .rvt файл для экспорта";
            _outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            // Логируем информацию о приложении
            LogMessage("✅ Revit приложение загружено");
            LogMessage(string.Format("📊 Версия Revit: {0}", _revitApp.VersionName));
            LogMessage("💡 Выберите .rvt файл для экспорта");
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
                LogMessage("🔓 Открываем файл Revit...");
                
                // Создаем опции для открытия файла
                var openOptions = new OpenOptions();
                openOptions.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;
                openOptions.Audit = false;
                
                // Открываем файл
                var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                _revitDocument = _revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (_revitDocument != null)
                {
                    LogMessage("✅ Файл успешно открыт");
                    LogMessage(string.Format("📁 Модель: {0}", _revitDocument.Title));
                    LogMessage(string.Format("📂 Путь: {0}", _revitDocument.PathName));
                    
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
    }
}
