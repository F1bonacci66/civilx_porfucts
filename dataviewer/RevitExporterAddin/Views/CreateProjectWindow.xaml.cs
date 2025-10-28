using System;
using System.IO;
using System.Windows;

namespace RevitExporterAddin.Views
{
    public partial class CreateProjectWindow : Window
    {
        public string ProjectName { get; private set; }
        public string ProjectPath { get; private set; }

        public CreateProjectWindow()
        {
            InitializeComponent();
            ProjectNameTextBox.Focus();
            ProjectNameTextBox.KeyDown += (s, e) => 
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    CreateButton_Click(s, e);
                }
            };
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку для сохранения проекта",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                MessageBox.Show("Выберите путь для сохранения проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                MessageBox.Show("Введите название проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var projectName = ProjectNameTextBox.Text.Trim();
            var basePath = PathTextBox.Text.Trim();
            var fullProjectPath = Path.Combine(basePath, projectName);

            // Проверяем, не существует ли уже папка с таким именем
            if (Directory.Exists(fullProjectPath))
            {
                MessageBox.Show($"Папка с названием '{projectName}' уже существует в указанном пути", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Создаем папку проекта
                Directory.CreateDirectory(fullProjectPath);

                ProjectName = projectName;
                ProjectPath = fullProjectPath;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании папки проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

