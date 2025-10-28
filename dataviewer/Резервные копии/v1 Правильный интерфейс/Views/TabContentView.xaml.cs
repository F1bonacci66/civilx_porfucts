using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Views
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
            SourceFileText.Text = _tab.ModelsFolder ?? "Не выбран";
            OutputPathText.Text = "Документы\\RevitExporter";
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
                _tab.ModelsFolder = dialog.FileName;
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
                var fileName = $"Выгрузка данных с модели {Path.GetFileNameWithoutExtension(_tab.ModelsFolder)} {_tab.Name}.csv";
                var outputPath = Path.Combine(dialog.SelectedPath, fileName);
                UpdateDisplay();
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_tab.ModelsFolder))
            {
                MessageBox.Show("Выберите исходный Revit файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
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

