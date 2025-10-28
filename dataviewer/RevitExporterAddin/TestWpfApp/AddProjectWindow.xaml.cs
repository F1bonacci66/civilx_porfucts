using System.Windows;
using Microsoft.Win32;

namespace RevitExporterAddin.TestWpfApp
{
    public partial class AddProjectWindow : Window
    {
        public string ProjectName { get; private set; }
        public string ProjectPath { get; private set; }

        public AddProjectWindow()
        {
            InitializeComponent();
            ProjectPathTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку для проекта",
                ShowNewFolderButton = true,
                SelectedPath = ProjectPathTextBox.Text
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProjectPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                MessageBox.Show("Введите название проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ProjectPathTextBox.Text))
            {
                MessageBox.Show("Выберите путь к проекту", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectName = ProjectNameTextBox.Text.Trim();
            ProjectPath = ProjectPathTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
