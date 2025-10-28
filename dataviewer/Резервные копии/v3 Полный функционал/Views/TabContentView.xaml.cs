using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
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
            ModelsPathText.Text = _tab.ModelsFolder ?? "Не выбран";
            ResultsPathText.Text = GetResultsFolderPath();
        }

        private string GetResultsFolderPath()
        {
            // Получаем путь к папке вкладки (где будут результаты)
            var projectPath = _exportView.GetProjectPath();
            var exportPath = Path.Combine(projectPath, _exportView.GetExportName());
            var tabPath = Path.Combine(exportPath, _tab.Name);
            return tabPath;
        }

        private void BrowseModelsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку с моделями Revit",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _tab.ModelsFolder = dialog.SelectedPath;
                CopyModelsToTabFolder();
                UpdateDisplay();
            }
        }

        private void ViewResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var resultsPath = GetResultsFolderPath();
            
            // Создаем папку, если она не существует
            if (!Directory.Exists(resultsPath))
            {
                Directory.CreateDirectory(resultsPath);
            }
            
            // Открываем папку в проводнике Windows
            Process.Start("explorer.exe", resultsPath);
        }

        private void CopyModelsToTabFolder()
        {
            if (string.IsNullOrEmpty(_tab.ModelsFolder) || !Directory.Exists(_tab.ModelsFolder))
                return;

            var resultsPath = GetResultsFolderPath();
            var modelsPath = Path.Combine(resultsPath, "Модели");

            try
            {
                // Удаляем существующую папку "Модели", если она есть
                if (Directory.Exists(modelsPath))
                {
                    Directory.Delete(modelsPath, true);
                }

                // Создаем новую папку "Модели"
                Directory.CreateDirectory(modelsPath);

                // Копируем все .rvt файлы с сохранением иерархии
                CopyRvtFilesRecursively(_tab.ModelsFolder, modelsPath);

                MessageBox.Show($"Модели успешно скопированы в папку:\n{modelsPath}", 
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании моделей:\n{ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyRvtFilesRecursively(string sourceDir, string targetDir)
        {
            // Создаем папку назначения, если она не существует
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Копируем все .rvt файлы из текущей папки
            var rvtFiles = Directory.GetFiles(sourceDir, "*.rvt");
            foreach (var file in rvtFiles)
            {
                var fileName = Path.GetFileName(file);
                var targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }

            // Рекурсивно обрабатываем подпапки
            var subDirs = Directory.GetDirectories(sourceDir);
            foreach (var subDir in subDirs)
            {
                var dirName = Path.GetFileName(subDir);
                var targetSubDir = Path.Combine(targetDir, dirName);
                CopyRvtFilesRecursively(subDir, targetSubDir);
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


