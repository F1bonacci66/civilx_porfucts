using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RevitExporterAddin.TestWpfApp
{
    public partial class TabContentView : UserControl
    {
        private ExportTab _tab;
        private ExportView _exportView;

        public TabContentView(ExportTab tab, ExportView exportView)
        {
            InitializeComponent();
            _tab = tab;
            _exportView = exportView;
            
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            SourceFileText.Text = _tab.RevitFilePath ?? "Не выбран";
            OutputPathText.Text = _tab.OutputPath ?? "Не указан";
        }

        private void BrowseSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите Revit файл",
                Filter = "Revit файлы (*.rvt)|*.rvt|Все файлы (*.*)|*.*",
                DefaultExt = "rvt"
            };

            if (dialog.ShowDialog() == true)
            {
                _tab.RevitFilePath = dialog.FileName;
                UpdateDisplay();
            }
        }

        private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку для сохранения результатов",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileName = $"Выгрузка данных с модели {Path.GetFileNameWithoutExtension(_tab.RevitFilePath)} {_tab.Name}.csv";
                _tab.OutputPath = Path.Combine(dialog.SelectedPath, fileName);
                UpdateDisplay();
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_tab.RevitFilePath))
            {
                MessageBox.Show("Выберите исходный Revit файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_tab.OutputPath))
            {
                MessageBox.Show("Укажите путь для сохранения результатов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Создаем папки если их нет
                var outputDir = Path.GetDirectoryName(_tab.OutputPath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Здесь будет логика экспорта
                MessageBox.Show("Экспорт начат! В реальном приложении здесь будет происходить выгрузка данных из Revit.", 
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
